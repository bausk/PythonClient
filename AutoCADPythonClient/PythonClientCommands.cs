// (C) Copyright 2013 by Microsoft 
//
//using System;
//using Microsoft.CSharp;
//using System.Text;
//using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using ZeroMQ;
using SocketWrapper;


// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ShadowbinderClient.PythonCommands))]
namespace ShadowbinderClient
{
    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!

    public class PythonCommands
    {
        //SortedList<string, string> _blockNames = null;

        //OK let's try an event handler
        //http://through-the-interface.typepad.com/through_the_interface/2010/02/watching-for-deletion-of-a-specific-autocad-block-using-net.html
        //

        [CommandMethod("REPENT")]
        public void SetEventHandler()
        {
            SocketWrapper.AutoCAD Session = new SocketWrapper.AutoCAD();
            SocketMessage Message = Protocol.NewCommand("REPENT");
            
            Transport.CommandLoop(Session, Message);

            /*bool exitflag = false;
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
            {
                do
                {
                    Reply = Transport.SendMessage(client, Message);
                    exitflag = Protocol.CheckForTermination(Reply); //client stops without doing any work
                    if (!exitflag)
                    {
                        Message = Session.DispatchReply(Reply);
                        exitflag = Protocol.CheckForServerExit(Reply); //server sends FINISH status, client does work and stops without initiating new message pair
                    }
                } while (!exitflag);

            }*/

        }
    }
}
