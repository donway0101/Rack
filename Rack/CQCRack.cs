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
        private bool GripperIsOnline = false;
        private string IP;

        private const double YIsInBox = 200;
        private const double YIsNearHome = 50;

        public bool RobotHomeComplete { get; set; }
        public bool SetupComplete { get; set; }

        public CQCRack(string controllerIp)
        {
            IP = controllerIp;
        }

        public void Start()
        {
            GripperIsOnline = false;
            Ch.OpenCommEthernet(IP, 701);
            Motion = new EthercatMotion(Ch, 3);
            Motion.Setup();

            if (GripperIsOnline==true)
            {
                Gripper = new Stepper("COM3");
                Gripper.Setup();
            }

            SetupComplete = true;
        }

        public void Test()
        {
            //Motion.ToPointX(700);
            SetSpeed(20);
            //Motion.ToPoint(Motion.MotorY, 291);
        }

        public void SetSpeed(double speed)
        {
            Motion.SetSpeed(speed);
            if (GripperIsOnline==true)
            {
                Gripper.SetVelocity(GripperMotor.One, speed * 360 / 1000);
                Gripper.SetVelocity(GripperMotor.One, speed * 360 / 1000); 
            }
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

            if (Motion.PickPosition.XPos - 200 > currentXPos & currentXPos > Motion.PickPosition.XPos + 50) //Robot is in conveyor zone.
            {
                //Conveyor homing, may need to pull up a little.
            }
            else //Robot in box zone.
            {
                if (currentYPos > YIsInBox) //Y is dangerous
                {
                    if (Motion.PickPosition.XPos - 200 > currentXPos) //On on side.
                    {

                    }
                    else
                    {
                        if (currentXPos > Motion.PickPosition.XPos + 50)
                        {

                        }
                        else
                        {
                            throw new Exception("Y motor is at unknown position, please home robot manually.");
                        }
                    }

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
