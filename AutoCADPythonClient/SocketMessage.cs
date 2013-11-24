using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Draftsocket
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
        //WORK HERE
        public ServerMessage()
            : base()
        {
            Payload = new List<Dictionary<string, object>>();
        }
        public ServerMessage(string Action, string Status)
            : base(Action, Status)
        {
            Payload = new List<Dictionary<string, object>>();
        }

        public List<Dictionary<string, object>> Payload //ALWAYS expect a list of dicts
            { get; set; }

        public List<string> GetPayloadAsStringList()
        {
            List<string> retval = new List<string>();
            foreach (Dictionary<string, object> Item in this.Payload)
            {
                retval.Add((string)Item[Protocol.Keywords.DEFAULT]);
            }
            return retval;
        }

        public string GetPayloadAsString(int num = 0)
        {
            return (string) this.Payload[num][Protocol.Keywords.DEFAULT];
        }

        public object GetPayloadAsObject(int num = 0)
        {
            return this.Payload[num][Protocol.Keywords.OBJECT];
        }

    }


    public class ClientMessage : SocketMessage
    {
        //Not sure how it will be serialized
        public ClientMessage(string Action, string Status, string Callback)
            : base(Action, Status, Callback)
        {
            Payload = new List<Dictionary<string,object>>();
        }
        public ClientMessage()
            : base()
        {
            Payload = new List<Dictionary<string, object>>();
        }
        public ClientMessage(string Action)
            : base(Action)
        {
            Payload = new List<Dictionary<string, object>>();
        }
        public ClientMessage(string Action, string Status)
            : base(Action, Status)
        {
            Payload = new List<Dictionary<string, object>>();
        }

        //private List<Dictionary<string, object>> _Payload;
        public List<Dictionary<string, object>> Payload
        {
            get; set;
        }

        public bool AddPayloadItem(object Input)
        {
            if (Input is Dictionary<string, object>)
                this.Payload.Add((Dictionary<string, object>)Input);
            else
                this.Payload.Add(new Dictionary<string, object>());

            Payload.Last().Add(Protocol.Keywords.DEFAULT, Input);
            //Here be different object fields implementation
            //WORK HERE
            return true;
        }

        public void SetPayload(object Input)
        {
            this.Payload = new List<Dictionary<string, object>>();
            if (Input is List<object>)
            {
                foreach (object Item in (List<string>)Input)
                {
                    AddPayloadItem(Item);
                }
            }
            else if (Input is Dictionary<string, object>)
            {
                AddPayloadItem(Input);
            }
            else
                this.AddPayloadItem(Input);
        }
    }
}
