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
            this.Status = GeneralProtocol.Status.OK;
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
            this.Status = GeneralProtocol.Status.OK;
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
            //If the Input is a dictionary, then just add its fields at first level
            //If the Input is not a dictionary, put in in the DEFAULT field of the new Payload member.
            Type T = Input.GetType();
            if (T.IsGenericType && T.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                this.Payload.Add((Dictionary<string, object>)Input);
            else
            {
                this.Payload.Add(new Dictionary<string, object>());
                Payload.Last().Add(GeneralProtocol.Keywords.DEFAULT, Input);
            }

            return true;
        }

        public string GetPayloadAsString(int num = 0)
        {
            return (string)this.Payload[num][GeneralProtocol.Keywords.DEFAULT];
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
        public void SetFinishedStatus()
        {
            this.Status = GeneralProtocol.Status.FINISH;
        }

        public void SetNames(ServerMessage reply)
        {
            var payload = reply.Payload;
            for(int i = 0; i < payload.Count; i++)
                {
                    this.Payload[i].Add(GeneralProtocol.Keywords.NAME, (string)payload[i][GeneralProtocol.Keywords.NAME]);
                }
        }
    }

    public class ServerMessage : SocketMessage
    {
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
                retval.Add((string)Item[GeneralProtocol.Keywords.DEFAULT]);
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



        public object GetPayloadAsObject(int num = 0)
        {
            return this.Payload[num][GeneralProtocol.Keywords.DEFAULT];
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
        {}
    }
}
