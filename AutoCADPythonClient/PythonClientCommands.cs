using Autodesk.AutoCAD.Runtime;
using Draftsocket;
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
            Draftsocket.Transport Transport = new Draftsocket.Transport(); 
            Draftsocket.AutoCAD Session = new Draftsocket.AutoCAD(Transport);
            ClientMessage Message = Protocol.NewCommand("Handshake");
            Transport.CommandLoop(Session, Message);
        }

        [CommandMethod("Inform")]
        public void Shadowbind()
        {
            Draftsocket.Transport Transport = new Draftsocket.Transport();
            Draftsocket.AutoCAD Session = new Draftsocket.AutoCAD(Transport);
            ClientMessage Message = Protocol.NewCommand("Inform");
            Transport.CommandLoop(Session, Message);
        }

        [CommandMethod("ServerSE")]
        public void ServerSE()
        {
            Draftsocket.Transport Transport = new Draftsocket.Transport();
            Draftsocket.AutoCAD Session = new Draftsocket.AutoCAD(Transport);
            ClientMessage Message = Protocol.NewCommand("ServerSE");
            Transport.CommandLoop(Session, Message);
        }


    }

}