using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketWrapper
{
    public class SocketMessage
    {
        public SocketMessage(string Action)
        {
            this.Callback = "";
            this.Action = Action;
            //this.Payload = null;
            //this.ContentType = "";
            this.Status = Protocol.Status.OK;
        }
        public SocketMessage(string Action, string Status)
        {
            this.Callback = "";
            this.Action = Action;
            //this.Payload = null;
            //this.ContentType = "";
            this.Status = Status;
        }
        public SocketMessage(string Action, string Status, string Callback)
        {
            this.Callback = Callback;
            this.Action = Action;
            //this.Payload = null; 
            //this.ContentType = "";
            this.Status = Status;
        }
        public SocketMessage()
        {
            this.Callback = "";
            this.Action = "";
            //this.Payload = null;
            //this.ContentType = "";
            this.Status = Protocol.Status.OK;
        }

            public string Action { get; set; }
            //public string ContentType { get; set; }
            public string Callback { get; set; }
            public string Status { get; set; }
            public object Parameters { get; set; }


            //public List<Dictionary<string,object>> Payload { get; set; }

    }

    public class ServerMessage : SocketMessage
    {
        public object Payload
        {
            get;
            set;
        }

    }


    public class ClientMessage : SocketMessage
    {
        public ClientMessage(string Action, string Status, string Callback)
            : base(Action, Status, Callback)
        {
            _Payload = new List<Dictionary<string,object>>();
        }
        public ClientMessage()
            : base()
        {
            _Payload = new List<Dictionary<string, object>>();
        }
        public ClientMessage(string Action)
            : base(Action)
        {
            _Payload = new List<Dictionary<string, object>>();
        }
        public ClientMessage(string Action, string Status)
            : base(Action, Status)
        {
            _Payload = new List<Dictionary<string, object>>();
        }

        private List<Dictionary<string, object>> _Payload;
        public object Payload
        {
            get
            {
                return (object)_Payload;
            }

            set
            {
                var a = new Dictionary<string, object>();
                a["object"] = value;
                _Payload.Add(a);
            }
        }

        public bool AddPayload(object Payload)
        {
            //Type T = Payload.GetType();
            //this.ContentType = "LIST";
            this.Payload = Payload;
            return true;
        }
    }
}
