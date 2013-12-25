﻿using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace Draftsocket.AutoCAD
{
    public partial class Session : ISession
    {
        public class Protocol : GeneralProtocol
        {
            public partial class AutocadAction
            {
                public const string GET_KEYWORD = "GET_KEYWORD";
                public const string GET_ENTITY_ID = "GET_ENTITY";
                public const string MANIPULATE_DB = "TR_MANIPULATE_DB";
                public const string REQUEST_USER_INPUT = "REQUEST_INPUT";
                public const string GET_DB_OBJECTS = "TR_GET_OBJECTS";

                public class DBObject
                {
                    public static readonly string UpgradeOpen = "UPGRADEOPEN";
                    public static readonly string DowngradeOpen = "DOWNGRADEOPEN";
                    public static readonly string SwapIdWith = "SWAPIDWITH";
                }
            }

            public class Local
            {
                //AutoCAD keywords
                public static readonly string Prompt = "PROMPT";
                public static readonly string Default = "DEFAULT";
                public static readonly string RejectMessage = "REJECTMESSAGE";
                public static readonly string AllowedClass = "ALLOWEDCLASS";
                public static readonly string AllowNone = "ALLOWNONE";
                public static readonly string Keywords = "KEYWORDS";
                public static readonly string AllowArbitraryInput = "ALLOWARBINPUT";

                public static readonly string ForRead = "FORREAD";

                public static readonly string ObjectID = "KEY_OBJECTID";
                public static readonly string TypeName = "KEY_TYPENAME";
                public static readonly string Handle = "KEY_HANDLE";
                public static readonly string Status = "STATUS";
                public static readonly string StringResult = "STRINGRESULT";


            }
            public static Dictionary<string, Type> EntityTypes = new Dictionary<string, Type>()
            {
                {"LINE",typeof(Line)},
                {"CURVE",typeof(Curve)},
                {"CIRCLE",typeof(Circle)},
                {"HATCH",typeof(Hatch)},
            };
        }
    }
}
