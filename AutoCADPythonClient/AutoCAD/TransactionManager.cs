using System;
using System.Collections.Generic;
//using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace Draftsocket.AutoCAD
{
    public partial class Session : ISession
    {
        private ClientMessage TransactionManager(ServerMessage reply)
        {
            var Reply = reply;
            this.CurrentTransaction = this.doc.Database.TransactionManager.StartTransaction();
            //preset message to generic "okay"
            var Message = new ClientMessage(Protocol.ClientAction.CONTINUE, Protocol.Status.OK);

            if (this.CurrentMessage != null)
            {
                this.CurrentMessage.AddPayloadItem(this.transport.Serialize(Message));
            }


            using (this.CurrentTransaction)
            {
                while (true)
                {
                    if (this.CurrentMessage == null)
                    { //this part is executed when non-batch message is received
                        //We are in a non-batch so we should set CurrentMessage ourselves
                        //WORK HERE:
                        //current framework does not account for non-batch messages received within transaction manager
                        if (Reply.Action == Protocol.ServerAction.TRANSACTION_START)
                        {
                            Message = new ClientMessage(Protocol.ClientAction.CONTINUE, Protocol.Status.OK);
                            this.CurrentMessage = Message;
                        }
                        else if (Reply.Action == Protocol.ServerAction.TRANSACTION_COMMIT)
                        {
                            this.CurrentTransaction.Commit();
                            return new ClientMessage(Protocol.ClientAction.CONTINUE, Protocol.Status.OK);
                        }
                        else if (Reply.Action == Protocol.ServerAction.TRANSACTION_COMMIT)
                        {
                            this.CurrentTransaction.Abort();
                            return new ClientMessage(Protocol.ClientAction.CONTINUE, Protocol.Status.OK);
                        }
                        else
                        {
                            Message = this.ExecuteServerMessage(Reply);
                        }

                    } //end of part executed for a single, non-batch message
                    else //this part is executed when batch message is not exhausted
                    {
                        //Add our input to batch, and exhaust ReplyStack, i.e. duplicate Dispatch() functions before calling Execute()
                        
                        while (this.CurrentReplyStack.Count > 0)
                        {
                            Reply = this.CurrentReplyStack.Dequeue();

                            if (Reply.Action == Protocol.ServerAction.TRANSACTION_COMMIT)
                            {
                                //return control to DispatchStack() if transaction end found
                                this.CurrentTransaction.Commit();
                                return new ClientMessage(Protocol.ClientAction.CONTINUE, Protocol.Status.OK);
                            }
                            else if (Reply.Action == Protocol.ServerAction.TRANSACTION_ABORT)
                            {
                                //return control to DispatchStack() if transaction end found
                                this.CurrentTransaction.Abort();
                                return new ClientMessage(Protocol.ClientAction.CONTINUE, Protocol.Status.OK);
                            }

                            Message = this.ExecuteServerMessage(Reply);
                            this.CurrentMessage.AddPayloadItem(this.transport.Serialize(Message));
                        }
                    }//end of part executed while batch message is not exhausted

                    this.CurrentMessage.Callback = this.CurrentReply.Callback;

                    this.transport.Send(this.CurrentMessage);
                    this.CurrentMessage = null;
                    //if (GeneralProtocol.CheckForExit(Message))
                    //    break;
                    Reply = this.transport.Receive();

                    //We duplicate DispatchReply logic of identifying batch and enqueuing reply(-ies).
                    if (Reply.Action == Protocol.CommonAction.BATCH)
                    {
                        this.CurrentMessage = new ClientMessage(Protocol.CommonAction.BATCH, Protocol.Status.OK);
                        this.CurrentMessage.Callback = Reply.Callback;
                        foreach (string SerializedReply in Reply.GetPayloadAsStringList())
                            //Add all nested messages to the message stack
                            this.CurrentReplyStack.Enqueue(this.transport.Deserialize(SerializedReply));
                    }
                    else
                    {
                        //Reply = this.CurrentReply;
                    }

                } //End of cycle executed while transaction is active

            }

        }
    }
}