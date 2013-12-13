using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace Draftsocket.AutoCAD
{
    public partial class Session : ISession
    {

        public List<Dictionary<string, object>> MakePayload(List<object> listobj)
        {
            var retval = new List<Dictionary<string, object>>();
            foreach (var obj in listobj)
            {
                var member = new Dictionary<string, object>();
                member.Add(Protocol.Keywords.DEFAULT, obj);
                foreach (var prop in obj.GetType().GetProperties())
                {
                    member.Add(prop.Name, prop.GetValue(obj, null));
                }
                retval.Add(member);
            }
            return retval;
        }

        public List<Dictionary<string, object>> MakePayload(List<PromptEntityResult> listobj)
        {
            var retval = new List<Dictionary<string, object>>();
            foreach (var obj in listobj)
            {
                var member = new Dictionary<string, object>();
                member.Add(Protocol.Keywords.DEFAULT, obj);
                member.Add(Protocol.Local.ObjectID, obj.ObjectId.ToString());
                member.Add(Protocol.Local.Handle, obj.ObjectId.Handle.Value);
                member.Add(Protocol.Local.TypeName, obj.GetType().Name);
                retval.Add(member);
            }
            return retval;
        }

        public List<Dictionary<string, object>> MakePayload(List<PromptResult> listobj)
        {
            var retval = new List<Dictionary<string, object>>();
            foreach (var obj in listobj)
            {
                var member = new Dictionary<string, object>();
                member.Add(Protocol.Keywords.DEFAULT, obj);
                member.Add(Protocol.Local.Status, obj.Status);
                member.Add(Protocol.Local.StringResult, obj.StringResult);
                retval.Add(member);
            }
            return retval;
        }

    }
}
