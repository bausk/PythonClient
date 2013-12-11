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
  

        public bool Send(ZmqSocket client, ClientMessage message)
        {
            if (client.ReceiveStatus == ZeroMQ.ReceiveStatus.TryAgain)
            {
                //This part has to wait until new pattern implementation
                //Dealer/router for example
                client.Disconnect("tcp://localhost:" + this.Port.ToString());
                return false;
            }
            client.Connect("tcp://localhost:" + this.Port.ToString());
            client.SendTimeout = new TimeSpan(0, 0, this.SendTimeout);
            client.ReceiveTimeout = new TimeSpan(0, 0, this.ReceiveTimeout);
            string SerializedMessage = JsonConvert.SerializeObject(message);
            client.Send(SerializedMessage, Encoding.Unicode);
            return true;
        }

        public ServerMessage Receive(ZmqSocket client)
        {
            ServerMessage response = new ServerMessage();
            string SerializedReply = client.Receive(Encoding.UTF8);//.Remove(0,1);
            try
            {
                response = JsonConvert.DeserializeObject<ServerMessage>(SerializedReply);
            }
            catch (ArgumentNullException ex)
            {
                response = GeneralProtocol.NewServerError("Server not reached or unknown server error, exiting");
            }
            return response;
        }

        public void CommandLoop(ISession Session, ClientMessage Message)
        {
            ServerMessage Reply = GeneralProtocol.NewReply(); 
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
            {
                do
                {
                    this.Send(client, Message);
                    if (GeneralProtocol.CheckForExit(Message))
                        break;
                    Reply = this.Receive(client);
                    Message = Session.DispatchReply(Reply);
                } while (true);
            }
        }
    }
    


}