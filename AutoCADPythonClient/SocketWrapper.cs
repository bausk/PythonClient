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
            Message = new SocketMessage();
            Reply = new SocketMessage();
            SendTimeout = 10;
            ReceiveTimeout = 10;
            Port = 5556;
        }
        public SocketMessage SendMessage(ZmqSocket client, SocketMessage message)
        {
            SocketMessage response = new SocketMessage();
            client.Connect("tcp://localhost:" + Port.ToString());
            client.SendTimeout = new TimeSpan(0, 0, SendTimeout);
            client.ReceiveTimeout = new TimeSpan(0, 0, ReceiveTimeout);
            SerializedMessage = JsonConvert.SerializeObject(message);
            client.Send(SerializedMessage, Encoding.Unicode);
            SerializedReply = client.Receive(Encoding.Unicode).Remove(0,1);
            Reply = JsonConvert.DeserializeObject<SocketMessage>(SerializedReply);
            return Reply;
        }
        public SocketMessage DispatchReply(SocketMessage message)
        {
            SocketMessage response = new SocketMessage();

            switch (message.MessageType)
            {
                case "Set Event":
                    switch (message.Content.ToString())
                    {
                        case "ObjectCreated":
                            ProcessCommand processor = new ProcessCommand();
                            break;
                        case "CommandEnded":
                            break;                    
                    }
                    
                    break;
                case "Add Entities":
                case "User Input":

                default:
                    break;
            }
            response.Content = "END";
            return response;
        }
        private void SetEvent(SocketMessage Message)
        {
            switch (Message.Content.ToString())
            {
                case "ObjectCreated":
                    db.ObjectAppended += new ObjectEventHandler(OnObjectCreated);
                    break;
                case "CommandEnded":
                    break;

            }
        }

        public void OnObjectCreated(object sender, ObjectEventArgs e)
        {
          // Very simple: we just add our ObjectId to the list
          // for later processing

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
