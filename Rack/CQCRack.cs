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
        private Api Ch;
        private EthercatMotion Motion;
        private string IP;

        private double YIsOutLenght = 50;

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

            //Box state should either be open or close.

            double YPos = Motion.GetPosition(Motion.MotorY);
            if (YPos > YIsOutLenght)
            {
                double Xpos = Motion.GetPositionX();

            }
            else
            {

            }

            //Load data

        }
    }
}
