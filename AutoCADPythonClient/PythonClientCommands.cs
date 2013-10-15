// (C) Copyright 2013 by Microsoft 
//
using System;
using Microsoft.CSharp;
using System.Text;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using ZeroMQ;
using Newtonsoft.Json;
//using System.Runtime.Serialization;
using SocketWrapper;


// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ShadowbinderClient.PythonCommands))]

namespace ShadowbinderClient
{
    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
/*    public class PythonMessage
    {
        public string MessageType { get; set; }
        public string ContentType { get; set; }
        public object Content { get; set; }
    }
 */

    public class PythonCommands
    {
        //SortedList<string, string> _blockNames = null;

        //Selection Set example
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
                            String message = ent.Layer;
                            using (ZmqContext context = ZmqContext.Create())
                            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
                            {
                                client.Connect("tcp://localhost:5556");
                                string request = message;
                                for (int requestNum = 1; requestNum < 3; requestNum++)
                                {
                                    ed.WriteMessage("\nSending layer information to Python try {0}...\n", requestNum);
                                    client.SendTimeout = new TimeSpan(0,0,50);
                                    client.ReceiveTimeout = new TimeSpan(0,0,50);
                                    //JsonConvert.SerializeObject(dict)
                                    client.Send(request, Encoding.Unicode); 
                                    string reply = client.Receive(Encoding.Unicode);
                                    ed.WriteMessage("\nReceived message {0} from Python: '{1}'\n", requestNum, reply);
                                }
                            }
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

        //OK let's try an event handler
        //http://through-the-interface.typepad.com/through_the_interface/2010/02/watching-for-deletion-of-a-specific-autocad-block-using-net.html
        //

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        [CommandMethod("REPENT")]
        public void SetEventHandler()
        {
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            //Database db = doc.Database;
            //Editor ed = doc.Editor;
            //SocketMessage Message = new SocketMessage();
            //SocketMessage SocketResponse = new SocketMessage();
            SocketWrapper.AutoCAD AutoCADWrapper = new SocketWrapper.AutoCAD();

            // Ask for the name of a block to watch for
            string response = "";
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
            {
                //Message.MessageType = "Init Command";
                //Message.ContentType = "string";
                //Message.Content = "REPENT";
                //AutoCADWrapper.Message = AutoCADWrapper.ComposeMessage("Init Command", "string", "REPENT");
                AutoCADWrapper.Message = new SocketMessage("Init Command", "string", "REPENT");
                //AutoCADWrapper.Message.MessageType = "Init Command";
                //AutoCADWrapper.Message.ContentType = "string";
                //AutoCADWrapper.Message.Content = "REPENT";
                do
                {
                    response = AutoCADWrapper.SendMessage(client, AutoCADWrapper.Message);
                    string message = AutoCADWrapper.DispatchReply(AutoCADWrapper.Reply);
                    response = AutoCADWrapper.Message.Content.ToString();
                } while (!response.Equals("END"));

            }
        }


        [CommandMethod("REPENT1")]
        public void SetEventHandler1()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            //SocketMessage Message = new SocketMessage();
            //SocketMessage SocketResponse = new SocketMessage();
            SocketWrapper.AutoCAD AutoCADWrapper = new SocketWrapper.AutoCAD();

            // Ask for the name of a block to watch for
            string response = "END";
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
            {
                //Message.MessageType = "Init Command";
                //Message.ContentType = "string";
                //Message.Content = "REPENT";
                do
                {
                    //SocketResponse = AutoCADWrapper.SendMessage(client, Message);
                    //Message = AutoCADWrapper.DispatchReply(SocketResponse);
                    //response = Message.Content.ToString();
                } while (!response.Equals("END"));

            }

            PromptStringOptions pso =
              new PromptStringOptions(
                "\nEnter entity type to mirror: "
              );
            pso.AllowSpaces = false;

            PromptResult pr = ed.GetString(pso);
            if (pr.Status != PromptStatus.OK)
                return;

            string entityType = pr.StringResult.ToUpper();
            
            //Message.MessageType = "UserInput";
            //Message.ContentType = "String";
            //Message.Content = entityType;

            //string messageBlob = JsonConvert.SerializeObject(Message);
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
            {

                client.Connect("tcp://localhost:5556");
                client.SendTimeout = new TimeSpan(0, 0, 10);
                client.ReceiveTimeout = new TimeSpan(0, 0, 10);

                //client.Send(messageBlob, Encoding.Unicode);
                string reply = client.Receive(Encoding.Unicode);
                reply = reply.Remove(0, 1);
                //var aaa = GetString(Encoding.Convert(Encoding.Unicode, Encoding.UTF8, GetBytes(reply))); garbage
                //reply = Encoding.UTF8.GetString(reply.ToCharArray()) garbage
                SocketMessage replyMessage = JsonConvert.DeserializeObject<SocketMessage>(reply);
                if (replyMessage.Content.Equals("OK"))
                {
                    ed.WriteMessage("\nIt went *okay*.\n");
                }

            }

        }

        private void OnCommandEnded(object sender, CommandEventArgs e)
        {
            // Start an outer transaction that we pass to our testing
            // function, avoiding the overhead of multiple transactions
            ObjectIdCollection _ids = null;
            Document doc = sender as Document;
            if (_ids != null)
            {
                Transaction tr =
                  doc.Database.TransactionManager.StartTransaction();
                using (tr)
                {
                    // Test each object, in turn

                    foreach (ObjectId id in _ids)
                    {
                        // The test function is responsible for presenting the
                        // user with the information: this could be returned to
                        // this function, if needed

                        //TestObjectAndShowMessage(doc, tr, id);
                    }

                    // Even though we're only reading, we commit the
                    // transaction, as this is more efficient

                    tr.Commit();
                }

                // Now we clear our list of entities

                _ids.Clear();
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
