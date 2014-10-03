using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Draftsocket;
using Draftsocket.AutoCAD;
using System.Linq;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(DraftSocketCommands.Commands))]
namespace DraftSocketCommands
{
    public class Commands
    {
        [CommandMethod("rpc")]
        public void rpccall()
        {
            Draftsocket.RPCTransport Transport = new Draftsocket.RPCTransport(5557);
            //var Session = new Draftsocket.AutoCAD.Session(Transport);
            var Message = Transport.new_message(
                    "id",
                    "GET",
                    "rpc",
                    "none"
                    );
            Transport.CommandLoop(Message);
        }

        public static T[] ConcatArrays<T>(params T[][] list)
        {
            var result = new T[list.Sum(a => a.Length)];
            int offset = 0;
            for (int x = 0; x < list.Length; x++)
            {
                list[x].CopyTo(result, offset);
                offset += list[x].Length;
            }
            return result;
        }

        [CommandMethod("registerevents")]
        public void rpccall2()
        {
            Application.DocumentManager.MdiActiveDocument.Database.ObjectAppended += new ObjectEventHandler(hndl);
        }

        public void hndl(object a, ObjectEventArgs b)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n DEBUG: event fired");
            DBObject object_in_question = b.DBObject;

            if (object_in_question.GetType() == typeof(Line))
            {
                ObjectId id = object_in_question.ObjectId;
                var p1 = new double[2];
                var p2 = new double[2];
                Transaction tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartOpenCloseTransaction();
                using (tr)
                {
                    Entity ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                    if (ent is Line)
                    {
                        var line = (Line)ent;
                        var retval1 = line.StartPoint.ToString();
                        var retval2 = line.EndPoint.ToString();
                        p1 = line.StartPoint.ToArray();
                        p2 = line.EndPoint.ToArray();
                    }
                    tr.Commit();
                }

                Draftsocket.RPCTransport Transport = new Draftsocket.RPCTransport(5557);
                //var Session = new Draftsocket.AutoCAD.Session(Transport);
                var msg = Transport.new_message(
                    "1001",
                    "GET",
                    "line_created",
                    p1[0], p1[1], p1[2], p2[0], p2[1], p2[2]
                    );

                Transport.CommandLoopOnce(msg);

            }


        }


    }


}