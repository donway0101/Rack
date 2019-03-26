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
using System.Threading;

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
        private Stepper Steppers;
        private bool GripperIsOnline = false;
        private string IP;

        private const double YIsInBox = 200;
        private const double YIsNearHome = 10;

        //After all motors are referenced to their home position, 
        // move gripper to the edge of pillar on each side of conveyor
        private const double ConveyorCenterToRightEdge = 750;
        private const double ConveyorCenterToLeftEdge = 40;

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
                Steppers = new Stepper("COM3");
                Steppers.Setup();
            }

            //Motion.EnableAll();

            SetupComplete = true;
        }

        public void Stop()
        {
            Motion.DisableAll();
        }

        public void Test()
        {
            //Motion.ToPointX(700);
            //SetSpeed(20);
            //Motion.ToPoint(Motion.MotorX1, 100);
            //while (Motion.GetPosition(Motion.MotorX1)<80)
            //{
            //    Thread.Sleep(100);
            //}
            //Motion.Break(Motion.MotorX1);
            //Motion.ToPoint(Motion.MotorX1, 150);
            //Motion.ToPointWaitTillEnd(Motion.MotorY, 291,1000);

            MoveToTargetPosition(Gripper.One, Motion.Holder4);
        }

        public void SetSpeed(double speed)
        {
            Motion.SetSpeed(speed);
            if (GripperIsOnline==true)
            {
                Steppers.SetVelocity(Gripper.One, speed * 360 / 1000);
                Steppers.SetVelocity(Gripper.One, speed * 360 / 1000); 
            }
        }

        public void SetSpeedImm(double speed)
        {
            Motion.SetSpeedImm(speed);
            if (GripperIsOnline == true)
            {
                Steppers.SetVelocity(Gripper.One, speed * 360 / 1000);
                Steppers.SetVelocity(Gripper.One, speed * 360 / 1000);
            }
        }

        public void HomeRobot(double homeSpeed = 10)
        {
            if (SetupComplete == false)
            {
                throw new Exception("Setup not Complete.");
            }
            Motion.SetSpeed(homeSpeed);

            //Careful is robot is holding a phone.

            //Box state should either be open or close.

            TargetPosition CurrentPosition = new TargetPosition();
            GetRobotPose(CurrentPosition);

            if (Motion.PickPosition.XPos + ConveyorCenterToRightEdge > CurrentPosition.XPos &
                CurrentPosition.XPos > Motion.PickPosition.XPos - ConveyorCenterToLeftEdge) //Robot is in conveyor zone.
            {
                //Conveyor homing, may need to pull up a little.
            }
            else //Robot in box zone.
            {
                if (CurrentPosition.YPos > YIsInBox) //Y is dangerous
                {
                    //Todo, need to check X?
                    //X Y Z tolerance 50mm. then is inside box

                    TargetPosition currentHolder = null;
                    double tolerance = 50;
                    foreach (var pos in Motion.Holders)
                    {
                        if (Math.Abs(CurrentPosition.XPos - pos.XPos) < tolerance &
                            Math.Abs(CurrentPosition.YPos - pos.YPos) < tolerance &
                            (CurrentPosition.ZPos > pos.ZPos - tolerance & CurrentPosition.ZPos < pos.ApproachHeight + tolerance))
                        {
                            currentHolder = pos;
                        }
                    }

                    if (currentHolder != null)
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
                        throw new Exception("Y motor is at unknown position, please home robot manually.");
                    }
                }
                else
                {
                    if (CurrentPosition.YPos < YIsNearHome)
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
                        throw new Exception("Y motor is at unknown position, please home robot manually.");
                    }
                }
            }

            RobotHomeComplete = true;
        }

        private void GetRobotPose(TargetPosition CurrentPosition)
        {
            CurrentPosition.XPos = Motion.GetPositionX();
            CurrentPosition.YPos = Motion.GetPosition(Motion.MotorY);
            CurrentPosition.ZPos = Motion.GetPosition(Motion.MotorZ);
            CurrentPosition.RPos = Motion.GetPosition(Motion.MotorR);
        }

        private void HomeGrippers()
        {
            if (GripperIsOnline == true)
            {
                Steppers.HomeMotor(Gripper.One, 0);
                Steppers.HomeMotor(Gripper.Two, 0);
            }
        }

        public void Pick(Gripper gripper)
        {
            //If system is OK, gripper is free and opened, conveyor is ready
            //If the other gripper is holding a phone, then conveyor can not reload.
        }

        public void Place(Gripper gripper)
        {
            //After place, conveyor can reload.
        }

        public void Bin(Gripper gripper)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gripper"></param>
        /// <param name="holders">One of holders in Motion</param>
        public void Load(Gripper gripper, TargetPosition holder)
        {

        }

        private void MoveToTargetPosition(Gripper gripper, TargetPosition target, bool zMoveAfterXEnd = false)
        {

            TargetPosition CurrentPosition = new TargetPosition();
            GetRobotPose(CurrentPosition);

            if (CurrentPosition.YPos>YIsNearHome)
            {
                throw new Exception("Y is out after previous movement");
            } //Dangerous, may have to 

            if ( CurrentPosition.XPos > Motion.PickPosition.XPos + ConveyorCenterToRightEdge) //Right box.
            {
                #region Move from right to ...
                if (target.XPos > Motion.ConveyorRightPosition.XPos)//Right to right.
                {
                    //Todo , detail exception.
                    Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
                    Motion.ToPointX(target.XPos);

                    if (gripper== Gripper.One)
                    {
                        Motion.ToPoint(Motion.MotorR, target.RPos);
                    }
                    else
                    {
                        Motion.ToPoint(Motion.MotorR, -target.RPos);
                    }
                    
                    Motion.WaitTillEnd(Motion.MotorZ);
                    Motion.WaitTillEndX();
                    Motion.WaitTillEnd(Motion.MotorR);

                    Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                    //if (breakApproachToZPos)
                    //{
                    //    Motion.BreakToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                    //}
                    //else
                    //{
                    Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                    //}

                }
                else //Not right to right.
                {
                    if (Motion.PickPosition.XPos - ConveyorCenterToLeftEdge > CurrentPosition.XPos)//Right to left
                    {
                        if (CurrentPosition.ZPos < Motion.HomePosition.ApproachHeight)//At bottom.
                        {
                            if (target.ZPos > Motion.HomePosition.ApproachHeight)//Bottom to top.
                            {
                                //move up
                                //move x to edge
                                //till height enough, move x to target
                                //till x and z end 
                                //move y ....
                            }
                            else
                            {
                                if (CurrentPosition.ZPos < Motion.HomePosition.ApproachHeight)//Bottom to bottom.
                                {
                                    //move up to pick approach
                                    //move x to edge
                                    //till height enough, move x to target
                                    //till x cross, move z
                                    //till x and z end 
                                    //move y ....
                                }
                                else
                                {
                                    throw new Exception("Wrong target position");
                                }
                            }
                        }
                    }
                    else//Right to conveyor.
                    {

                    }
                } 
                #endregion
            }
            else //Current Not at right
            {
                if (CurrentPosition.XPos < Motion.ConveyorLeftPosition.XPos) //Current at Left.
                {
                    #region Move from left to ...
                    if (target.XPos > Motion.ConveyorRightPosition.XPos)//Left to right
                    {

                    }
                    else
                    {
                        if (target.XPos < Motion.ConveyorLeftPosition.XPos )//Left to left
                        {

                        }
                        else//Left to conveyor.
                        {
                            
                        }
                    }
                    #endregion
                }
                else//Current at Conveyor.
                {
                    #region Move from conveyor to ...
                    if (target.XPos > Motion.ConveyorRightPosition.XPos)//Conveyor to right.
                    {

                    }
                    else
                    {
                        if (target.XPos < Motion.ConveyorLeftPosition.XPos)//Conveyor to left
                        {

                        }
                        else//Conveyor to conveyor.
                        {

                        }
                    }
                    #endregion
                }
            }
        }



        public void Unload(Gripper gripper, TargetPosition holder)
        {

        }

        public void UnloadAndLoad(Gripper gripper, TargetPosition holder)
        {

        }
    }
}
