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
            Motion = new EthercatMotion(Ch, 5);
            Motion.Setup();

            SetSpeed(10);

            if (GripperIsOnline==true)
            {
                Steppers = new Stepper("COM3");
                Steppers.Setup();
            }

            Motion.EnableAll();

            SetupComplete = true;
        }

        public void Stop()
        {
            Motion.DisableAll();
        }

        public void Test()
        {
            //PickAndLoad();
            LoadGold();
        }

        private void LoadGold()
        {
            Task.Run(() =>
            {
                //DemoMoveToTarget(Motion.Gold1);
                //DemoMoveToTarget(Motion.Holder1);
                //DemoMoveToTarget(Motion.Gold2);
                //DemoMoveToTarget(Motion.Holder2);
                //DemoMoveToTarget(Motion.Gold3);
                //DemoMoveToTarget(Motion.Holder3);
                //DemoMoveToTarget(Motion.Gold4);
                //DemoMoveToTarget(Motion.Holder4);
                //DemoMoveToTarget(Motion.Gold5);
                //DemoMoveToTarget(Motion.Holder5);
                DemoMoveToTarget(Motion.Gold1);
                DemoMoveToTarget(Motion.Holder6);
            });
        }

        private void PickAndLoad()
        {
            Task.Run(() =>
            {
                SetSpeed(20);
                DemoMoveToTarget(Motion.Holder1);
                DemoMoveToTarget(Motion.PickPosition);
                DemoMoveToTarget(Motion.Holder2);
                DemoMoveToTarget(Motion.PickPosition);
                DemoMoveToTarget(Motion.Holder3);
                DemoMoveToTarget(Motion.PickPosition);
                DemoMoveToTarget(Motion.Holder4);
                DemoMoveToTarget(Motion.PickPosition);
                DemoMoveToTarget(Motion.Holder5);
                DemoMoveToTarget(Motion.PickPosition);
                DemoMoveToTarget(Motion.Holder6);
                DemoMoveToTarget(Motion.PickPosition);
            });
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

        public void HomeRobot(double homeSpeed = 20)
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

            if (CurrentPosition.XPos < Motion.ConveyorRightPosition.XPos &
                CurrentPosition.XPos > Motion.ConveyorLeftPosition.XPos) //Robot is in conveyor zone.
            {
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

        private void DemoMoveToTarget(TargetPosition target)
        {
            MoveToTargetPosition(Gripper.One, target);
            Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ApproachHeight);
            Motion.ToPointWaitTillEnd(Motion.MotorY, Motion.PickPosition.YPos);
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

            //if (RobotHomeComplete==false)
            //{
            //    throw new Exception("Robot is not home complete");
            //}

            if (SetupComplete == false)
            {
                throw new Exception("Robot is not SetupComplete");
            }

            if (CurrentPosition.YPos>YIsNearHome)
            {
                throw new Exception("Y is out after previous movement");
            } //Dangerous, may have to 

            if ( CurrentPosition.XPos > Motion.ConveyorRightPosition.XPos) //Now on right side.
            {
                #region Move from right to ...
                if (target.XPos > Motion.ConveyorRightPosition.XPos)//Now on right side.Move from Right to right. 
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
                                      
                    Motion.WaitTillEndX();
                    Motion.WaitTillEnd(Motion.MotorR);
                    if (zMoveAfterXEnd == true & target.Id>7) //Holder id <= 6, only for gold phone.
                    {
                        Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                        Motion.BreakToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                    }
                    else //For shield boxes.
                    {
                        Motion.WaitTillEnd(Motion.MotorZ);
                        Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                        Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                    }
                }
                else 
                {
                    if (target.XPos < Motion.ConveyorLeftPosition.XPos)//Now on right side. Move from Right to left
                    {
                        if (target.ZPos > Motion.PickPosition.ApproachHeight)
                        //Now on right side. Move from Right to left
                        //Bottom to top.
                        {
                            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
                            Motion.ToPointX(Motion.ConveyorRightPosition.XPos);
                            if (CurrentPosition.ZPos < Motion.PickPosition.ApproachHeight)
                            {                               
                                Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 10);
                            }

                            Motion.BreakToPointX(target.XPos);

                            if (gripper == Gripper.One)
                            {
                                Motion.ToPoint(Motion.MotorR, target.RPos);
                            }
                            else
                            {
                                Motion.ToPoint(Motion.MotorR, -target.RPos);
                            }

                            Motion.WaitTillEndX();
                            Motion.WaitTillEnd(Motion.MotorR);
                            Motion.WaitTillEnd(Motion.MotorZ);
                            Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                            Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                        }
                        else
                        {
                            if (target.ZPos < Motion.PickPosition.ApproachHeight)
                            //Now on right side. Move from Right to left
                            //Bottom to bottom.
                            {
                                Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
                                Motion.ToPointX(Motion.ConveyorRightPosition.XPos);
                                Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 10);
                                Motion.BreakToPointX(target.XPos);

                                Motion.WaitTillXSmallerThan(Motion.ConveyorLeftPosition.XPos);

                                Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);

                                Motion.WaitTillEndX();
                                Motion.WaitTillEnd(Motion.MotorR);

                                Motion.WaitTillEnd(Motion.MotorZ);
                                Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                                Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                            }
                            else
                            {
                                throw new Exception("MoveToTargetPosition failed! Wrong target position");
                            }
                        }

                    }
                    else
                    //Now on right side. Move from Right to conveyor.
                    {
                        Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
                        if (CurrentPosition.ZPos < Motion.PickPosition.ApproachHeight)
                        {                          
                            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);
                        }

                        Motion.ToPointX(target.XPos);

                        if (gripper == Gripper.One)
                        {
                            Motion.ToPoint(Motion.MotorR, target.RPos);
                        }
                        else
                        {
                            Motion.ToPoint(Motion.MotorR, -target.RPos);
                        }

                        Motion.WaitTillEndX();
                        Motion.WaitTillEnd(Motion.MotorR);
                        Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                        Motion.BreakToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                    }
                } 
                #endregion
            }
            else
            {             
                if (CurrentPosition.XPos < Motion.ConveyorLeftPosition.XPos) //Now on left side.  
                {
                    #region Move from left to ...
                    if (target.XPos > Motion.ConveyorRightPosition.XPos)//Now on left side. Move from Left to right
                    {
                        if (target.ZPos < Motion.PickPosition.ApproachHeight)// To bottom.
                        {
                            Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
                            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 10);
                            Motion.ToPointX(target.XPos);

                            if (gripper == Gripper.One)
                            {
                                Motion.ToPoint(Motion.MotorR, target.RPos);
                            }
                            else
                            {
                                Motion.ToPoint(Motion.MotorR, -target.RPos);
                            }

                            Motion.WaitTillXBiggerThan(Motion.ConveyorRightPosition.XPos);
                            Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);


                            Motion.WaitTillEndX();
                            Motion.WaitTillEnd(Motion.MotorR);
                            if (zMoveAfterXEnd == true & target.Id > 7) //Holder id <= 6, only for gold phone.
                            {
                                Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                                Motion.BreakToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                            }
                            else //For shield boxes.
                            {
                                Motion.WaitTillEnd(Motion.MotorZ);
                                Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                                Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                            }
                        }
                        else //To top.
                        {
                            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
                            if (CurrentPosition.ZPos < Motion.PickPosition.ApproachHeight)
                            {
                                Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 10);
                            }
                            
                            Motion.ToPointX(target.XPos);
                          
                            if (gripper == Gripper.One)
                            {
                                Motion.ToPoint(Motion.MotorR, target.RPos);
                            }
                            else
                            {
                                Motion.ToPoint(Motion.MotorR, -target.RPos);
                            }

                            Motion.WaitTillEndX();
                            Motion.WaitTillEnd(Motion.MotorR);
                            Motion.WaitTillEnd(Motion.MotorZ);
                            Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                            Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                        }
                    }
                    else
                    {
                        if (target.XPos < Motion.ConveyorLeftPosition.XPos)//Now on left side. Move from Left to left
                        {
                            Motion.ToPointX(target.XPos);
                            if (gripper == Gripper.One)
                            {
                                Motion.ToPoint(Motion.MotorR, target.RPos);
                            }
                            else
                            {
                                Motion.ToPoint(Motion.MotorR, -target.RPos);
                            }
                            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
                            Motion.WaitTillEndX();
                            Motion.WaitTillEnd(Motion.MotorR);
                            Motion.WaitTillEnd(Motion.MotorZ);
                            Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                            Motion.BreakToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                        }
                        else//Now on left side. Move from Left to conveyor.
                        {
                            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
                            if (CurrentPosition.ZPos < Motion.PickPosition.ApproachHeight)
                            {
                                Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 10);
                            }

                            Motion.ToPointX(target.XPos);

                            if (gripper == Gripper.One)
                            {
                                Motion.ToPoint(Motion.MotorR, target.RPos);
                            }
                            else
                            {
                                Motion.ToPoint(Motion.MotorR, -target.RPos);
                            }

                            Motion.WaitTillEndX();
                            Motion.WaitTillEnd(Motion.MotorR);
                            Motion.WaitTillEnd(Motion.MotorZ);
                            Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                            Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                        }
                    }
                    #endregion
                }
                else//Now on conveyor side.
                {
                    #region Move from conveyor to ...
                    if (target.XPos > Motion.ConveyorRightPosition.XPos)
                    //Now on conveyor side. Move from Conveyor to right.
                    {
                        if (target.ZPos < Motion.PickPosition.ApproachHeight)// To bottom.
                        {
                            Motion.ToPointX(target.XPos);
                            Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);

                            if (gripper == Gripper.One)
                            {
                                Motion.ToPoint(Motion.MotorR, target.RPos);
                            }
                            else
                            {
                                Motion.ToPoint(Motion.MotorR, -target.RPos);
                            }

                            Motion.WaitTillXBiggerThan(Motion.ConveyorRightPosition.XPos);
                            Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);


                            Motion.WaitTillEndX();
                            Motion.WaitTillEnd(Motion.MotorR);
                            if (zMoveAfterXEnd == true & target.Id > 7) //Holder id <= 6, only for gold phone.
                            {
                                Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                                Motion.BreakToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                            }
                            else //For shield boxes.
                            {
                                Motion.WaitTillEnd(Motion.MotorZ);
                                Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                                Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                            }
                        }
                        else //To top.
                        {
                            Motion.ToPointX(target.XPos);
                            if (gripper == Gripper.One)
                            {
                                Motion.ToPoint(Motion.MotorR, target.RPos);
                            }
                            else
                            {
                                Motion.ToPoint(Motion.MotorR, -target.RPos);
                            }
                            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
                            Motion.WaitTillEndX();
                            Motion.WaitTillEnd(Motion.MotorR);
                            Motion.WaitTillEnd(Motion.MotorZ);
                            Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                            Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                        }
                    }
                    else
                    {
                        //Now on conveyor side. Move from Conveyor to left
                        if (target.XPos < Motion.ConveyorLeftPosition.XPos)
                        {
                            if (target.ZPos < Motion.PickPosition.ApproachHeight)// To bottom.
                            {
                                Motion.ToPointX(target.XPos);
                                Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);

                                if (gripper == Gripper.One)
                                {
                                    Motion.ToPoint(Motion.MotorR, target.RPos);
                                }
                                else
                                {
                                    Motion.ToPoint(Motion.MotorR, -target.RPos);
                                }

                                Motion.WaitTillXSmallerThan(Motion.ConveyorLeftPosition.XPos);
                                Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);

                                Motion.WaitTillEndX();
                                Motion.WaitTillEnd(Motion.MotorR);

                                Motion.WaitTillEnd(Motion.MotorZ);
                                Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                                Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                            }
                            else //To top.
                            {
                                Motion.ToPointX(target.XPos);
                                if (gripper == Gripper.One)
                                {
                                    Motion.ToPoint(Motion.MotorR, target.RPos);
                                }
                                else
                                {
                                    Motion.ToPoint(Motion.MotorR, -target.RPos);
                                }
                                Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
                                Motion.WaitTillEndX();
                                Motion.WaitTillEnd(Motion.MotorR);
                                Motion.WaitTillEnd(Motion.MotorZ);
                                Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                                Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
                            }
                        }
                        else//Now on conveyor side. Move from Conveyor to conveyor.
                        {
                            Motion.ToPointX(target.XPos);
                            if (gripper == Gripper.One)
                            {
                                Motion.ToPoint(Motion.MotorR, target.RPos);
                            }
                            else
                            {
                                Motion.ToPoint(Motion.MotorR, -target.RPos);
                            }
                            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
                            Motion.WaitTillEndX();
                            Motion.WaitTillEnd(Motion.MotorR);
                            Motion.WaitTillEnd(Motion.MotorZ);
                            Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
                            Motion.BreakToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
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
