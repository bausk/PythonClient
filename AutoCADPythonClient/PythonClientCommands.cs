using Autodesk.AutoCAD.Runtime;
using SocketWrapper;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ShadowbinderClient.PythonCommands))]
namespace ShadowbinderClient
{

    public class PythonCommands
    {

        [CommandMethod("Handshake")]
        public void Repent()
        {
            SocketWrapper.AutoCAD Session = new SocketWrapper.AutoCAD();
            SocketMessage Message = Protocol.NewCommand("Handshake");
            Transport.CommandLoop(Session, Message);
        }

        [CommandMethod("Inform")]
        public void Shadowbind()
        {
            SocketWrapper.AutoCAD Session = new SocketWrapper.AutoCAD();
            SocketMessage Message = Protocol.NewCommand("Inform");
            Transport.CommandLoop(Session, Message);
        }
    }
}