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
    public class SocketMessage
        {
        public const int Empty = 0;
        public const int End = 1;
        public const int Abort = 2;
        public const int Error = 3;

        public SocketMessage(string msgType, string contentType, object content)
        {
            this.MessageType = msgType;
            this.ContentType = contentType;
            this.Content = content;
            this.Callback = "";
            //this.SerializedContent = JsonConvert.SerializeObject(content);
        }
        public SocketMessage(string msgType, string contentType, object content, string callback)
        {
            this.MessageType = msgType;
            this.ContentType = contentType;
            this.Content = content;
            this.Callback = callback;
            //this.SerializedContent = JsonConvert.SerializeObject(content);
        }
        public SocketMessage()
        {
            this.MessageType = "";
            this.ContentType = "";
            this.Content = null;
            this.Callback = "";
            //this.SerializedContent = "";
        }
        public string MessageType { get; set; }
        public string ContentType { get; set; }
        public string Callback { get; set; }
        //public string SerializedContent { get; set; }
        public object Content { get; set; }

        }

    public class AutoCAD
    {
        public AutoCAD()
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;
            //Message = new SocketMessage();
            //Reply = new SocketMessage();
            SendTimeout = 10;
            ReceiveTimeout = 10;
            Port = 5556;
        }
        /*public SocketMessage ComposeMessage(string msgType, string contentType, object content)
        {
            SocketMessage message = new SocketMessage();
            message.MessageType = msgType;
            message.ContentType = contentType;
            message.Content = content;
            message.SerializedContent = JsonConvert.SerializeObject(content);
            return message;
        }*/
        public SocketMessage SendMessage(ZmqSocket client, SocketMessage message)
        {
            //SocketMessage response = new SocketMessage();
            client.Connect("tcp://localhost:" + Port.ToString());
            client.SendTimeout = new TimeSpan(0, 0, SendTimeout);
            client.ReceiveTimeout = new TimeSpan(0, 0, ReceiveTimeout);
            this.SerializedMessage = JsonConvert.SerializeObject(message);
            client.Send(SerializedMessage, Encoding.Unicode);
            this.SerializedReply = client.Receive(Encoding.UTF8);//.Remove(0,1);
            var a = JsonConvert.DeserializeObject(SerializedReply);
            SocketMessage response = JsonConvert.DeserializeObject<SocketMessage>(SerializedReply);
            return response;
        }
        public SocketMessage DispatchReply(SocketMessage reply)
        {
            SocketMessage message = new SocketMessage("END", "STRING", "END");
            switch (reply.MessageType)
            {
                case "SET_EVENT":
                    message = this.SetEvent(reply);
                    break;
                case "MANIPULATE_DB":
                    break;
                case "REQUEST_INPUT":
                    //this.Reply = new SocketMessage("CONTINUE", "STRING", message.Callback);
                    message = this.GetUserInput(reply);
                    break;
                case "END":
                    message = new SocketMessage("END", "STRING", "END");
                    //returnValue = "END";
                    break;
                default:
                    message = new SocketMessage("END", "STRING", "END");
                    //returnValue = "END";
                    break;
            }
            message.Callback = reply.Callback;
            return message;
        }

        private SocketMessage GetUserInput(SocketMessage reply)
        {
            PromptStringOptions pso = new PromptStringOptions("\nPlease enter your input:");
            pso.AllowSpaces = true;
            PromptResult pr = ed.GetString(pso);

            if (pr.Status != PromptStatus.OK)
                return new SocketMessage("ERROR", "STRING", "USERABORT");

            return new SocketMessage("CONTINUE", "STRING", pr.StringResult);
        }

        private SocketMessage SetEvent(SocketMessage reply)
        {
            this.db.ObjectAppended += new ObjectEventHandler(OnObjectCreated);
            switch (reply.Content.ToString())
            {
                case "ObjectCreated":
                    db.ObjectAppended += new ObjectEventHandler(OnObjectCreated);
                    break;
                case "CommandEnded":
                    break;
                default:
                    this.Message.MessageType = "END";
                    return new SocketMessage();
            }
            this.Message.MessageType = "OK";
            return new SocketMessage();
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

        public bool CheckForExit(SocketMessage Message)
        {
            return false;
        }

        public SocketMessage Message { get; set; }
        public SocketMessage Reply { get; set; }
        public int SendTimeout { get; set; }
        public int ReceiveTimeout { get; set; }
        public int Port { get; set; }
        public string SerializedReply { get; set; }
        public string SerializedMessage { get; set; }
        private Document doc { get; set; }
        private Database db { get; set; }
        private Editor ed { get; set; }


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

    }

}
