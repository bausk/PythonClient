using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace Draftsocket.AutoCAD
{
    public partial class Session : ISession
    {
        private ClientMessage GetDBObjects(ServerMessage reply)
        {
            List<string> prompts = reply.GetPayloadAsStringList();
            List<string> replies = new List<string>();
            ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
            foreach (string prompt in prompts)
            {
                PromptStringOptions pso = new PromptStringOptions("\n" + prompt);
                PromptResult pr = ed.GetString(pso);
                if (pr.Status != PromptStatus.OK)
                {
                    //WORK HERE - will need to process canceled inputs
                }
                else
                {
                    replies.Add(pr.StringResult);
                }
            }
            message.SetPayload(replies);
            message.SetNames(reply);
            return message;
        }

        private ClientMessage GetStrings(ServerMessage reply)
        {
            List<string> prompts = reply.GetPayloadAsStringList();
            List<string> replies = new List<string>();
            ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
            foreach (string prompt in prompts)
            {
                PromptStringOptions pso = new PromptStringOptions("\n" + prompt);
                PromptResult pr = ed.GetString(pso);
                if (pr.Status != PromptStatus.OK)
                {
                    //WORK HERE - will need to process canceled inputs
                }
                else
                {
                    replies.Add(pr.StringResult);
                }
            }
            message.SetPayload(replies);
            message.SetNames(reply);
            return message;
        }

        private ClientMessage GetEntity(ServerMessage reply)
        {

            List<Dictionary<string, object>> Prompts = reply.Payload;
            List<PromptEntityResult> Result = new List<PromptEntityResult>();
            foreach (Dictionary<string, object> PromptDict in Prompts)
            {
                PromptEntityOptions peo;
                try
                {
                    peo = new PromptEntityOptions((string)PromptDict[Protocol.Local.Prompt]);
                }
                catch
                {
                    return new ClientMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);
                }

                object value;
                if (PromptDict.TryGetValue(Protocol.Local.RejectMessage, out value))
                    peo.SetRejectMessage((string)value);

                if (PromptDict.TryGetValue(Protocol.Local.AllowedClass, out value))
                    foreach (string Type in (List<string>)value)
                        peo.AddAllowedClass(Protocol.EntityTypes[Type], false);

                PromptEntityResult per = ed.GetEntity(peo);

                //Add the result to payload (SetPayload called later to form the message).
                Result.Add(per);

                //Memoization: if a name field was set in the payload item of reply message,
                //keep the result (per) in SavedObjects
                if (PromptDict.TryGetValue(GeneralProtocol.Keywords.NAME, out value))
                    this.SavedObjects.Add((string)value, per);
            }

            ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
            List<Dictionary<string, object>> DictResult = this.ObjectsToDicts(Result);
            message.SetPayload(DictResult);
            message.SetNames(reply);
            return message;
        }


        private ClientMessage Batch(ServerMessage reply)
        {
            var response = new ServerMessage();
            var finalMessage = new ClientMessage(Protocol.CommonAction.BATCH, Protocol.Status.OK);

            foreach (string SerializedReply in reply.GetPayloadAsStringList())
            {
                response = this.transport.Deserialize(SerializedReply);
                //call the message dispatcher recursively for each deserialized payload item
                ClientMessage message = this.DispatchReply(response);
                //Add the result to the batch message
                var SerializedMessage = this.transport.Serialize(message);
                finalMessage.AddPayloadItem(SerializedMessage);
                if (GeneralProtocol.CheckForExit(message))
                    break;
            }
            return finalMessage;
        }


        private ClientMessage Write(ServerMessage reply)
        {
            ed.WriteMessage(String.Join(String.Empty, reply.GetPayloadAsStringList()));
            ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
            return message;
        }

        private ClientMessage SetEvent(ServerMessage reply)
        {
            this.db.ObjectAppended += new ObjectEventHandler(OnObjectCreated);
            SocketMessage message = new SocketMessage();
            switch (reply.Payload.ToString())
            {
                case "ObjectCreated":
                    db.ObjectAppended += new ObjectEventHandler(OnObjectCreated);
                    break;
                case "CommandEnded":
                    break;
                default:
                    message.Action = "END";
                    return new ClientMessage();
            }
            message.Action = "OK";
            return new ClientMessage();
        }

        public void OnObjectCreated(object sender, ObjectEventArgs e)
        {
            // Callback binder for real Python event handler
            // We should prepare a message and send it to Python
            // to initiate a session
            //var a = e.DBObject.ObjectId;
            //SocketWrapper.AutoCAD AutoCADWrapper = new SocketWrapper.AutoCAD();
            /*string our_reply = "";
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
            {
                this.Message = new SocketMessage("Init Event", "STRING", "OnObjectCreated");
                do
                {
                    string response = this.SendMessage(client, this.Message);
                    our_reply = this.DispatchReply(this.Reply);
                    our_reply = "END";
                } while (!our_reply.Equals("END"));

            }*/

        }


        private ClientMessage GetKeyword(ServerMessage reply)
        {
                List<Dictionary<string, object>> Prompts = reply.Payload;
                var Result = new List<PromptResult>();
                foreach (Dictionary<string, object> PromptDict in Prompts)
                {
                    PromptKeywordOptions pko;

                    object value;
                    if (PromptDict.TryGetValue(Protocol.Local.Prompt, out value))
                        pko = new PromptKeywordOptions((string)PromptDict[Protocol.Local.Prompt]);
                    else
                        return GeneralProtocol.NewClientError("");

                    if (PromptDict.TryGetValue(Protocol.Local.AllowNone, out value))
                        pko.AllowNone = true;

                    if (PromptDict.TryGetValue(Protocol.Local.AllowArbitraryInput, out value))
                        if ((bool)value == true)
                            pko.AllowArbitraryInput = true;
                        else
                            pko.AllowArbitraryInput = false;
                    if (PromptDict.TryGetValue(Protocol.Local.Keywords, out value))
                        foreach (string Keyword in (Newtonsoft.Json.Linq.JArray)value)
                            pko.Keywords.Add(Keyword);
                    else
                        return new ClientMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);

                    if (PromptDict.TryGetValue(Protocol.Local.Default, out value))
                        pko.Keywords.Default = (string)value;

                    PromptResult pkr =
                      ed.GetKeywords(pko);

                    //Add the result to payload (SetPayload called later to form the message).
                    Result.Add(pkr);

                    //Memoization: if a name field was set in the payload item of reply message,
                    //keep the result (per) in SavedObjects
                    if (PromptDict.TryGetValue(GeneralProtocol.Keywords.NAME, out value))
                        this.SavedObjects.Add((string)value, pkr);
                }

                ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
                List<Dictionary<string, object>> DictResult = this.MakePayload(Result);
                message.SetPayload(DictResult);
                message.SetNames(reply);
                return message;
        }




    }
}
