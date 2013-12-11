using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace Draftsocket
{
    public abstract class GeneralProtocol
    {

        public static class ClientAction
        {
            //client-issued action lines
            public static readonly string CMD = "COMMAND";
            public static readonly string ERROR = "CLIENT_ERROR";
            public static readonly string EVENT = "EVENT";
            public static readonly string CONTINUE = "CONTINUE";
            public static readonly string CONSOLE = "CONSOLE";
        }

        public static class ServerAction
        {
            //server-issued action lines
            public const string WRITE = "WRITE";
            public const string SETEVENT = "SET_EVENT";
            public const string TRANSACTION_START = "TR_START";
            public const string TRANSACTION_COMMIT = "TR_COMMIT";
            public const string TRANSACTION_ABORT = "TR_ABORT";
        }

        public static class CommonAction
        {
            //common action lines
            public const string TERMINATE = "CA_TERMINATE";
            public const string BATCH = "CA_BATCH";
        }
        //status lines
        public static class Status
        {
            public const string FINISH = "_FINISH";
            public const string OK = "_OK";
            public const string ONHOLD = "_ONHOLD";
            public const string TERMINATE = "_TERMINATE";
        }

        public static class Keywords
        {
            public const string DEFAULT = "DEFAULT";
        };

        public static bool CheckForExit(SocketMessage Message)
        {
            if (Message.Status == GeneralProtocol.Status.FINISH)
                return true;
            else
                return false;
        }
        public static bool CheckForTermination(SocketMessage Reply)
        {
            if (Reply.Status == GeneralProtocol.Status.TERMINATE)
                return true;
            else
                return false;
        }

        //client command factories
        public static ClientMessage NewCommand(string Name)
        {
            return new ClientMessage(GeneralProtocol.ClientAction.CMD, GeneralProtocol.Status.OK, Name);
        }

        public static ServerMessage NewReply()
        {
            return new ServerMessage();
        }

        public static ServerMessage NewServerError(string Prompt)
        {
            //Called when server is not reached or the input can't be understood.
            //Dispatching this message will cause client to
            //write a message from Prompt argument,
            //issue a TERMINATE command to server (causing it to stop servicing the current callback without calling the next State),
            //and exit the command loop permanently.
            ServerMessage M = new ServerMessage(GeneralProtocol.ServerAction.WRITE, GeneralProtocol.Status.TERMINATE);
            M.SetPayload(Prompt);
            return M;
        }
    }
}
