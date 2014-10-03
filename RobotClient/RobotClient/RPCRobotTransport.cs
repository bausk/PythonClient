using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using ZeroMQ;
using MsgPack.Serialization;
using RobotOM;

namespace Draftsocket
{
    public interface ITransport
    {
        bool Send2(Tuple<Dictionary<String, String>, String, List<String>> messag);
        Tuple<Dictionary<String, String>, String, List<String>> Receive2();
        void CommandLoop(RobotApplication robot);
        int SendTimeout { get; set; }
        int ReceiveTimeout { get; set; }
        int Port { get; set; }
    }

    public class RPCTransport : ITransport
    {
        public int SendTimeout { get; set; }
        public int ReceiveTimeout { get; set; }
        public int Port { get; set; }
        public ZmqSocket Client { get; set; }
        public Dictionary<Guid, object> CurrentObjects { get; set; }
        public Dictionary<string, object> Names = new Dictionary<string, object>()
            {
                {"Robot","Robot"},
            };

        public RPCTransport()
        {
            SendTimeout = 1;
            ReceiveTimeout = 10;
            Port = 5558;
        }

        public RPCTransport(int port)
        {
            SendTimeout = 1;
            ReceiveTimeout = 10; 
            Port = port;
        }
        public bool Send2(Tuple<Dictionary<String, String>, String, List<String>> message)
        {
            this.Client.SendTimeout = new TimeSpan(0, 0, this.SendTimeout);
            var serializer = MessagePackSerializer.Get<Tuple<Dictionary<String, String>, String, List<String>>>();
            byte[] SerializedMessage = serializer.PackSingleObject(message);
            this.Client.Send(SerializedMessage);
            return true;
        }

        public Tuple<Dictionary<String, String>, String, List<String>> Receive2()
        {
            //this.Client.ReceiveTimeout = new TimeSpan(0, 0, this.ReceiveTimeout);
            var aaa = this.Client.ReceiveMessage();
            var byteStream = aaa.First.Buffer;
            var serializer = MessagePackSerializer.Get<Tuple<Dictionary<String, String>, String, List<String>>>();
            var reply = serializer.UnpackSingleObject(byteStream);
            return reply;
        }

        public Tuple<Dictionary<String, String>, String, List<String>> new_message(
            string id,
            string method,
            string header,
            params object [] parameters)
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

        public void CommandLoop(RobotApplication robot)
        {
            //ServerMessage Reply = GeneralProtocol.NewReply();
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REP))
            {
                this.Client = client;
                this.Client.Connect("tcp://localhost:" + this.Port.ToString());
                //this.Client.Connect("tcp://localhost:" + this.Port.ToString());
                do
                {
                    var outgoing_message = new_message(
                        "1001",
                        "GET",
                        "eh",
                        "none"
                        );
                    //this.Send2(outgoing_message);
                    var Reply = this.Receive2();
                    if (Reply.Item1["method"] == "GET")
                    {
                        var parentObject = Names[Reply.Item1["namespace"]];
                        string commandstring = Reply.Item2;
                        var obj = FollowPropertyPath(parentObject, commandstring);
                        var currentguid = Guid.NewGuid();
                        //CurrentObjects.Add(currentguid, obj);
                        Names.Add(currentguid.ToString(), obj);
                        //Command executed. build new message. and put object GUID in Item2
                        outgoing_message = new_message(Reply.Item1["message_id"], "GET", currentguid.ToString(), "none");
                        var result = this.Send2(outgoing_message);
                    }
                    if (Reply.Item1["method"] == "INVOKE")
                    {
                        var parentObject = Names[Reply.Item1["namespace"]];
                        string method = Reply.Item2;
                        string parameter = Reply.Item3[0];
                        MethodInfo m = parentObject.GetType().GetMethod(method, new Type[] { typeof(string) });
                        object result = m.Invoke(parentObject, new object[] { parameter });

                    }
                    if (Reply.Item1["method"] == "METHOD")
                    {
                        int i1 = robot.Project.Structure.Nodes.FreeNumber;
                        robot.Project.Structure.Nodes.Create(i1,
                            Convert.ToDouble(Reply.Item3[0], CultureInfo.InvariantCulture),
                            Convert.ToDouble(Reply.Item3[1], CultureInfo.InvariantCulture),
                            Convert.ToDouble(Reply.Item3[2], CultureInfo.InvariantCulture)
                            );
                        int i2 = robot.Project.Structure.Nodes.FreeNumber;
                        robot.Project.Structure.Nodes.Create(i2,
                            Convert.ToDouble(Reply.Item3[3], CultureInfo.InvariantCulture),
                            Convert.ToDouble(Reply.Item3[4], CultureInfo.InvariantCulture),
                            Convert.ToDouble(Reply.Item3[5], CultureInfo.InvariantCulture)
                            );
                        robot.Project.Structure.Bars.Create(robot.Project.Structure.Bars.FreeNumber,i1,i2);
                        robot.Project.ViewMngr.Refresh();
                    }


                    //var aaaa = foo.GetValue(Application);
                    //var aas = Application.DocumentManager.MdiActiveDocument;
                    this.Send2(outgoing_message);

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

    }

}