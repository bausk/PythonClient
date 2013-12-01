using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Newtonsoft.Json;

namespace Draftsocket
{

    //public static class Utilities
    //{

    //    public struct AutoCADKeywords
    //    {
    //        //AutoCAD keywords
    //        public const string Prompt = "Prompt";
    //        public const string RejectString = "RejectString";
    //        public const string AllowedClass = "AllowedClass";


    //    }

    //    public static SocketMessage GetEntityKeywords(object Payload)
    //    {
    //        return new SocketMessage();
    //    }

    //    public static List<Dictionary<string,object>> ParametersToList(object Params)
    //    {
    //        List<Dictionary<string, object>> Prompts = new List<Dictionary<string, object>>();
    //        if (Params is Newtonsoft.Json.Linq.JArray)
    //        {
    //            Newtonsoft.Json.Linq.JArray ParamsArray = (Newtonsoft.Json.Linq.JArray)Params;
    //            List<Dictionary<string,object>> ParamsList = ParamsArray.ToObject<List<Dictionary<string,object>>>();
    //            return ParamsList;
    //        }
    //        else if (Params is Newtonsoft.Json.Linq.JObject)
    //        {
    //            Newtonsoft.Json.Linq.JArray ParamsArray = new Newtonsoft.Json.Linq.JArray((Newtonsoft.Json.Linq.JObject)Params);
    //            List<Dictionary<string, object>> ParamsList = ParamsArray.ToObject<List<Dictionary<string, object>>>();
    //            return ParamsList;
    //        }
    //        return Prompts; //empty list
    //    }

    //}
}
