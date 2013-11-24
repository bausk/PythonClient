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

        /*
        public object Payload
            set //- this is to become AddPayload
            {
                Type T = value.GetType();
                _Payload = new List<Dictionary<string, object>>();
                if (value is System.String)
                {
                    _Payload.Add(new Dictionary<string, object>());
                    _Payload[0].Add(Protocol.Keywords.DEFAULT, (string) value);
                    _PayloadType = Protocol.PayloadTypes.STRING;
                }
                else if (value is List<string>)
                {
                    foreach (string Item in (List<string>)value)
                    {
                        _Payload.Add(new Dictionary<string, object>());
                        _Payload.Last().Add(Protocol.Keywords.DEFAULT, Item);
                    }
                    _PayloadType = Protocol.PayloadTypes.LISTOFSTRINGS;
                }
                else if (value is List<object>)
                {
                    foreach (object Item in (List<object>)value)
                    {
                        if (Item is Dictionary<string, object>)
                        {
                            _Payload.Add((Dictionary<string, object>)Item);
                        }
                        else
                            _Payload.Add(new Dictionary<string, object>());
                        _Payload.Last().Add(Protocol.Keywords.OBJECT, Item);
                    }
                    _PayloadType = Protocol.PayloadTypes.LIST;
                }
                else if (value is Dictionary<string,object>)
                {
                    _Payload.Add((Dictionary<string,object>)value);
                    _Payload.Last().Add(Protocol.Keywords.OBJECT, value);
                    _PayloadType = Protocol.PayloadTypes.DICT;
                }
                //_Payload = (List<Dictionary<string, object>>)value;
            }
        }
        */
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
/*            {
                return (object)_Payload;
            }

            set
            {
                //this is the most interesting part

                var a = new Dictionary<string, object>();
                a["object"] = value;
                _Payload.Add(a);
            }
 */
        }

        public bool SetPayload(object Input)
        {
            //Type T = Input.GetType();
            this.Payload = new List<Dictionary<string, object>>();
            if (Input is System.String)
            {
                this.Payload.Add(new Dictionary<string, object>());
                Payload[0].Add(Protocol.Keywords.DEFAULT, (string)Input);
                //_PayloadType = Protocol.PayloadTypes.STRING;
            }
            else if (Input is List<string>)
            {
                foreach (string Item in (List<string>)Input)
                {
                    this.Payload.Add(new Dictionary<string, object>());
                    this.Payload.Last().Add(Protocol.Keywords.DEFAULT, Item);
                }
                //_PayloadType = Protocol.PayloadTypes.LISTOFSTRINGS;
            }
            else if (Input is List<object>)
            {
                foreach (object Item in (List<object>)Input)
                {
                    if (Item is Dictionary<string, object>)
                    {
                        this.Payload.Add((Dictionary<string, object>)Item);
                    }
                    else
                        this.Payload.Add(new Dictionary<string, object>());
                    this.Payload.Last().Add(Protocol.Keywords.OBJECT, Item);
                }
                //_PayloadType = Protocol.PayloadTypes.LIST;
            }
            else if (Input is Dictionary<string, object>)
            {
                this.Payload.Add((Dictionary<string, object>)Input);
                this.Payload.Last().Add(Protocol.Keywords.OBJECT, Input);
                //_PayloadType = Protocol.PayloadTypes.DICT;
            }
            else
                return false;
            return true;
        }

        public bool AddPayload(object Payload)
        {
            //Type T = Payload.GetType();
            //this.ContentType = "LIST";
            this.Payload = (List<Dictionary<string, object>>) Payload;
            return true;
        }
    }
}
