using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace SocketWrapper
{
    public static class Protocol
    {
        public struct ClientAction
        {
            //client-issued action lines
            public const string CMD = "COMMAND";
            public const string ERROR = "CLIENT_ERROR";
            public const string EVENT = "EVENT";
            public const string CONTINUE = "CONTINUE";
            public const string CONSOLE = "CONSOLE";
        }

        public struct ServerAction
        {
            //server-issued action lines
            public const string SETEVENT = "SET_EVENT";
            public const string WRITE = "WRITE_MESSAGE";
            public const string REQUEST_USER_INPUT = "REQUEST_INPUT";
            public const string REQUEST_SEVERAL_USER_INPUTS = "REQUEST_SEVERAL_INPUTS";
            public const string TERMINATE = "TERMINATE_SESSION";
            public const string GET_ENTITY_ID = "GET_ENTITY";
            public const string TRANSACTION_START = "TRANSACTION_START";
            public const string TRANSACTION_COMMIT = "TRANSACTION_COMMIT";
            public const string TRANSACTION_ABORT = "TRANSACTION_ABORT";
            public const string TRANSACTION_GETOBJECT = "TR_GET_OBJECT";
            public const string TRANSACTION_MANIPULATE_DB = "TR_MANIPULATE_DB";
        }

        //status lines
        public struct Status
        {
            public const string FINISH = "_FINISH";
            public const string OK = "_OK";
            public const string ONHOLD = "_ONHOLD";
            public const string SERVER_ERROR = "_SERVERERROR";
        }

        public struct PayloadTypes
        {
            public const string STRING = "STRING";
            public const string LIST = "LIST";
            public const string DICT = "DICT";
            public const string LISTOFSTRINGS = "LISTOFSTRINGS";
        };

        public struct Keywords
        {
            public const string DEFAULT = "DEFAULT";
            public const string OBJECT = "OBJECT";
        };

        public static Dictionary<string, Type> EntityTypes = new Dictionary<string, Type>()
        {
            {"LINE",typeof(Line)},
            {"CURVE",typeof(Curve)},
            {"CIRCLE",typeof(Circle)},
            {"HATCH",typeof(Hatch)},
        };

        public static bool CheckForClientExit(SocketMessage Message)
        {
            if (Message.Status == Protocol.Status.FINISH)
                return true;
            else
                return false;
        }
        public static bool CheckForServerCleanExit(SocketMessage Reply)
        {
            if (Reply.Status == Protocol.Status.FINISH)
                return true;
            else
                return false;
        }
        public static bool CheckForTermination(SocketMessage Reply)
        {
            if (Reply.Action == Protocol.ServerAction.TERMINATE)
                return true;
            else
                return false;
        }

        //client command factories
        public static ClientMessage NewCommand(string Name)
        {
            return new ClientMessage(Protocol.ClientAction.CMD, Protocol.Status.OK, Name);
        }

        public static ServerMessage NewReply()
        {
            return new ServerMessage();
        }

        public static ServerMessage NewServerError(string Prompt)
        {
            ServerMessage M = new ServerMessage(Protocol.ServerAction.TERMINATE, Protocol.Status.SERVER_ERROR);
            M.Payload = Prompt;
            return M;
        }
    }
}
