using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ACS.SPiiPlusNET;
using Motion;
using GripperStepper;
using Tools;

namespace Rack
{

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>If estop button is on, then ethercat bus error occur, notify user, use reboot method</remarks>
    /// Power up, Ethercat error occur, wire problem? 
    public class CQCRack
    {
        private Api Ch = new Api();
        private EthercatMotion Motion;
        private Stepper Gripper;
        private string IP;

        private double YIsInBox = 200;
        private double YIsNearHome = 50;
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

            //Gripper = new Stepper("COM3");
            //Gripper.Setup();

            SetupComplete = true;
        }

        public void Test()
        {
            //Motion.ToPointX(700);
            Motion.SetSpeed(20);
            //Motion.ToPoint(Motion.MotorY, 291);
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

            double currentYPos = Motion.GetPosition(Motion.MotorY);
            double currentXPos = Motion.GetPositionX();
            double currentZPos = Motion.GetPosition(Motion.MotorZ);
            double currentRPos = Motion.GetPosition(Motion.MotorR);

            double XRPos = Convert.ToDouble(
                  XmlReaderWriter.GetTeachAttribute(Files.RackData, TeachPos.Pick, PosItem.XPos));

            if (Math.Abs(currentXPos - XRPos) < ConveyorWidth / 2) //Robot is in conveyor zone.
            {
                //Conveyor homing, may need to pull up a little.
            }
            else //Robot in box zone.
            {
                if (currentYPos > YIsInBox) //Y is dangerous
                {

                }
                else
                {
                    if (currentYPos < YIsNearHome)
                    {
                        //Z up, X move, Y move, R move gripper move, check phone left
                        Motion.ToPoint(Motion.MotorY, Motion.HomePosition.YPos);
                        Motion.ToPoint(Motion.MotorZ, Motion.HomePosition.ZPos);
                        Motion.ToPointX(Motion.HomePosition.XPos);
                        Motion.ToPoint(Motion.MotorR, Motion.HomePosition.RPos);

                        //Gripper
                        //Disable one of the motor.
                        Gripper.HomeMotor(GripperMotor.One, 0);
                        Gripper.HomeMotor(GripperMotor.Two, 0);
                    }
                    else
                    {
                        throw new Exception("Y motor is at unknown position, please home robot manually.");
                    }
                }
            }

            //Load data

        }
    }
}
