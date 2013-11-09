using Autodesk.AutoCAD.Runtime;
using SocketWrapper;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(TestCommands.TestCommands))]
namespace TestCommands
{

    public class TestCommands
    {

        [CommandMethod("SWAPENT")]
        public void Repent()
        {

        }
    }

}