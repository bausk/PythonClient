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
        private Document doc { get; set; }
        private Database db { get; set; }
        private Editor ed { get; set; }
        public Dictionary<string, object> SavedObjects { get; set; }
        private Transport transport { get; set; }
        private Queue<ServerMessage> CurrentReplyStack { get; set; }
        private Transaction CurrentTransaction { get; set; }
        public ClientMessage CurrentMessage { get; set; }
        private ServerMessage CurrentReply { get; set; }
        private Boolean CurrentTransactionActive { get; set; }

        public Session(Transport tr)
        {
            this.doc = Application.DocumentManager.MdiActiveDocument;
            this.db = doc.Database;
            this.ed = doc.Editor;
            this.transport = tr;
            this.transport.SendTimeout = 1;
            this.transport.ReceiveTimeout = 1;
            this.transport.Port = 5556;
            this.SavedObjects = new Dictionary<string, object>();
            this.CurrentReplyStack = new Queue<ServerMessage>();
            this.CurrentReply = null;
            this.CurrentMessage = null;
            this.CurrentTransactionActive = false;
        }

        public ClientMessage DispatchReply(ServerMessage RawReply)
        {
            this.CurrentReply = RawReply;
            //this.CurrentMessage = new ClientMessage();
            this.CurrentMessage = null;
            if (this.CurrentReply.Action == Protocol.CommonAction.BATCH)
            {
                this.CurrentMessage = new ClientMessage(Protocol.CommonAction.BATCH, Protocol.Status.OK);
                foreach (string SerializedReply in this.CurrentReply.GetPayloadAsStringList())
                    //Add all nested messages to the message stack
                    this.CurrentReplyStack.Enqueue(this.transport.Deserialize(SerializedReply));
            }
            else
            {
                this.CurrentReplyStack.Enqueue(this.CurrentReply);
            }


            while (this.CurrentReplyStack.Count > 0)
            {
                var Message = new ClientMessage();
                var Reply = this.CurrentReplyStack.Dequeue();
                Message = this.ExecuteServerMessage(Reply);
                if (this.CurrentMessage.Action == Protocol.CommonAction.BATCH)
                {
                    this.CurrentMessage.AddPayloadItem(this.transport.Serialize(Message));
                }
                else
                {
                    this.CurrentMessage = Message;
                }
                //WORK HERE: current implementation accounts for batch variant only
                
            }

            //Checking whether remote server wants to finish session gracefully
            if (Protocol.CheckForExit(this.CurrentReply))
                //The current server message is the last one.
                //Inform server about us finishing (while still returning our message).
                this.CurrentMessage.SetFinishedStatus();

            //Check whether remote server has hung up altogether
            if (Protocol.CheckForTermination(this.CurrentReply))
                this.CurrentMessage = new ClientMessage(Protocol.CommonAction.TERMINATE, Protocol.Status.FINISH);

            this.CurrentMessage.Callback = this.CurrentReply.Callback; //highly doubtful we should do it here
            return this.CurrentMessage;
        }

        public void DispatchStack(ServerMessage RawReply)
        {
            if (RawReply.Action == Protocol.CommonAction.BATCH)
            {
                this.CurrentMessage = new ClientMessage(Protocol.CommonAction.BATCH, Protocol.Status.OK);
                foreach (string SerializedReply in RawReply.GetPayloadAsStringList())
                    //Add all nested messages to the message stack
                    this.CurrentReplyStack.Enqueue(this.transport.Deserialize(SerializedReply));

                while (this.CurrentReplyStack.Count > 0)
                {
                    var Message = new ClientMessage();
                    var Reply = this.CurrentReplyStack.Dequeue();
                    Message = this.ExecuteServerMessage(Reply);
                    this.CurrentMessage.AddPayloadItem(this.transport.Serialize(Message));
                }
            }
            else
            {
                this.CurrentMessage = this.ExecuteServerMessage(RawReply);
            }

            //Checking whether remote server wants to finish session gracefully
            if (Protocol.CheckForExit(this.CurrentReply))
                //The current server message is the last one.
                //Inform server about us finishing (while still returning our message).
                this.CurrentMessage.SetFinishedStatus();

            //Check whether remote server has hung up altogether
            if (Protocol.CheckForTermination(this.CurrentReply))
                this.CurrentMessage = new ClientMessage(Protocol.CommonAction.TERMINATE, Protocol.Status.FINISH);

            this.CurrentMessage.Callback = this.CurrentReply.Callback; //highly doubtful we should do it here
        }

        private ClientMessage ExecuteServerMessage(ServerMessage Reply)
        {
            var Message = new ClientMessage();

            switch (Reply.Action)
            {
                case Protocol.CommonAction.TERMINATE:
                    //Don't do anything, return a TERM message to server
                    Message = new ClientMessage(Protocol.CommonAction.TERMINATE, Protocol.Status.FINISH);
                    break;
                case Protocol.CommonAction.BATCH:
                    //Batch will invoke a recursive call of DispatchReply
                    //Development note: Only batch messages that have nothing to do with transactions
                    //should be allowed here
                    Message = this.Batch(Reply);
                    break;
                case Protocol.ServerAction.SETEVENT:
                    Message = this.SetEvent(Reply);
                    break;
                case Protocol.AutocadAction.REQUEST_USER_INPUT:
                    Message = this.GetStrings(Reply);
                    break;
                case Protocol.ServerAction.WRITE:
                    Message = this.Write(Reply);
                    break;
                case Protocol.AutocadAction.GET_ENTITY_ID:
                    Message = this.GetEntity(Reply);
                    break;
                case Protocol.AutocadAction.GET_KEYWORD:
                    Message = this.GetKeyword(Reply);
                    break;
                case Protocol.AutocadAction.GET_DB_OBJECTS:
                    Message = this.GetDBObjects(Reply);
                    break;
                case Protocol.AutocadAction.MANIPULATE_DB:
                    break;
                case Protocol.ServerAction.TRANSACTION_START:
                    Message = this.TransactionManager(Reply);
                    break;
                case Protocol.ServerAction.TRANSACTION_COMMIT:
                    Message = new ClientMessage(Protocol.ClientAction.CONTINUE, Protocol.Status.OK);
                    break;
                case Protocol.ServerAction.TRANSACTION_ABORT:
                    break;
                default:
                    Message = new ClientMessage(Protocol.CommonAction.TERMINATE, Protocol.Status.FINISH);
                    break;
            }

            Message.Callback = Reply.Callback; //highly doubtful we should do it here
            return Message;
        }

        public List<Dictionary<string, object>> ObjectsToDicts<T>(List<T> listobj)
        {
            List<Dictionary<string, object>> retval = new List<Dictionary<string, object>>();
            foreach (object obj in listobj)
            {
                Dictionary<string, object> member = new Dictionary<string, object>();
                member.Add(Protocol.Keywords.DEFAULT, obj);
                //Todo: if there's a name, add name as name
                //WORK HERE

                Type Type = obj.GetType();
                var obj2 = obj as PromptEntityResult;
                if(obj2 != null)
                {
                    member.Add(Protocol.Local.ObjectID, obj2.ObjectId.ToString());
                    member.Add(Protocol.Local.Handle, obj2.ObjectId.Handle.Value);
                    member.Add(Protocol.Local.TypeName, obj2.GetType().Name);
                }
                retval.Add(member);
            }
            return retval;
        }

    }
}
