using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroMQ;

namespace SocketWrapper
{
    public class PythonMessage
        {
            public string MessageType { get; set; }
            public string ContentType { get; set; }
            public object Content { get; set; }
        }
    public class AutoCAD
    {
        public void SendMessage(ZmqSocket client, PythonMessage message)
        {

        }
        public string Meh {
            get; set;
        }
    }
}
