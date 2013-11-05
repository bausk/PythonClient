using System;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;


namespace DgnPurger
{
    public class Commands
    {
        const string dgnLsDefName = "DGNLSDEF";
        const string dgnLsDictName = "ACAD_DGNLINESTYLECOMP";

        public struct ads_name
        {
            public IntPtr a;
            public IntPtr b;
        };

        [DllImport("acdb19.dll",
          CharSet = CharSet.Unicode,
          CallingConvention = CallingConvention.Cdecl,
          EntryPoint = "acdbHandEnt")]
        public static extern int acdbHandEnt(string h, ref ads_name n);

        [CommandMethod("SE")]
        static public void SwapEntities()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            PromptEntityResult per = ed.GetEntity("\nSelect first entity: ");
            if (per.Status != PromptStatus.OK)
                return;

            ObjectId firstId = per.ObjectId;

            per = ed.GetEntity("\nSelect second entity: ");
            if (per.Status != PromptStatus.OK)
                return;

            ObjectId secondId = per.ObjectId;
            Transaction tr = doc.Database.TransactionManager.StartTransaction();

            DBObject firstObj = null;

            using (tr)
            {
                firstObj =
                    tr.GetObject(firstId, OpenMode.ForRead);

                DBObject secondObj = null;

                secondObj =
                  tr.GetObject(secondId, OpenMode.ForRead);

                PromptKeywordOptions pko =
                  new PromptKeywordOptions(
                    "\nSwap their identities?"
                  );
                pko.AllowNone = true;
                pko.Keywords.Add("Yes");
                pko.Keywords.Add("No");
                pko.Keywords.Default = "No";

                PromptResult pkr =
                  ed.GetKeywords(pko);

                if (pkr.StringResult == "Yes")
                {
                    try
                    {
                        firstObj.UpgradeOpen();
                        firstObj.SwapIdWith(secondId, true, true);
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        ed.WriteMessage(
                          "\nCould not swap identities: " + ex.Message
                        );
                    }
                }
                tr.Commit();
            }
        }

        [CommandMethod("DGNPURGE")]
        public void PurgeDgnLinetypes()
        {
            var doc =
              Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            using (var tr = doc.TransactionManager.StartTransaction())
            {
                // Start by getting all the "complex" DGN linetypes
                // from the linetype table

                var linetypes = CollectComplexLinetypeIds(db, tr);

                // Store a count before we start removing the ones
                // that are referenced

                var ltcnt = linetypes.Count;

                // Remove any from the "to remove" list that need to be
                // kept (as they have references from objects other
                // than anonymous blocks)

                var ltsToKeep =
                  PurgeLinetypesReferencedNotByAnonBlocks(db, tr, linetypes);

                // Now we collect the DGN stroke entries from the NOD

                var strokes = CollectStrokeIds(db, tr);

                // Store a count before we start removing the ones
                // that are referenced

                var strkcnt = strokes.Count;

                // Open up each of the "keeper" linetypes, and go through
                // their data, removing any NOD entries from the "to
                // remove" list that are referenced

                PurgeStrokesReferencedByLinetypes(tr, ltsToKeep, strokes);

                // Erase each of the NOD entries that are safe to remove

                foreach (ObjectId id in strokes)
                {
                    var obj = tr.GetObject(id, OpenMode.ForWrite);
                    obj.Erase();
                }

                // And the same for the complex linetypes

                foreach (ObjectId id in linetypes)
                {
                    var obj = tr.GetObject(id, OpenMode.ForWrite);
                    obj.Erase();
                }

                // Remove the DGN stroke dictionary from the NOD if empty

                var nod =
                  (DBDictionary)tr.GetObject(
                    db.NamedObjectsDictionaryId, OpenMode.ForRead
                  );

                ed.WriteMessage(
                  "\nPurged {0} unreferenced complex linetype records" +
                  " (of {1}).",
                  linetypes.Count, ltcnt
                );

                ed.WriteMessage(
                  "\nPurged {0} unreferenced strokes (of {1}).",
                  strokes.Count, strkcnt
                );

                if (nod.Contains(dgnLsDictName))
                {
                    var dgnLsDict =
                      (DBDictionary)tr.GetObject(
                        (ObjectId)nod[dgnLsDictName],
                        OpenMode.ForRead
                      );

                    if (dgnLsDict.Count == 0)
                    {
                        dgnLsDict.UpgradeOpen();
                        dgnLsDict.Erase();

                        ed.WriteMessage(
                          "\nRemoved the empty DGN linetype stroke dictionary."
                        );
                    }
                }

                tr.Commit();
            }
        }

        // Collect the complex DGN linetypes from the linetype table

        private static ObjectIdCollection CollectComplexLinetypeIds(
          Database db, Transaction tr
        )
        {
            var ids = new ObjectIdCollection();

            var lt =
              (LinetypeTable)tr.GetObject(
                db.LinetypeTableId, OpenMode.ForRead
              );
            foreach (var ltId in lt)
            {
                // Complex DGN linetypes have an extension dictionary
                // with a certain record inside

                var obj = tr.GetObject(ltId, OpenMode.ForRead);
                if (obj.ExtensionDictionary != ObjectId.Null)
                {
                    var exd =
                      (DBDictionary)tr.GetObject(
                        obj.ExtensionDictionary, OpenMode.ForRead
                      );
                    if (exd.Contains(dgnLsDefName))
                    {
                        ids.Add(ltId);
                    }
                }
            }
            return ids;
        }

        // Collect the DGN stroke entries from the NOD

        private static ObjectIdCollection CollectStrokeIds(
          Database db, Transaction tr
        )
        {
            var ids = new ObjectIdCollection();

            var nod =
              (DBDictionary)tr.GetObject(
                db.NamedObjectsDictionaryId, OpenMode.ForRead
              );

            // Strokes are stored in a particular dictionary

            if (nod.Contains(dgnLsDictName))
            {
                var dgnDict =
                  (DBDictionary)tr.GetObject(
                    (ObjectId)nod[dgnLsDictName],
                    OpenMode.ForRead
                  );

                foreach (var item in dgnDict)
                {
                    ids.Add(item.Value);
                }
            }

            return ids;
        }

        // Remove the linetype IDs that have references from objects
        // other than anonymous blocks from the list passed in,
        // returning the ones removed in a separate list

        private static ObjectIdCollection
          PurgeLinetypesReferencedNotByAnonBlocks(
            Database db, Transaction tr, ObjectIdCollection ids
          )
        {
            var keepers = new ObjectIdCollection();

            // To determine the references from objects in the database,
            // we need to open every object. One reasonably efficient way
            // to do so is to loop through all handles in the possible
            // handle space for this drawing (starting with 1, ending with
            // the value of "HANDSEED") and open each object we can

            // Get the last handle in the db

            var handseed = db.Handseed;

            // Copy the handseed total into an efficient raw datatype

            var handseedTotal = handseed.Value;

            // Loop from 1 to the last handle (could be a big loop)

            var ename = new ads_name();

            for (long i = 1; i < handseedTotal; i++)
            {
                // Get a handle from the counter

                var handle = Convert.ToString(i, 16);

                // Get the entity name using acdbHandEnt()

                var res = acdbHandEnt(handle, ref ename);

                if (res != 5100) // RTNORM
                    continue;

                // Convert the entity name to an ObjectId

                var id = new ObjectId(ename.a);

                // Open the object and check its linetype

                var obj = tr.GetObject(id, OpenMode.ForRead, true);
                var ent = obj as Entity;
                if (ent != null && !ent.IsErased)
                {
                    if (ids.Contains(ent.LinetypeId))
                    {
                        // If the owner does not belong to an anonymous
                        // block, then we take it seriously as a reference

                        var owner =
                          (BlockTableRecord)tr.GetObject(
                            ent.OwnerId, OpenMode.ForRead
                          );
                        if (
                          !owner.Name.StartsWith("*") ||
                          owner.Name.ToUpper() == BlockTableRecord.ModelSpace ||
                          owner.Name.ToUpper().StartsWith(
                            BlockTableRecord.PaperSpace
                          )
                        )
                        {
                            // Move the linetype ID from the "to remove" list
                            // to the "to keep" list

                            ids.Remove(ent.LinetypeId);
                            keepers.Add(ent.LinetypeId);
                        }
                    }
                }
            }
            return keepers;
        }

        // Remove the stroke objects that have references from
        // complex linetypes (or from other stroke objects, as we
        // recurse) from the list passed in

        private static void PurgeStrokesReferencedByLinetypes(
          Transaction tr,
          ObjectIdCollection tokeep,
          ObjectIdCollection nodtoremove
        )
        {
            foreach (ObjectId id in tokeep)
            {
                PurgeStrokesReferencedByObject(tr, nodtoremove, id);
            }
        }

        // Remove the stroke objects that have references from this
        // particular complex linetype or stroke object from the list
        // passed in

        private static void PurgeStrokesReferencedByObject(
          Transaction tr, ObjectIdCollection nodIds, ObjectId id
        )
        {
            var obj = tr.GetObject(id, OpenMode.ForRead);
            if (obj.ExtensionDictionary != ObjectId.Null)
            {
                // Get the extension dictionary

                var exd =
                  (DBDictionary)tr.GetObject(
                    obj.ExtensionDictionary, OpenMode.ForRead
                  );

                // And the "DGN Linestyle Definition" object

                if (exd.Contains(dgnLsDefName))
                {
                    var lsdef =
                      tr.GetObject(
                        exd.GetAt(dgnLsDefName), OpenMode.ForRead
                      );

                    // Use a DWG filer to extract the references

                    var refFiler = new ReferenceFiler();
                    lsdef.DwgOut(refFiler);

                    // Loop through the references and remove any from the
                    // list passed in

                    foreach (ObjectId refid in refFiler.HardPointerIds)
                    {
                        if (nodIds.Contains(refid))
                        {
                            nodIds.Remove(refid);
                        }

                        // We need to recurse, as linetype strokes can reference
                        // other linetype strokes

                        PurgeStrokesReferencedByObject(tr, nodIds, refid);
                    }
                }
            }
        }
    }
}