using System;
using System.Threading.Tasks;
using ACS.SPiiPlusNET;
using Motion;
using GripperStepper;
using EcatIo;
using Conveyor;

namespace Rack
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>If estop button is on, then ethercat bus error occur, notify user, use reboot method</remarks>
    /// Power up, Ethercat error occur, wire problem? 
    public partial class CqcRack
    {
        private readonly Api _ch = new Api();
        private const double YIsInBox = 200;
        private const double YIsNearHome = 10;
        private bool _gripperIsOnline = false;
        private readonly string _ip;
        private bool _testRun = false;

        public EthercatMotion Motion;
        public Stepper Gripper;
        public EthercatIo Io;
        public PickAndPlaceConveyor Conveyor;

        public bool RobotHomeComplete { get; set; }
        public bool SetupComplete { get; set; }

        public CqcRack(string controllerIp)
        {
            _ip = controllerIp;
        }

        public void Start()
        {
            if (_ch.IsConnected==false)
            {
                _ch.OpenCommEthernet(_ip, 701);              
            }
            Motion = new EthercatMotion(_ch, 5);
            Motion.Setup();
            Io = new EthercatIo(_ch, 72, 7, 4);
            Io.Setup();
            Conveyor= new PickAndPlaceConveyor(Io);

            if (_gripperIsOnline)
            {
                if (Gripper == null)
                {
                    Gripper = new Stepper("COM3");                   
                }
                Gripper.Setup();
            }

            SetSpeed(10);

            SetupComplete = true;
        }

        public void Stop()
        {
            Motion.DisableAll();
            //_motion.KillAll();
        }

        public void SetSpeed(double speed)
        {
            Motion.SetSpeed(speed);
            if (_gripperIsOnline)
            {
                int stepperSpeed = Convert.ToInt16(speed / 20.0);
                stepperSpeed++;
                if (stepperSpeed>30)
                {
                    stepperSpeed = 30;
                }
                Gripper.SetSpeed(GripperStepper.Gripper.One, stepperSpeed);
                Gripper.SetSpeed(GripperStepper.Gripper.Two, stepperSpeed); 
            }
        }

        public void SetSpeedImm(double speed)
        {
            Motion.SetSpeedImm(speed);
        }

        public void HomeRobot(double homeSpeed = 5)
        {
            if (SetupComplete == false)
            {
                throw new Exception("Setup not Complete.");
            }
            Motion.SetSpeed(homeSpeed);

            //Careful is robot is holding a phone.

            //Box state should either be open or close.

            TargetPosition currentPosition;
            currentPosition = GetRobotCurrentPose();

            if (currentPosition.XPos < Motion.ConveyorRightPosition.XPos &
                currentPosition.XPos > Motion.ConveyorLeftPosition.XPos) //Robot is in conveyor zone.
            {
                if (currentPosition.YPos > YIsInBox) //Y is dangerous
                {
                    TargetPosition currentHolder = new TargetPosition(){Id = Location.Home};
                    double tolerance = 50;
                    foreach (var pos in Motion.Locations)
                    {
                        if (Math.Abs(currentPosition.XPos - pos.XPos) < tolerance &
                            Math.Abs(currentPosition.YPos - pos.YPos) < tolerance &
                            (currentPosition.ZPos > pos.ZPos - tolerance & currentPosition.ZPos < pos.ApproachHeight + tolerance))
                        {
                            currentHolder = pos;
                        }
                    }

                    if (currentHolder.Id != Location.Home)
                    {
                        Motion.ToPointWaitTillEnd(Motion.MotorZ, currentHolder.ApproachHeight);
                        Motion.ToPointWaitTillEnd(Motion.MotorR, currentHolder.RPos);
                        Motion.ToPointWaitTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
                        Motion.ToPointWaitTillEnd(Motion.MotorZ, Motion.HomePosition.ZPos);
                        Motion.ToPointXWaitTillEnd(Motion.HomePosition.XPos);
                        Motion.ToPointWaitTillEnd(Motion.MotorR, Motion.HomePosition.RPos);
                        HomeGrippers();
                    }
                    else
                    {
                        throw new Exception("Gripper is in unknown conveyor area, please home Y and manually then retry.");
                    }
                }
                else
                {
                    if (currentPosition.YPos < YIsNearHome)
                    {

                        Motion.ToPointWaitTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
                        Motion.ToPointWaitTillEnd(Motion.MotorZ, Motion.HomePosition.ZPos);
                        Motion.ToPointXWaitTillEnd(Motion.HomePosition.XPos);
                        Motion.ToPointWaitTillEnd(Motion.MotorR, Motion.HomePosition.RPos);

                        //Disable one of the motor.
                        HomeGrippers();
                    }
                    else
                    {
                        throw new Exception("Gripper is in unknown position, please home Y and manually then retry.");
                    }
                }

                
            }
            else //Robot in box zone.
            {
                if (currentPosition.YPos > YIsInBox) //Y is dangerous
                {
                    //Todo, need to check X?
                    //X Y Z tolerance 50mm. then is inside box

                    TargetPosition currentHolder = new TargetPosition(){Id = Location.Home};
                    double tolerance = 50;
                    foreach (var pos in Motion.Locations)
                    {
                        if (Math.Abs(currentPosition.XPos - pos.XPos) < tolerance &
                            Math.Abs(currentPosition.YPos - pos.YPos) < tolerance &
                            (currentPosition.ZPos > pos.ZPos - tolerance & currentPosition.ZPos < pos.ApproachHeight + tolerance))
                        {
                            currentHolder = pos;
                            break;
                        }
                    }

                    if (currentHolder.Id != Location.Home)
                    {
                        Motion.ToPointWaitTillEnd(Motion.MotorZ, currentHolder.ApproachHeight);
                        Motion.ToPointWaitTillEnd(Motion.MotorR, currentHolder.RPos);
                        Motion.ToPointWaitTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
                        Motion.ToPointWaitTillEnd(Motion.MotorZ, Motion.HomePosition.ZPos);
                        Motion.ToPointXWaitTillEnd(Motion.HomePosition.XPos);
                        Motion.ToPointWaitTillEnd(Motion.MotorR, Motion.HomePosition.RPos);
                        HomeGrippers();
                    }
                    else
                    {
                        throw new Exception("Gripper is in unknown box, please home Y and manually then retry.");
                    }
                }
                else
                {
                    if (currentPosition.YPos < YIsNearHome)
                    {

                        Motion.ToPointWaitTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
                        Motion.ToPointWaitTillEnd(Motion.MotorZ, Motion.HomePosition.ZPos);
                        Motion.ToPointXWaitTillEnd(Motion.HomePosition.XPos);
                        Motion.ToPointWaitTillEnd(Motion.MotorR, Motion.HomePosition.RPos);

                        //Disable one of the motor.
                        HomeGrippers();
                    }
                    else
                    {
                        throw new Exception("Gripper is in unknown position, please home Y and manually then retry.");
                    }
                }
            }

            RobotHomeComplete = true;
        }

        private TargetPosition GetRobotCurrentPose()
        {
            TargetPosition currentPosition = new TargetPosition
            {
                XPos = Motion.GetPositionX(),
                YPos = Motion.GetPosition(Motion.MotorY),
                ZPos = Motion.GetPosition(Motion.MotorZ),
                RPos = Motion.GetPosition(Motion.MotorR)
            };
            return currentPosition;
        }

        private void HomeGrippers()
        {
            if (_gripperIsOnline)
            {
                Gripper.HomeMotor(GripperStepper.Gripper.One, -6);
                Gripper.HomeMotor(GripperStepper.Gripper.Two, -2);
            }
        }

    }
}
