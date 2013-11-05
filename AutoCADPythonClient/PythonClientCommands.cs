using Autodesk.AutoCAD.Runtime;
using SocketWrapper;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ShadowbinderClient.PythonCommands))]
namespace ShadowbinderClient
{

    public class PythonCommands
    {

        [CommandMethod("Handshake")]
        public void Repent()
        {
            SocketWrapper.Transport Transport = new SocketWrapper.Transport(); 
            SocketWrapper.AutoCAD Session = new SocketWrapper.AutoCAD(Transport);
            SocketMessage Message = Protocol.NewCommand("Handshake");
            Transport.CommandLoop(Session, Message);
        }

        [CommandMethod("Inform")]
        public void Shadowbind()
        {
            SocketWrapper.Transport Transport = new SocketWrapper.Transport();
            SocketWrapper.AutoCAD Session = new SocketWrapper.AutoCAD(Transport);
            SocketMessage Message = Protocol.NewCommand("Inform");
            Transport.CommandLoop(Session, Message);
        }

        [CommandMethod("ServerSE")]
        public void ServerSE()
        {
            SocketWrapper.Transport Transport = new SocketWrapper.Transport();
            SocketWrapper.AutoCAD Session = new SocketWrapper.AutoCAD(Transport);
            SocketWrapper.SocketMessage Message = Protocol.NewCommand("ServerSE");
            Transport.CommandLoop(Session, Message);
        }


    }

}