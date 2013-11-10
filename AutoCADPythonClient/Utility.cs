using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Newtonsoft.Json;

namespace SocketWrapper
{

    public static class Utilities
    {

        public struct AutoCADKeywords
        {
            //AutoCAD keywords
            public const string Prompt = "Prompt";
            public const string RejectString = "RejectString";
            public const string AllowedClass = "AllowedClass";


        }

        public static SocketMessage GetEntityKeywords(object Payload)
        {
            return new SocketMessage();
        }

        public static List<Dictionary<string,object>> ParametersToList(object Params)
        {
            List<Dictionary<string, object>> Prompts = new List<Dictionary<string, object>>();
            if (Params is List<Dictionary>)
            {
                Prompts = (List<Dictionary<string, object>>)Params;
            }
            else if (Params is Dictionary)
            {
                Prompts.Add((Dictionary<string, object>)Params);
            }
            return Prompts;
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

    }

}
