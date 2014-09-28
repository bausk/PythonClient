using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using ZeroMQ;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using MsgPack.Serialization;

namespace Draftsocket
{

    public class RPCTransport : ITransport
    {
        public int SendTimeout { get; set; }
        public int ReceiveTimeout { get; set; }
        public int Port { get; set; }
        public ZmqSocket Client { get; set; }
        public Dictionary<Guid, object> CurrentObjects { get; set; }
        public Dictionary<string, object> Names = new Dictionary<string, object>()
            {
                {"DocumentManager",Application.DocumentManager},
            };

        public bool Send(ClientMessage message)
        {
            if (this.Client.ReceiveStatus == ZeroMQ.ReceiveStatus.TryAgain)
            {
                //This part has to wait until new pattern implementation
                //Dealer/router for example
                this.Client.Disconnect("tcp://localhost:" + this.Port.ToString());
                return false;
            }
            this.Client.Connect("tcp://localhost:" + this.Port.ToString());
            this.Client.SendTimeout = new TimeSpan(0, 0, this.SendTimeout);
            this.Client.ReceiveTimeout = new TimeSpan(0, 0, this.ReceiveTimeout);


            var serializer = MessagePackSerializer.Get<ClientMessage>();
            byte[] SerializedMessage = serializer.PackSingleObject(message);
            //string SerializedMessage = JsonConvert.SerializeObject(message);
            this.Client.Send(SerializedMessage);
            return true;
        }

        public Tuple<Dictionary<String, String>, String, List<String>> Receive2()
        {
            var message = new byte[96];
            //System.Text.Encoding.UTF8.GetBytes (myString)
            var aaa = this.Client.ReceiveMessage();
            //int result = this.Client.Receive(message);
            var byteStream = aaa.First.Buffer;
            var serializer = MessagePackSerializer.Get<Tuple<Dictionary<String, String>, String, List<String>>>();
            var reply = serializer.UnpackSingleObject(byteStream);
            return reply;
        }

        public ServerMessage Receive()
        {
            return new ServerMessage();
        }
        private byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public void CommandLoop(ISession Session, ClientMessage Message)
        {
            //ServerMessage Reply = GeneralProtocol.NewReply();
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
            {
                this.Client = client;
                this.Send(Message);

                do
                {

                    if (GeneralProtocol.CheckForExit(Message))
                    {
                        if (GeneralProtocol.CheckForError(Message))
                            Session.Alert(Message.GetPayloadAsString());
                        break;
                    }

                    var Reply = this.Receive2();

                    //introspection should begin here

                    //Execute command,
                    //Send reply,
                    //Check if time to exit (either non-parsable message or "EXIT" received)
                    //Message = Session.DispatchReply(Reply);
                    //Type myType = Type.GetType("Autodesk.AutoCAD.ApplicationServices.Application");
                    //MemberInfo[] foo = typeof(Application).GetMember("DocumentManager");

                    //GET
                    if(Reply.Item1["method"] == "GET")
                    {
                        var parentObject = Names[Reply.Item1["namespace"]];
                        string commandstring = Reply.Item2; 
                        var obj = FollowPropertyPath(parentObject, commandstring);
                        var currentguid = Guid.NewGuid();
                        //CurrentObjects.Add(currentguid, obj);
                        Names.Add(currentguid.ToString(), obj);
                    }
                    

                    //var aaaa = foo.GetValue(Application);
                    //var aas = Application.DocumentManager.MdiActiveDocument;


                } while (true);
            }
        }


        public static object FollowPropertyPath(object value, string path)
        {
            Type currentType = value.GetType();

            foreach (string propertyName in path.Split('.'))
            {
                PropertyInfo property = currentType.GetProperty(propertyName);
                value = property.GetValue(value, null);
                currentType = property.PropertyType;
            }
            return value;
        }

        public ServerMessage Deserialize(string Reply)
        {
            var retval = new ServerMessage();
            retval = GeneralProtocol.NewServerError("Server not reached or unknown server error, exiting");
            return retval;
        }

        public string Serialize(ClientMessage Message)
        {
            var retval = "";
            retval = "SERIALIZATION FAILED!";
            return retval;
        }

    }
    
}