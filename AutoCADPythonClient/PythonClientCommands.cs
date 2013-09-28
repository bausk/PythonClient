// (C) Copyright 2013 by Microsoft 
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(AutoCADPythonClient.PythonCommands))]

namespace AutoCADPythonClient
{

    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class PythonCommands
    {
        // The CommandMethod attribute can be applied to any public  member 
        // function of any public class.
        // The function should take no arguments and return nothing.
        // If the method is an intance member then the enclosing class is 
        // intantiated for each document. If the member is a static member then
        // the enclosing class is NOT intantiated.
        //
        // NOTE: CommandMethod has overloads where you can provide helpid and
        // context menu.

        // Modal Command with localized name
        [CommandMethod("MyGroup", "MyCommand", "MyCommandLocal", CommandFlags.Modal)]
        public void MyCommand() // This method can have any name
        {
            // Put your command code here

        }


        [CommandMethod("PFT", CommandFlags.UsePickSet |
                                  CommandFlags.Redraw |
                                  CommandFlags.Modal)
            ]
        static public void PickFirstTest()
        {
            Document doc =
              Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            try
            {
                PromptSelectionResult selectionRes =
                  ed.SelectImplied();
                // If there's no pickfirst set available...
                if (selectionRes.Status == PromptStatus.Error)
                {
                    // ... ask the user to select entities
                    PromptSelectionOptions selectionOpts =
                      new PromptSelectionOptions();
                    selectionOpts.MessageForAdding =
                      "\nSelect objects to list: ";
                    selectionRes =
                      ed.GetSelection(selectionOpts);
                }
                else
                {
                    // If there was a pickfirst set, clear it
                    ed.SetImpliedSelection(new ObjectId[0]);
                }
                // If the user has not cancelled...
                if (selectionRes.Status == PromptStatus.OK)
                {
                    // ... take the selected objects one by one
                    Transaction tr =
                      doc.TransactionManager.StartTransaction();
                    try
                    {
                        ObjectId[] objIds = selectionRes.Value.GetObjectIds();
                        foreach (ObjectId objId in objIds)
                        {
                            Entity ent =
                              (Entity)tr.GetObject(objId, OpenMode.ForRead);
                            // In this simple case, just dump their properties
                            // to the command-line using list
                            ent.List();
                            ent.Dispose();
                        }
                        // Although no changes were made, use Commit()
                        // as this is much quicker than rolling back
                        tr.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        ed.WriteMessage(ex.Message);
                        tr.Abort();
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                ed.WriteMessage(ex.Message);
            }
        }





        // Modal Command with pickfirst selection
        [CommandMethod("MyGroup", "MyPickFirst", "MyPickFirstLocal", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void MyPickFirst() // This method can have any name
        {
            PromptSelectionResult result = Application.DocumentManager.MdiActiveDocument.Editor.GetSelection();
            if (result.Status == PromptStatus.OK)
            {
                // There are selected entities
                // Put your command using pickfirst set code here

            }
            else
            {
                // There are no selected entities
                // Put your command code here
            }
        }

        // Application Session Command with localized name
        [CommandMethod("MyGroup", "MySessionCmd", "MySessionCmdLocal", CommandFlags.Modal | CommandFlags.Session)]
        public void MySessionCmd() // This method can have any name
        {
            // Put your command code here
        }

        // LispFunction is similar to CommandMethod but it creates a lisp 
        // callable function. Many return types are supported not just string
        // or integer.
        [LispFunction("MyLispFunction", "MyLispFunctionLocal")]
        public int MyLispFunction(ResultBuffer args) // This method can have any name
        {
            // Put your command code here

            // Return a value to the AutoCAD Lisp Interpreter
            return 1;
        }

    }

}
