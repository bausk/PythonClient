using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using ZeroMQ;
using Newtonsoft.Json;

namespace SocketWrapper
{

    public class Transport
    {
        public int SendTimeout { get; set; }
        public int ReceiveTimeout { get; set; }
        public int Port { get; set; }  
  
        public ServerMessage SendMessage(ZmqSocket client, ClientMessage message)
        {
            ServerMessage response = new ServerMessage();
            client.Connect("tcp://localhost:" + this.Port.ToString());
            client.SendTimeout = new TimeSpan(0, 0, this.SendTimeout);
            client.ReceiveTimeout = new TimeSpan(0, 0, this.ReceiveTimeout);
            string SerializedMessage = JsonConvert.SerializeObject(message);
            //substitute for testing GetEntity
            client.Send(SerializedMessage, Encoding.Unicode);
            if (Protocol.CheckForClientExit(message))
            {
                //Client exits without waiting for reply
                //bypass client.Receive. Emulate server exit to break dispatch loop
                response.Action = Protocol.ServerAction.TERMINATE;
            }
            else
            {
                //Client waits for reply
                
                //string SerializedReply = "{\"Status\": \"_ONHOLD\", \"ContentType\": \"NONE\", \"Parameters\": {\"Prompt\": \"Choose first entity\"}, \"Callback\": \"E7C2B6230C8647059ACEC108F957D3F5\", \"Action\": \"GET_ENTITY\", \"Payload\": null}";
                //string SerializedReply = "{\"Status\": \"_ONHOLD\", \"ContentType\": \"NONE\", \"Parameters\": [{\"Prompt\": \"Choose first entity\"}, {\"Prompt\": \"Choose second entity\"}], \"Callback\": \"E7C2B6230C8647059ACEC108F957D3F5\", \"Action\": \"GET_ENTITY\", \"Payload\": null}";
                string SerializedReply = client.Receive(Encoding.UTF8);//.Remove(0,1);
                try
                {
                    response = JsonConvert.DeserializeObject<ServerMessage>(SerializedReply);
                }
                catch (ArgumentNullException ex)
                {
                    response = Protocol.NewServerError("Server not reached, exiting");
                }
            }
            return response;
        }

        public void CommandLoop(SocketWrapper.AutoCAD Session, ClientMessage Message)
        {
            ServerMessage Reply = Protocol.NewReply(); 
            bool exitflag = false;
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
            {
                do
                {
                    //Substitute message
                    
                    Reply = this.SendMessage(client, Message);
                    exitflag = Protocol.CheckForTermination(Reply); //client stops without doing any work
                    Message = Session.DispatchReply(Reply);
                    if (!exitflag)
                    {
                        exitflag = Protocol.CheckForServerCleanExit(Reply); //server sends FINISH status, client does work and stops without initiating new message pair
                    }
                } while (!exitflag);
            }
        }
    }
    
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

            List<Dictionary<string,object>> Prompts = Utilities.ParametersToList(reply.Parameters);
            List<PromptEntityResult> Result = new List<PromptEntityResult>();
            foreach (Dictionary<string,object> Prompt in Prompts)
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
                    peo.SetRejectMessage((string) value);

                if (Prompt.TryGetValue(Utilities.AutoCADKeywords.AllowedClass, out value))
                    foreach (string Type in (List<string>) value)
                        peo.AddAllowedClass(Protocol.EntityTypes[Type], false);

                PromptEntityResult per = ed.GetEntity(peo);

                if (per.Status != PromptStatus.OK)
                    return new ClientMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);
                Result.Add(per);
                //ObjectId regId = per.ObjectId;
                //Add object mining and message forming
            }

            ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
            message.AddPayload(Result);
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
            message.AddPayload("No result");
            return message;
        }

        private ClientMessage Write(ServerMessage reply)
        {
            //WORK HERE
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