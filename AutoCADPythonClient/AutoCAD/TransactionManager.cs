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

            using (this.CurrentTransaction)
            {
                while (true)
                {
                    while (true)
                    {
                        ClientMessage Message = new ClientMessage(Protocol.ClientAction.CONTINUE, Protocol.Status.OK);
                        if (Reply.Action == Protocol.ServerAction.TRANSACTION_START) ;
                        else if (Reply.Action == Protocol.ServerAction.TRANSACTION_COMMIT)
                        {
                            //return control to DispatchStack() if transaction end found
                            this.CurrentTransaction.Commit();
                            return Message;
                        }
                        else if (Reply.Action == Protocol.ServerAction.TRANSACTION_ABORT)
                        {
                            //return control to DispatchStack() if transaction end found
                            this.CurrentTransaction.Dispose();
                            return Message;
                        }
                        else
                        {
                            Message = this.ExecuteServerMessage(Reply);
                        }

                        if (this.CurrentMessage == null) //CurrentMessage is set in two different ways depending on
                                                         //whether it's a simple message (then CM == null) or batch
                        {
                            this.CurrentMessage = Message;
                            this.CurrentMessage.Callback = Reply.Callback; //only suspect in here
                        }
                        else
                            this.CurrentMessage.AddPayloadItem(this.transport.Serialize(Message));

                        if (this.CurrentReplyStack.Count > 0)
                            Reply = this.CurrentReplyStack.Dequeue();
                        else
                            break;
                    }
                        

                    this.transport.Send(this.CurrentMessage);
                    //if (GeneralProtocol.CheckForExit(Message))
                    //    break;
                    this.CurrentReply = this.transport.Receive();
                    this.CurrentReplyStack.Clear();
                    this.CurrentMessage = null;

                    if (GeneralProtocol.CheckForExit(this.CurrentReply))
                    { //if a finishing message is received here, there is an error in server's logic
                        //this.CurrentTransaction.Abort();
                        this.CurrentTransaction.Dispose();
                        return GeneralProtocol.NewClientError("\nERROR: Server finished the current command while inside a transaction. Review server-side procedure.\n");
                    }

                    //We duplicate DispatchReply logic of identifying batch and enqueuing reply(-ies).
                    if (this.CurrentReply.Action == Protocol.CommonAction.BATCH)
                    {
                        this.CurrentMessage = new ClientMessage(Protocol.CommonAction.BATCH, Protocol.Status.OK);
                        this.CurrentMessage.Callback = this.CurrentReply.Callback;
                        foreach (string SerializedReply in this.CurrentReply.GetPayloadAsStringList())
                            //Add all nested messages to the message stack
                            this.CurrentReplyStack.Enqueue(this.transport.Deserialize(SerializedReply));
                        Reply = this.CurrentReplyStack.Dequeue();
                    }
                    else
                    {
                        Reply = this.CurrentReply;
                        //CHANGE TO ENQUEUE!!
                    }

                } //End of cycle executed while transaction is active

            }

        }
    }
}