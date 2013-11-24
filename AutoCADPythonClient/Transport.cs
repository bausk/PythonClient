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
  
        public ServerMessage SendMessage(ZmqSocket client, ClientMessage message)
        {
            ServerMessage response = new ServerMessage();
            client.Connect("tcp://localhost:" + this.Port.ToString());
            client.SendTimeout = new TimeSpan(0, 0, this.SendTimeout);
            client.ReceiveTimeout = new TimeSpan(0, 0, this.ReceiveTimeout);
            string SerializedMessage = JsonConvert.SerializeObject(message);
            //substitute for testing GetEntity
            client.Send(SerializedMessage, Encoding.Unicode);
            if (Protocol.CheckForClientExit(message))
            {
                //Client exits without waiting for reply
                //bypass client.Receive. Emulate server exit to break dispatch loop
                response.Action = Protocol.ServerAction.TERMINATE;
            }
            else
            {
                //Client waits for reply
                
                //string SerializedReply = "{\"Status\": \"_ONHOLD\", \"ContentType\": \"NONE\", \"Parameters\": {\"Prompt\": \"Choose first entity\"}, \"Callback\": \"E7C2B6230C8647059ACEC108F957D3F5\", \"Action\": \"GET_ENTITY\", \"Payload\": null}";
                //string SerializedReply = "{\"Status\": \"_ONHOLD\", \"ContentType\": \"NONE\", \"Parameters\": [{\"Prompt\": \"Choose first entity\"}, {\"Prompt\": \"Choose second entity\"}], \"Callback\": \"E7C2B6230C8647059ACEC108F957D3F5\", \"Action\": \"GET_ENTITY\", \"Payload\": null}";
                string SerializedReply = client.Receive(Encoding.UTF8);//.Remove(0,1);
                try
                {
                    response = JsonConvert.DeserializeObject<ServerMessage>(SerializedReply);
                }
                catch (ArgumentNullException ex)
                {
                    response = Protocol.NewServerError("Server not reached, exiting");
                }
            }
            return response;
        }

        public void CommandLoop(Draftsocket.AutoCAD Session, ClientMessage Message)
        {
            ServerMessage Reply = Protocol.NewReply(); 
            bool exitflag = false;
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
            {
                do
                {
                    //Substitute message
                    
                    Reply = this.SendMessage(client, Message);
                    exitflag = Protocol.CheckForTermination(Reply); //client stops.
                    Message = Session.DispatchReply(Reply);
                    if (!exitflag)
                    {
                        exitflag = Protocol.CheckForServerCleanExit(Reply); //server sends FINISH status, client does work and stops without initiating new message pair
                    }
                } while (!exitflag);
            }
        }
    }
    


}