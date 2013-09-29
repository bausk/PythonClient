// (C) Copyright 2013 by Microsoft 
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using ZeroMQ;

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

        //Test messaging command
        [CommandMethod("STEST", CommandFlags.UsePickSet |
                                  CommandFlags.Redraw |
                                  CommandFlags.Modal)
            ]
        static public void ZeroMQTest()
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



        //THIS IS A REFERENCE PFT COMMAND OVER WHICH WE BUILD THE EXAMPLE ZEROMQ CLIENT
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
        //End of reference function
    }
}
