using System;
using System.Collections.Generic;
using System.Text;
using ZeroMQ;
using Newtonsoft.Json;

namespace Draftsocket
{

    public class Transport
    {
        public int SendTimeout { get; set; }
        public int ReceiveTimeout { get; set; }
        public int Port { get; set; }
        public ZmqSocket Client { get; set; }

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
            string SerializedMessage = JsonConvert.SerializeObject(message);
            this.Client.Send(SerializedMessage, Encoding.Unicode);
            return true;
        }

        public ServerMessage Receive()
        {
            string SerializedReply = this.Client.Receive(Encoding.UTF8);//.Remove(0,1);
            var reply = this.Deserialize(SerializedReply);
            return reply;
        }

        public void CommandLoop(ISession Session, ClientMessage Message)
        {
            ServerMessage Reply = GeneralProtocol.NewReply(); 
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
            {
                this.Client = client;
                do
                {
                    this.Send(Message);
                    if (GeneralProtocol.CheckForExit(Message))
                    {
                        if (GeneralProtocol.CheckForError(Message))
                            Session.Alert(Message.GetPayloadAsString());
                        break;
                    }
                    Reply = this.Receive();
                    Message = Session.DispatchReply(Reply);
                } while (true);
            }
        }

        public ServerMessage Deserialize(string Reply)
        {
            var retval = new ServerMessage();
            try
            {
                retval = JsonConvert.DeserializeObject<ServerMessage>(Reply);
            }
            catch (ArgumentNullException ex)
            {
                retval = GeneralProtocol.NewServerError("Server not reached or unknown server error, exiting");
            }
            return retval;
        }

        public string Serialize(ClientMessage Message)
        {
            var retval = "";
            try
            {
                retval = JsonConvert.SerializeObject(Message);
            }
            catch (Exception ex)
            {
                retval = "SERIALIZATION FAILED!";
            }
            return retval;
        }

    }
    


}