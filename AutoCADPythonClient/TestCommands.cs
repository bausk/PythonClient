using Autodesk.AutoCAD.Runtime;
using Draftsocket;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(TestCommands.TestCommands))]
namespace TestCommands
{

    public class TestCommands
    {

        [CommandMethod("SWAPENT")]
        static public void SwapEntities()
        {
            Document doc =
              Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            PromptEntityResult per =
              ed.GetEntity("\nSelect first entity: ");
            if (per.Status != PromptStatus.OK)
                return;

            ObjectId firstId = per.ObjectId;

            per = ed.GetEntity("\nSelect second entity: ");
            if (per.Status != PromptStatus.OK)
                return;

            ObjectId secondId = per.ObjectId;

            Transaction tr =
              doc.Database.TransactionManager.StartTransaction();
            using (tr)
            {
                DBObject firstObj =
                  tr.GetObject(firstId, OpenMode.ForRead);

                DBObject secondObj =
                  tr.GetObject(secondId, OpenMode.ForRead);

                PrintIdentities(firstObj, secondObj);

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

                        PrintIdentities(firstObj, secondObj);
                    }
                    catch (Exception ex)
                    {
                        ed.WriteMessage(
                          "\nCould not swap identities: " + ex.Message
                        );
                    }
                }
                tr.Commit();
            }
        }

        private static void PrintIdentities(
          DBObject first, DBObject second)
        {
            PrintIdentity(first, "First");
            PrintIdentity(second, "Second");
        }

        private static void PrintIdentity(
          DBObject obj, string name)
        {
            Document doc =
              Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            ed.WriteMessage(
              "\n{0} object, of type {1}: " +
              "ObjectId is {2}, " +
              "Handle is {3}.",
              name,
              obj.GetType().Name,
              obj.ObjectId,
              obj.Handle
            );
        }
    }

}