﻿using System;
using System.Collections.Generic;
//using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace Draftsocket
{
    public class AutoCAD : ISession
    {
        private Document doc { get; set; }
        private Database db { get; set; }
        private Editor ed { get; set; }
        public Dictionary<string, object> SavedObjects { get; set; }

        public struct AutoCADKeywords
        {
            //AutoCAD keywords
            public const string Prompt = "PROMPT";
            public const string RejectMessage = "REJECTMESSAGE";
            public const string AllowedClass = "ALLOWEDCLASS";
            public const string Name = "NAME";
        }

        public AutoCAD(Transport transport)
        {
            this.doc = Application.DocumentManager.MdiActiveDocument;
            this.db = doc.Database;
            this.ed = doc.Editor;
            transport.SendTimeout = 1;
            transport.ReceiveTimeout = 1;
            transport.Port = 5556;
            this.SavedObjects = new Dictionary<string, object>();
        }

        public ClientMessage DispatchReply(ServerMessage Reply)
        {
            ClientMessage Message = new ClientMessage();
            switch (Reply.Action)
            {
                case Protocol.CommonAction.TERMINATE:
                    //Don't do anything, return a TERM message to server
                    Message = new ClientMessage(Protocol.CommonAction.TERMINATE, Protocol.Status.FINISH);
                    break;
                case Protocol.ServerAction.SETEVENT:
                    Message = this.SetEvent(Reply);
                    break;
                case Protocol.ServerAction.REQUEST_USER_INPUT:
                    Message = this.GetStrings(Reply);
                    break;
                case Protocol.ServerAction.WRITE:
                    Message = this.Write(Reply);
                    break;
                case Protocol.ServerAction.GET_ENTITY_ID:
                    Message = this.GetEntity(Reply);
                    break;
                case Protocol.ServerAction.TRANSACTION_START:
                    Message = this.Transaction(Reply);
                    break;
                case Protocol.ServerAction.TRANSACTION_GETOBJECT:
                    break;
                case Protocol.ServerAction.TRANSACTION_MANIPULATE_DB:
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
            if (Protocol.CheckForExit(Reply))
                //The current server message is the last one.
                //Return the result, whatever it is, and inform about Client finishing.
                Message.SetFinishedStatus();
            if (Protocol.CheckForTermination(Reply))
                Message = new ClientMessage(Protocol.CommonAction.TERMINATE, Protocol.Status.FINISH);

            Message.Callback = Reply.Callback; //highly doubtful we should do it here
            return Message;
        }

        private ClientMessage GetStrings(ServerMessage reply)
        {
            List<string> prompts = reply.GetPayloadAsStringList();
            List<string> replies = new List<string>();
            ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
            foreach (string prompt in prompts)
            {
                PromptStringOptions pso = new PromptStringOptions("\n" + prompt);
                PromptResult pr = ed.GetString(pso);
                if (pr.Status != PromptStatus.OK)
                {
                    //WORK HERE - will need to process canceled inputs
                }
                else
                {
                    replies.Add(pr.StringResult);
                }
            }
            message.SetPayload(replies);
            return message;
        }

        private ClientMessage GetEntity(ServerMessage reply)
        {

            List<Dictionary<string,object>> Prompts = reply.Payload;
            List<PromptEntityResult> Result = new List<PromptEntityResult>();
            foreach (Dictionary<string, object> PromptDict in Prompts)
            {
                PromptEntityOptions peo;
                try
                {
                    peo = new PromptEntityOptions((string)PromptDict[AutoCADKeywords.Prompt]);
                }
                catch
                {
                    return new ClientMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);
                }

                object value;
                if (PromptDict.TryGetValue(AutoCADKeywords.RejectMessage, out value))
                    peo.SetRejectMessage((string)value);

                if (PromptDict.TryGetValue(AutoCADKeywords.AllowedClass, out value))
                    foreach (string Type in (List<string>)value)
                        peo.AddAllowedClass(Protocol.EntityTypes[Type], false);

                PromptEntityResult per = ed.GetEntity(peo);

                //Add the result to payload (SetPayload called later to form the message).
                Result.Add(per);

                //Memoization: if a name field was set in the payload item of reply message,
                //keep the result (per) in SavedObjects
                if (PromptDict.TryGetValue(AutoCADKeywords.Name, out value))
                    this.SavedObjects.Add((string) value, per);
            }

            ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
            List<Dictionary<string,object>> DictResult = this.ObjectsToDicts(Result);
            message.SetPayload(DictResult);
            return message;
        }

        private ClientMessage Transaction(ServerMessage reply)
        {

            /*PromptStringOptions pso = new PromptStringOptions("\n" + reply.Payload);
            //bool value = true;
            object value;
            if (reply.Parameters.TryGetValue("AllowSpaces", out value))
                pso.AllowSpaces = (bool)value;
            else
                pso.AllowSpaces = true;

            PromptResult pr = ed.GetString(pso);

            if (pr.Status != PromptStatus.OK)
                return new SocketMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);*/

            ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
            message.SetPayload("No result");
            return message;
        }

        private ClientMessage Write(ServerMessage reply)
        {
            ed.WriteMessage(String.Join(String.Empty, reply.GetPayloadAsStringList()));
            ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
            return message;
        }

        private ClientMessage SetEvent(ServerMessage reply)
        {
            this.db.ObjectAppended += new ObjectEventHandler(OnObjectCreated);
            SocketMessage message = new SocketMessage();
            switch (reply.Payload.ToString())
            {
                case "ObjectCreated":
                    db.ObjectAppended += new ObjectEventHandler(OnObjectCreated);
                    break;
                case "CommandEnded":
                    break;
                default:
                    message.Action = "END";
                    return new ClientMessage();
            }
            message.Action = "OK";
            return new ClientMessage();
        }

        public void OnObjectCreated(object sender, ObjectEventArgs e)
        {
            // Callback binder for real Python event handler
            // We should prepare a message and send it to Python
            // to initiate a session
            //var a = e.DBObject.ObjectId;
            //SocketWrapper.AutoCAD AutoCADWrapper = new SocketWrapper.AutoCAD();
            /*string our_reply = "";
            using (ZmqContext context = ZmqContext.Create())
            using (ZmqSocket client = context.CreateSocket(SocketType.REQ))
            {
                this.Message = new SocketMessage("Init Event", "STRING", "OnObjectCreated");
                do
                {
                    string response = this.SendMessage(client, this.Message);
                    our_reply = this.DispatchReply(this.Reply);
                    our_reply = "END";
                } while (!our_reply.Equals("END"));

            }*/

        }

        public List<Dictionary<string, object>> ObjectsToDicts<T>(List<T> listobj)
        {
            List<Dictionary<string, object>> retval = new List<Dictionary<string, object>>();
            foreach (object obj in listobj)
            {
                Dictionary<string, object> member = new Dictionary<string, object>();
                member.Add(Protocol.Keywords.DEFAULT, obj);

                Type Type = obj.GetType();
                var obj2 = obj as PromptEntityResult;
                if(obj2 != null)
                {
                    member.Add("ObjectID", obj2.ObjectId.ToString());
                    member.Add("Handle", obj2.ObjectId.Handle.Value);
                }
                retval.Add(member);
            }
            return retval;
        }

    }
}
