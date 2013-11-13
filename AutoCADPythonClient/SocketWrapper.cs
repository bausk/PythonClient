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

    public static class Protocol
    {
        public struct ClientAction
        {
            //client-issued action lines
            public const string CMD = "COMMAND";
            public const string ERROR = "CLIENT_ERROR";
            public const string EVENT = "EVENT";
            public const string CONTINUE = "CONTINUE";
            public const string CONSOLE = "CONSOLE";
        }

        public struct ServerAction
        {
            //server-issued action lines
            public const string SETEVENT = "SET_EVENT";
            public const string WRITE = "WRITE_MESSAGE";
            public const string REQUEST_USER_INPUT = "REQUEST_INPUT";
            public const string REQUEST_SEVERAL_USER_INPUTS = "REQUEST_SEVERAL_INPUTS";
            public const string TERMINATE = "TERMINATE_SESSION";
            public const string GET_ENTITY_ID = "GET_ENTITY";
            public const string TRANSACTION_START = "TRANSACTION_START";
            public const string TRANSACTION_COMMIT = "TRANSACTION_COMMIT";
            public const string TRANSACTION_ABORT = "TRANSACTION_ABORT";
            public const string TRANSACTION_GETOBJECT = "TR_GET_OBJECT";
            public const string TRANSACTION_MANIPULATE_DB = "TR_MANIPULATE_DB";
        }

        //status lines
        public struct Status
        {
            public const string FINISH = "_FINISH";
            public const string OK = "_OK";
            public const string ONHOLD = "_ONHOLD";
        }        
        
        //Payload parameter demands, should rename
        public const int PAYLOAD_STRING = 1;
        public const int PAYLOAD_ENTITIES = 2;

        public static Dictionary<Type, string> Types = new Dictionary<Type, string>()
        {
            {typeof(int), "INT"},
            {typeof(float), "FLOAT"},
            {typeof(string), "STRING"},
            {typeof(List<>), "LIST"},
            {typeof(Dictionary), "DICTIONARY"},
            {typeof(object), "OBJECT"}
        };

        public static Dictionary<string, Type> EntityTypes = new Dictionary<string, Type>()
        {
            {"LINE",typeof(Line)},
            {"CURVE",typeof(Curve)},
            {"CIRCLE",typeof(Circle)},
            {"HATCH",typeof(Hatch)},
        };

        public static bool CheckForClientExit(SocketMessage Message)
        {
            if(Message.Status == Protocol.Status.FINISH)
                return true;
            else
                return false;
        }
        public static bool CheckForServerExit(SocketMessage Reply)
        {
            if (Reply.Status == Protocol.Status.FINISH)
                return true;
            else
                return false;
        }
        public static bool CheckForTermination(SocketMessage Reply)
        {
            if (Reply.Action == Protocol.ServerAction.TERMINATE)
                return true;
            else
                return false;
        }

        //client command factories
        public static ClientMessage NewCommand(string Name)
        {
            return new ClientMessage(Protocol.ClientAction.CMD, Protocol.Status.OK, Name);
        }
        public static ServerMessage NewReply()
        {
            return new ServerMessage();
        }
        
    }

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
                response = JsonConvert.DeserializeObject<ServerMessage>(SerializedReply);
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
                    if (!exitflag)
                    {
                        Message = Session.DispatchReply(Reply);
                        exitflag = Protocol.CheckForServerExit(Reply); //server sends FINISH status, client does work and stops without initiating new message pair
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

            PromptStringOptions pso = new PromptStringOptions("\n" + reply.Payload);
            PromptResult pr = ed.GetString(pso);

            if (pr.Status != PromptStatus.OK)
                return new ClientMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);

            ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
            message.AddPayload(pr.StringResult);
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

            ed.WriteMessage((string) reply.Payload);
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