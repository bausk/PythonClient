using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace Draftsocket
{
    public class AutoCAD
    {
        public AutoCAD(Transport transport)
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;
            transport.SendTimeout = 1;
            transport.ReceiveTimeout = 1;
            transport.Port = 5556;
        }

        public ClientMessage DispatchReply(ServerMessage reply)
        {
            ClientMessage message = new ClientMessage();
            switch (reply.Action)
            {
                case Protocol.ServerAction.TERMINATE:
                    if (reply.Status == Protocol.Status.SERVER_ERROR)
                    {
                        this.Write(reply);
                    }
                    message = new ClientMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);
                    break;
                case Protocol.ServerAction.SETEVENT:
                    message = this.SetEvent(reply);
                    break;
                case Protocol.ServerAction.REQUEST_USER_INPUT:
                    message = this.GetString(reply);
                    break;
                case Protocol.ServerAction.WRITE:
                    message = this.Write(reply);
                    break;
                case Protocol.ServerAction.GET_ENTITY_ID:
                    message = this.GetEntity(reply);
                    break;
                case Protocol.ServerAction.TRANSACTION_START:
                    message = this.Transaction(reply);
                    break;
                case Protocol.ServerAction.TRANSACTION_GETOBJECT:
                    break;
                case Protocol.ServerAction.TRANSACTION_MANIPULATE_DB:
                    break;
                case Protocol.ServerAction.TRANSACTION_COMMIT:
                    message = new ClientMessage(Protocol.ClientAction.CONTINUE, Protocol.Status.OK);
                    break;
                case Protocol.ServerAction.TRANSACTION_ABORT:
                    break;
                default:
                    message = new ClientMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);
                    break;
            }
            message.Callback = reply.Callback; //highly doubtful we should do it here
            return message;
        }

        private ClientMessage GetString(ServerMessage reply)
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
                }
                else
                {
                    replies.Add(pr.StringResult);
                }
                //return new ClientMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);
            }

            //message.AddPayload(pr.StringResult);
            return message;
        }

        private ClientMessage GetEntity(ServerMessage reply)
        {

            List<Dictionary<string, object>> Prompts = Utilities.ParametersToList(reply.Parameters);
            List<PromptEntityResult> Result = new List<PromptEntityResult>();
            foreach (Dictionary<string, object> Prompt in Prompts)
            {
                PromptEntityOptions peo;
                try
                {
                    peo = new PromptEntityOptions((string)Prompt[Utilities.AutoCADKeywords.Prompt]);
                }
                catch
                {
                    return new ClientMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);
                }

                object value;
                if (Prompt.TryGetValue(Utilities.AutoCADKeywords.RejectString, out value))
                    peo.SetRejectMessage((string)value);

                if (Prompt.TryGetValue(Utilities.AutoCADKeywords.AllowedClass, out value))
                    foreach (string Type in (List<string>)value)
                        peo.AddAllowedClass(Protocol.EntityTypes[Type], false);

                PromptEntityResult per = ed.GetEntity(peo);

                if (per.Status != PromptStatus.OK)
                    return new ClientMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);
                Result.Add(per);
                //ObjectId regId = per.ObjectId;
                //Add object mining and message forming
            }

            ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
            message.SetPayload(Result);
            return message;
        }

        private ClientMessage Transaction(ServerMessage reply)
        {

            /*PromptStringOptions pso = new PromptStringOptions("\n" + reply.Payload);
            //bool value = true;
            object value;
            if (reply.Parameters.TryGetValue("AllowSpaces", out value))
                pso.AllowSpaces = (bool)value;
            else
                pso.AllowSpaces = true;

            PromptResult pr = ed.GetString(pso);

            if (pr.Status != PromptStatus.OK)
                return new SocketMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);*/

            ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
            message.SetPayload("No result");
            return message;
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

        private Document doc { get; set; }
        private Database db { get; set; }
        private Editor ed { get; set; }

    }
}
