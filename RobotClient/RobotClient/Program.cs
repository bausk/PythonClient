using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RobotOM;

namespace RobotClient
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Type objBLType = System.Type.GetTypeFromProgID("Robot.Application");
            var robot_app = new RobotApplication();
            robot_app = (RobotApplication) System.Activator.CreateInstance(objBLType);
            robot_app.Visible = 1;
            robot_app.Interactive = 1;

            //this is some dank shit right here
            Draftsocket.RPCTransport Transport = new Draftsocket.RPCTransport();
            Transport.CommandLoop(robot_app);
        }
    }
}
