using Autodesk.AutoCAD.Runtime;
using Draftsocket;
using Draftsocket.AutoCAD;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(DraftSocket.PythonCommands))]
namespace DraftSocket
{

    public class PythonCommands
    {


        [CommandMethod("Handshake")]
        public void Repent()
        {
            Draftsocket.Transport Transport = new Draftsocket.Transport();
            var Session = new Draftsocket.AutoCAD.Session(Transport);
            var Message = Draftsocket.GeneralProtocol.NewCommand("Handshake");
            Transport.CommandLoop(Session, Message);
        }

        [CommandMethod("Inform")]
        public void Inform()
        {
            Draftsocket.Transport Transport = new Draftsocket.Transport();
            var Session = new Draftsocket.AutoCAD.Session(Transport);
            ClientMessage Message = GeneralProtocol.NewCommand("Inform");
            Transport.CommandLoop(Session, Message);
        }

        [CommandMethod("InformAndReceive")]
        public void Inform2()
        {
            Draftsocket.Transport Transport = new Draftsocket.Transport();
            var Session = new Draftsocket.AutoCAD.Session(Transport);
            ClientMessage Message = GeneralProtocol.NewCommand("Inform2");
            Transport.CommandLoop(Session, Message);

        }

        [CommandMethod("ServerSE")]
        public void ServerSE()
        {
            Draftsocket.Transport Transport = new Draftsocket.Transport();
            var Session = new Draftsocket.AutoCAD.Session(Transport);
            ClientMessage Message = GeneralProtocol.NewCommand("ServerSE");
            Transport.CommandLoop(Session, Message);
        }

        [CommandMethod("TestTransaction")]
        public void TestTransaction()
        {
            Draftsocket.Transport Transport = new Draftsocket.Transport();
            var Session = new Draftsocket.AutoCAD.Session(Transport);
            ClientMessage Message = GeneralProtocol.NewCommand("TestTransaction");
            Transport.CommandLoop(Session, Message);
        }
    }

}