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
            this.Status = Protocol.Status.OK;
            Payload = new List<Dictionary<string, object>>();
        }
        public SocketMessage(string Action, string Status)
        {
            this.Callback = "";
            this.Action = Action;
            this.Status = Status;
            Payload = new List<Dictionary<string, object>>();
        }
        public SocketMessage(string Action, string Status, string Callback)
        {
            this.Callback = Callback;
            this.Action = Action;
            this.Status = Status;
            Payload = new List<Dictionary<string, object>>();
        }
        public SocketMessage()
        {
            this.Callback = "";
            this.Action = "";
            this.Status = Protocol.Status.OK;
            Payload = new List<Dictionary<string, object>>();
        }

        public List<Dictionary<string, object>> Payload //ALWAYS expect a list of dicts
        { get; set; }
            public string Action { get; set; }
            public string Callback { get; set; }
            public string Status { get; set; }
            public object Parameters { get; set; }

            public bool AddPayloadItem(object Input)
            {
                //A single entry can be a Dictionary or an arbitrary object.
                //We put any entry under Default field.
                //If a dictionary, then also add its fields at first level
                Type T = Input.GetType();
                if (T.IsGenericType && T.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    this.Payload.Add((Dictionary<string, object>)Input);
                else
                    this.Payload.Add(new Dictionary<string, object>());

                Payload.Last().Add(Protocol.Keywords.DEFAULT, Input);
                //Here be different object fields implementation
                //WORK HERE - external function to take care of fields specific to particular types
                return true;
            }

            public void SetPayload(object Input)
            {
                //Accepted Payload can be a List or not a List
                //List means multiple inputs so is treated sequentially
                this.Payload = new List<Dictionary<string, object>>();
                Type T = Input.GetType();
                if (T.IsGenericType && T.GetGenericTypeDefinition() == typeof(List<>))
                {
                    foreach (object Item in (IEnumerable<object>)Input)
                    {
                        AddPayloadItem(Item);
                    }
                }
                else
                {
                    AddPayloadItem(Input);
                }
            }
    }

    public class ServerMessage : SocketMessage
    {
        //WORK HERE
        public ServerMessage()
            : base()
        {}
        public ServerMessage(string Action, string Status)
            : base(Action, Status)
        {}

        public List<string> GetPayloadAsStringList()
        {
            List<string> retval = new List<string>();
            foreach (Dictionary<string, object> Item in this.Payload)
            {
                retval.Add((string)Item[Protocol.Keywords.DEFAULT]);
            }
            return retval;
        }

        public List<object> GetPayloadListByKey(string Key)
        {
            List<object> retval = new List<object>();
            foreach (Dictionary<string, object> Item in this.Payload)
            {
                retval.Add((object)Item[Key]);
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
        public ClientMessage()
            : base()
        {}
        public ClientMessage(string Action)
            : base(Action)
        {}
        public ClientMessage(string Action, string Status)
            : base(Action, Status)
        {}
        public ClientMessage(string Action, string Status, string Callback)
            : base(Action, Status, Callback)
        { }
    }
}
