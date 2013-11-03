using Autodesk.AutoCAD.Runtime;
using SocketWrapper;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ShadowbinderClient.PythonCommands))]
namespace ShadowbinderClient
{

    public class PythonCommands
    {

        [CommandMethod("REPENT")]
        public void Repent()
        {
            SocketWrapper.AutoCAD Session = new SocketWrapper.AutoCAD();
            SocketMessage Message = Protocol.NewCommand("REPENT");
            Transport.CommandLoop(Session, Message);
        }

        [CommandMethod("SHADOWBIND")]
        public void Shadowbind()
        {
            SocketWrapper.AutoCAD Session = new SocketWrapper.AutoCAD();
            SocketMessage Message = Protocol.NewCommand("SHADOWBIND");
            Transport.CommandLoop(Session, Message);
        }
    }
}