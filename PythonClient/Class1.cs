using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
//using Microsoft.Scripting.Hosting;

[assembly: CommandClass(typeof(PythonClient.Commands))]

namespace PythonClient
{
    public class Commands
    {
        //private ScriptEngine _engine = null;
        //private ScriptRuntime _runtime = null;
        private dynamic _scope = null;
        //private SimpleLogger _logger = new SimpleLogger();
        private string _owd = null;

        [CommandMethod("DESKEW_IMAGE", CommandFlags.NoHistory)]
        public void DeskewRasterImage()
        {
            const string recBase = "ADSKEW_";

            var doc =
              Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            _owd = Directory.GetCurrentDirectory();

            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var expath = Path.GetDirectoryName(asm.Location) + "\\";
                var pypath = expath + "Python\\";
                var pyfile = pypath + "deskew.py";

                var pr = ed.GetString("\nName of input image");
                if (pr.Status != PromptStatus.OK)
                    return;

                var name = new Uri(pr.StringResult).LocalPath;

                pr = ed.GetString("\nName of output image");
                if (pr.Status != PromptStatus.OK)
                    return;

                var outname = new Uri(pr.StringResult).LocalPath;

                pr = ed.GetString("\nWindow coordinates");
                if (pr.Status != PromptStatus.OK)
                    return;

                var coords = pr.StringResult.Split(",".ToCharArray());
                if (coords.Length != 8)
                {
                    ed.WriteMessage(
                      "\nExpecting 8 coordinate values, received {0}.",
                      coords.Length
                    );
                    return;
                }

                var pdr = ed.GetDouble("\nWidth over height");
                if (pdr.Status != PromptStatus.OK)
                    return;

                var xscale = pdr.Value;

                Directory.SetCurrentDirectory(pypath);

                //dynamic deskmod = UsePythonScript(pyfile);

                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    var dictId =
                        RasterImageDef.GetImageDictionary(db);

                    if (dictId.IsNull)
                    {
                        // Image dictionary doesn't exist, create new

                        dictId =
                            RasterImageDef.CreateImageDictionary(db);
                    }

                    // Open the image dictionary

                    var dict =
                        (DBDictionary)tr.GetObject(
                        dictId,
                        OpenMode.ForRead
                        );

                    // Get a unique record name for our raster image
                    // definition

                    int i = 0;
                    string recName = recBase + i.ToString();

                    while (dict.Contains(recName))
                    {
                        i++;
                        recName = recBase + i.ToString();
                    }

                    var rid = new RasterImageDef();

                    // Set its source image

                    rid.SourceFileName = outname;

                    // Load it

                    rid.Load();
                    dict.UpgradeOpen();

                    ObjectId defId = dict.SetAt(recName, rid);

                    // Let the transaction know

                    tr.AddNewlyCreatedDBObject(rid, true);

                    var ppr =
                        ed.GetPoint("\nFirst corner of de-skewed raster");
                    if (ppr.Status != PromptStatus.OK)
                        return;

                    // Call our jig to define the raster

                    /*var jig =
                        new RectangularRasterJig(
                        defId,
                        ed.CurrentUserCoordinateSystem,
                        ppr.Value,
                        xscale
                        );
                    var prj = ed.Drag(jig);

                    if (prj.Status != PromptStatus.OK)
                    {
                        rid.Erase();
                        return;
                    }*/

                    // Get our entity and add it to the modelspace

                    //var ri = (RasterImage)jig.GetEntity();

                    var bt =
                        (BlockTable)tr.GetObject(
                        db.BlockTableId,
                        OpenMode.ForRead
                        );

                    var btr =
                        (BlockTableRecord)tr.GetObject(
                        bt[BlockTableRecord.ModelSpace],
                        OpenMode.ForWrite
                        );

                    //btr.AppendEntity(ri);
                    //tr.AddNewlyCreatedDBObject(ri, true);

                    // Create a reactor between the RasterImage and the
                    // RasterImageDef to avoid the "unreferenced"
                    // warning in the XRef palette

                    RasterImage.EnableReactors(true);
                    //ri.AssociateRasterDef(rid);

                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage(
                  "\nProblem executing script: {0}", ex.Message
                );
            }
            finally
            {
                Directory.SetCurrentDirectory(_owd);
            }
        }


        private static void AddPathToList(
          string dir, ICollection<string> paths
        )
        {
            if (
              !String.IsNullOrWhiteSpace(dir) &&
              !paths.Contains(dir)
            )
            {
                paths.Add(dir);
            }
        }
    }
}