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
        public ServerMessage()
            : base()
        {
            _Payload = new List<Dictionary<string, object>>();
            _PayloadType = null;
        }
        public ServerMessage(string Action, string Status)
            : base(Action, Status)
        {
            _Payload = new List<Dictionary<string, object>>();
            _PayloadType = null;
        }

        private List<Dictionary<string, object>> _Payload;
        private String _PayloadType;
        public object Payload
        {
            get
            {
                switch(_PayloadType)
                {
                    case Protocol.PayloadTypes.STRING:
                        return _Payload[0][Protocol.Keywords.DEFAULT];
                    case Protocol.PayloadTypes.LIST:
                        return _Payload;
                    case Protocol.PayloadTypes.LISTOFSTRINGS:
                        List<string> retval = new List<string>();
                        foreach (Dictionary<string, object> Item in _Payload)
                        {
                            retval.Add((string)Item[Protocol.Keywords.DEFAULT]);
                        }
                        return retval;
                    case Protocol.PayloadTypes.DICT:
                        return _Payload;
                    default:
                        return new List<Dictionary<string, object>>();
                }
            }
            set
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
                //this is the most interesting part

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
