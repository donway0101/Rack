using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ACS.SPiiPlusNET;
using Motion;

namespace Rack
{
    public class CQCRack
    {
        private Api Ch = new Api();
        private EthercatMotion Motion;
        private string IP;

        private double YIsInBox = 50;
        private double ConveyorWidth = 300;

        public bool RobotHomeComplete { get; set; }
        public bool SetupComplete { get; set; }

        public CQCRack(string controllerIp)
        {
            IP = controllerIp;
        }

        public void Start()
        {
            Ch.OpenCommEthernet(IP, 701);
            Motion = new EthercatMotion(Ch, 3);
            Motion.Setup();

            SetupComplete = true;
        }

        public void HomeRobot(double homeSpeed = 10)
        {
            if (SetupComplete==false)
            {
                throw new Exception("Setup not Complete.");
            }
            Motion.SetSpeed(homeSpeed);

            //Careful is robot is holding a phone.

            //Box state should either be open or close.

            double YPos = Motion.GetPosition(Motion.MotorY);
            if (YPos > YIsInBox) //Y is dangerous
            {
                double Xpos = Motion.GetPositionX();
                double XRPos = Convert.ToDouble( 
                    XmlReaderWriter.GetTeachAttribute("RackData.xml", TeachData.Pick, TeachData.XPos));

                if ( Math.Abs( Xpos- XRPos) < ConveyorWidth/2) //Robot is in conveyor zone.
                {
                    //Conveyor homing, may need to pull up a little.
                }
                else //Robot in box zone.
                {
                    if (true) // If not in the box, tell user to pull home robot manually.
                    {
                        //
                    }
                }

                
            }
            else
            {

            }

            //Load data

        }
    }
}
