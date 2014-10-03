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

    public class RPCTransport
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

        public RPCTransport(int Port) {
            this.Port = Port;
        }

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

        public bool Send2(Tuple<Dictionary<String, String>, String, List<String>> message)
        {
            this.Client.Connect("tcp://localhost:" + this.Port.ToString());
            this.Client.SendTimeout = new TimeSpan(0, 0, this.SendTimeout);
            var serializer = MessagePackSerializer.Get<Tuple<Dictionary<String, String>, String, List<String>>>();
            byte[] SerializedMessage = serializer.PackSingleObject(message);
            this.Client.Send(SerializedMessage);
            return true;
        }

        public Tuple<Dictionary<String, String>, String, List<String>> Receive2()
        {
            //var message = new byte[96];
            byte[] byteStream;
            //System.Text.Encoding.UTF8.GetBytes (myString)
            var aaa = this.Client.ReceiveMessage(new TimeSpan(0,0,5));
            //int result = this.Client.Receive(message);
            try
            {
                byteStream = aaa.First.Buffer;
            }
            catch
            {
                return new_message("whatever", "whatever", "whatever", "whatever");
            }
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

        public Tuple<Dictionary<String, String>, String, List<String>> new_message(
            string id,
            string method,
            string header,
            params object[] parameters)
        {
            List<object> list = new List<object>(parameters);
            List<string> stringList = list.ConvertAll(obj => obj.ToString());
            Tuple<Dictionary<String, String>, String, List<String>> retval = new Tuple<Dictionary<string, string>, string, List<string>>(
                           new Dictionary<string, string>()
                            {
                                {"message_id", id},
                                {"method", method},
                            },
                            header,
                            stringList
                );
            return retval;
        }


        public void CommandLoop(Tuple<Dictionary<String, String>, String, List<String>> msg)
        {
            //ServerMessage Reply = GeneralProtocol.NewReply();
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
            {
                this.Client = client;

                this.Send2(msg);
                do
                {
                    var Reply = this.Receive2();

                    if(Reply.Item1["method"] == "GET")
                    {
                        var parentObject = Names[Reply.Item1["namespace"]];
                        string commandstring = Reply.Item2; 
                        var obj = FollowPropertyPath(parentObject, commandstring);
                        var currentguid = Guid.NewGuid();
                        //CurrentObjects.Add(currentguid, obj);
                        Names.Add(currentguid.ToString(), obj);
                        //Command executed. build new message. and put object GUID in Item2
                        var outgoing_message = this.new_message(
                            Reply.Item1["message_id"],
                            "GET",
                            currentguid.ToString(),
                            "none"
                            );
                        var result = this.Send2(outgoing_message);
                    }
                    if(Reply.Item1["method"] == "INVOKE")
                    {
                        var parentObject = Names[Reply.Item1["namespace"]];
                        string method = Reply.Item2;
                        string parameter = Reply.Item3[0];
                        MethodInfo m = parentObject.GetType().GetMethod(method, new Type[] {typeof(string)});
                        object result = m.Invoke(parentObject, new object[] {parameter});

                        var outgoing_message = this.new_message(
                            Reply.Item1["message_id"],
                            "INVOKE",
                            method,
                            "none"
                            );
                        var msg_result = this.Send2(outgoing_message);

                    }
                    if (Reply.Item1["method"] == "END")
                    {
                        break;
                    }
                    if (Reply.Item1["method"] == "whatever")
                    {
                        break;
                    }


                } while (true);
            }
        }

        public void CommandLoopOnce(Tuple<Dictionary<String, String>, String, List<String>> message)
        {
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
            {
                this.Client = client;
                this.Send2(message);
                var Reply = this.Receive2();
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