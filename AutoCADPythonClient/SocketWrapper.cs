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

            return response;
        }
        public SocketMessage DispatchReply(SocketMessage message)
        {
            SocketMessage response = new SocketMessage(); 
            response.Content = "END";
            return response;
        }
        public SocketMessage Message { get; set; }
        public SocketMessage Reply { get; set; }
        public int SendTimeout { get; set; }
        public int ReceiveTimeout { get; set; }
        public int Port { get; set; }
        private Document doc { get; set; }
        private Database db { get; set; }
        private Editor ed { get; set; }
    }
}
