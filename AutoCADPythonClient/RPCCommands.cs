using Autodesk.AutoCAD.Runtime;
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

    }

}