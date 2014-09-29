using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Draftsocket;
using Draftsocket.AutoCAD;


// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(DraftSocketCommands.Commands))]
namespace DraftSocketCommands
{
    public class Commands
    {
        [CommandMethod("rpc")]
        public void rpccall()
        {
            Draftsocket.RPCTransport Transport = new Draftsocket.RPCTransport();
            var Session = new Draftsocket.AutoCAD.Session(Transport);
            var Message = Draftsocket.GeneralProtocol.NewCommand("RPC");
            Transport.CommandLoop(Session, Message);
        }
        [CommandMethod("registerevents")]
        public void rpccall2()
        {
            Application.DocumentManager.MdiActiveDocument.Database.ObjectAppended += new ObjectEventHandler(hndl);
        }

        public void hndl(object a, ObjectEventArgs b)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nMeh.");
        }


    }


}