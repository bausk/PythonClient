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
    public delegate void ProcessCommand(SocketMessage Message);
    public class SocketMessage
        {
        public SocketMessage(string msgType, string contentType, object content)
        {
            this.MessageType = msgType;
            this.ContentType = contentType;
            this.Content = content;
            this.SerializedContent = JsonConvert.SerializeObject(content);
        }

            public string MessageType { get; set; }
            public string ContentType { get; set; }
            public string SerializedContent { get; set; }
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
        public string SendMessage(ZmqSocket client, SocketMessage message)
        {
            string response = "";
            client.Connect("tcp://localhost:" + Port.ToString());
            client.SendTimeout = new TimeSpan(0, 0, SendTimeout);
            client.ReceiveTimeout = new TimeSpan(0, 0, ReceiveTimeout);
            this.SerializedMessage = JsonConvert.SerializeObject(message);
            client.Send(SerializedMessage, Encoding.Unicode);
            this.SerializedReply = client.Receive(Encoding.Unicode).Remove(0,1);
            this.Reply = JsonConvert.DeserializeObject<SocketMessage>(SerializedReply);
            return response;
        }
        public string DispatchReply(SocketMessage message)
        {
            //SocketMessage response = new SocketMessage();
            string returnValue = "OK";
            this.Reply = new SocketMessage("OK", "string", "OK");
            switch (message.MessageType)
            {
                case "Set Event":
                    SetEvent();
                    break;
                case "Add Entities":
                    break;
                case "User Input":
                    break;
                case "END":
                    this.Reply = new SocketMessage("END", "string", "END");
                    returnValue = "END";
                    break;
                default:
                    this.Reply = new SocketMessage("END", "string", "END");
                    returnValue = "END";
                    break;
            }
            return returnValue;
        }

        private void SetEvent()
        {
            switch (Message.Content.ToString())
            {
                case "ObjectCreated":
                    db.ObjectAppended += new ObjectEventHandler(OnObjectCreated);
                    break;
                case "CommandEnded":
                    break;
                default:
                    Reply.MessageType = "END";
                    return;
            }
            Reply.MessageType = "OK";

        }

        public void OnObjectCreated(object sender, ObjectEventArgs e)
        {
          // Callback binder for real Python event handler
          // We should prepare a message and send it to Python
          // to initiate a session
            var a = e.DBObject.ObjectId;
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
    }
}
