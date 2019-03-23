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

            SetSpeed(10);

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
            //SetSpeed(20);
            //Motion.ToPoint(Motion.MotorY, 291);
            //Motion.ToPointWaitEnd(Motion.MotorY, 291,1000);
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
                    //Todo, need to check X?
                    //X Y Z tolerance 50mm. then is inside box

                    TargetPosition currentHolder = null;
                    double tolerance = 50;
                    foreach (var pos in Motion.Holders)
                    {
                        if (Math.Abs(currentXPos-pos.XPos) < tolerance & 
                            Math.Abs(currentYPos - pos.YPos) < tolerance & 
                            (currentZPos>pos.ZPos- tolerance & currentZPos<pos.ApproachHeight+ tolerance))
                        {
                            currentHolder = pos;
                        }
                    }

                    if (currentHolder!=null)
                    {
                        Motion.ToPointWaitEnd(Motion.MotorZ, currentHolder.ApproachHeight);
                        Motion.ToPointWaitEnd(Motion.MotorR, currentHolder.RPos);
                        Motion.ToPointWaitEnd(Motion.MotorY, Motion.HomePosition.YPos);
                        Motion.ToPointWaitEnd(Motion.MotorZ, Motion.HomePosition.ZPos);
                        Motion.ToPointXWaitEnd(Motion.HomePosition.XPos);
                        Motion.ToPoint(Motion.MotorR, Motion.HomePosition.RPos);
                    }
                    else
                    {
                        throw new Exception("Y motor is at unknown position, please home robot manually.");
                    }
                }
                else
                {
                    if (currentYPos < YIsNearHome)
                    {
                        Motion.ToPointWaitEnd(Motion.MotorY, Motion.HomePosition.YPos);
                        Motion.ToPointWaitEnd(Motion.MotorZ, Motion.HomePosition.ZPos);
                        Motion.ToPointX(Motion.HomePosition.XPos);
                        Motion.ToPoint(Motion.MotorR, Motion.HomePosition.RPos);

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
