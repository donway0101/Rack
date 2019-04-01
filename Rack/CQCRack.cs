using System;
using System.Threading.Tasks;
using ACS.SPiiPlusNET;
using Motion;
using GripperStepper;

namespace Rack
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>If estop button is on, then ethercat bus error occur, notify user, use reboot method</remarks>
    /// Power up, Ethercat error occur, wire problem? 
    public class CqcRack
    {
        private readonly Api _ch = new Api();
        private EthercatMotion _motion;
        private Stepper _steppers;
        private bool _gripperIsOnline = true;
        private readonly string _ip;

        private const double YIsInBox = 200;
        private const double YIsNearHome = 10;

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
            _motion = new EthercatMotion(_ch, 5);
            _motion.Setup();
            _motion.EnableAll();
     
            if (_gripperIsOnline)
            {
                _steppers = new Stepper("COM3");
                _steppers.Setup();
            }

            SetSpeed(10);

            SetupComplete = true;
        }

        public void Stop()
        {
            _motion.DisableAll();
        }

        public void Test()
        {
            //PickAndLoad();
            //LoadGold();
            //UnloadAndBin();
            //TestRun();
            Exchange(_motion.PickPosition, Gripper.One);
            //Exchange(_motion.Holder1, Gripper.Two);
        }

        private void LoadGold()
        {
            Task.Run(() =>
            {
                DemoMoveToTarget(_motion.Gold1);
                DemoMoveToTarget(_motion.Holder1);
                DemoMoveToTarget(_motion.Gold2);
                DemoMoveToTarget(_motion.Holder2);
                DemoMoveToTarget(_motion.Gold3);
                DemoMoveToTarget(_motion.Holder3);
                DemoMoveToTarget(_motion.Gold4);
                DemoMoveToTarget(_motion.Holder4);
                DemoMoveToTarget(_motion.Gold5);
                DemoMoveToTarget(_motion.Holder5);
                DemoMoveToTarget(_motion.Gold1);
                DemoMoveToTarget(_motion.Holder6);
                DemoMoveToTarget(_motion.HomePosition);
            });
        }

        private void UnloadAndBin()
        {
            Task.Run(() =>
            {
                SetSpeed(20);
                DemoMoveToTarget(_motion.Holder1);
                DemoMoveToTarget(_motion.BinPosition);
                DemoMoveToTarget(_motion.Holder2);
                DemoMoveToTarget(_motion.BinPosition);
                DemoMoveToTarget(_motion.Holder3);
                DemoMoveToTarget(_motion.BinPosition);
                DemoMoveToTarget(_motion.Holder4);
                DemoMoveToTarget(_motion.BinPosition);
                DemoMoveToTarget(_motion.Holder5);
                DemoMoveToTarget(_motion.BinPosition);
                DemoMoveToTarget(_motion.Holder6);
                DemoMoveToTarget(_motion.BinPosition);
                DemoMoveToTarget(_motion.HomePosition);
            });
        }

        private void PickAndLoad()
        {
            Task.Run(() =>
            {
                SetSpeed(20);
                DemoMoveToTarget(_motion.Holder1);
                DemoMoveToTarget(_motion.PickPosition);
                DemoMoveToTarget(_motion.Holder2);
                DemoMoveToTarget(_motion.PickPosition);
                DemoMoveToTarget(_motion.Holder3);
                DemoMoveToTarget(_motion.PickPosition);
                DemoMoveToTarget(_motion.Holder4);
                DemoMoveToTarget(_motion.PickPosition);
                DemoMoveToTarget(_motion.Holder5);
                DemoMoveToTarget(_motion.PickPosition);
                DemoMoveToTarget(_motion.Holder6);
                DemoMoveToTarget(_motion.PickPosition);
                DemoMoveToTarget(_motion.HomePosition);
            });
        }

        private void TestRun()
        {
            //Task.Run(() =>
            //{
                //SetSpeed(20);
                Exchange(_motion.PickPosition, Gripper.One);
                Exchange(_motion.Holder1, Gripper.Two);
                Exchange(_motion.PickPosition, Gripper.One);
                Exchange(_motion.Holder2, Gripper.Two);
                Exchange(_motion.PickPosition, Gripper.One);
                Exchange(_motion.Holder3, Gripper.Two);
                Exchange(_motion.PickPosition, Gripper.One);
                Exchange(_motion.Holder4, Gripper.Two);
                Exchange(_motion.PickPosition, Gripper.One);
                Exchange(_motion.Holder5, Gripper.Two);
                Exchange(_motion.PickPosition, Gripper.One);
                Exchange(_motion.Holder6, Gripper.Two);
            //});
        }

        public void SetSpeed(double speed)
        {
            _motion.SetSpeed(speed);
            if (_gripperIsOnline)
            {
                int stepperSpeed = Convert.ToInt16(speed / 20.0);
                stepperSpeed++;
                if (stepperSpeed>30)
                {
                    stepperSpeed = 30;
                }
                _steppers.SetSpeed(Gripper.One, stepperSpeed);
                _steppers.SetSpeed(Gripper.Two, stepperSpeed); 
            }
        }

        public void SetSpeedImm(double speed)
        {
            _motion.SetSpeedImm(speed);
        }

        public void HomeRobot(double homeSpeed = 20)
        {
            if (SetupComplete == false)
            {
                throw new Exception("Setup not Complete.");
            }
            _motion.SetSpeed(homeSpeed);

            //Careful is robot is holding a phone.

            //Box state should either be open or close.

            TargetPosition currentPosition = new TargetPosition();
            GetRobotPose(currentPosition);

            if (currentPosition.XPos < _motion.ConveyorRightPosition.XPos &
                currentPosition.XPos > _motion.ConveyorLeftPosition.XPos) //Robot is in conveyor zone.
            {
                if (currentPosition.YPos > YIsInBox) //Y is dangerous
                {
                    TargetPosition currentHolder = null;
                    double tolerance = 50;
                    foreach (var pos in _motion.Locations)
                    {
                        if (Math.Abs(currentPosition.XPos - pos.XPos) < tolerance &
                            Math.Abs(currentPosition.YPos - pos.YPos) < tolerance &
                            (currentPosition.ZPos > pos.ZPos - tolerance & currentPosition.ZPos < pos.ApproachHeight + tolerance))
                        {
                            currentHolder = pos;
                        }
                    }

                    if (currentHolder != null)
                    {
                        _motion.ToPointWaitTillEnd(_motion.MotorZ, currentHolder.ApproachHeight);
                        _motion.ToPointWaitTillEnd(_motion.MotorR, currentHolder.RPos);
                        _motion.ToPointWaitTillEnd(_motion.MotorY, _motion.HomePosition.YPos);
                        _motion.ToPointWaitTillEnd(_motion.MotorZ, _motion.HomePosition.ZPos);
                        _motion.ToPointXWaitTillEnd(_motion.HomePosition.XPos);
                        _motion.ToPointWaitTillEnd(_motion.MotorR, _motion.HomePosition.RPos);
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

                        _motion.ToPointWaitTillEnd(_motion.MotorY, _motion.HomePosition.YPos);
                        _motion.ToPointWaitTillEnd(_motion.MotorZ, _motion.HomePosition.ZPos);
                        _motion.ToPointXWaitTillEnd(_motion.HomePosition.XPos);
                        _motion.ToPointWaitTillEnd(_motion.MotorR, _motion.HomePosition.RPos);

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

                    TargetPosition currentHolder = null;
                    double tolerance = 50;
                    foreach (var pos in _motion.Locations)
                    {
                        if (Math.Abs(currentPosition.XPos - pos.XPos) < tolerance &
                            Math.Abs(currentPosition.YPos - pos.YPos) < tolerance &
                            (currentPosition.ZPos > pos.ZPos - tolerance & currentPosition.ZPos < pos.ApproachHeight + tolerance))
                        {
                            currentHolder = pos;
                        }
                    }

                    if (currentHolder != null)
                    {
                        _motion.ToPointWaitTillEnd(_motion.MotorZ, currentHolder.ApproachHeight);
                        _motion.ToPointWaitTillEnd(_motion.MotorR, currentHolder.RPos);
                        _motion.ToPointWaitTillEnd(_motion.MotorY, _motion.HomePosition.YPos);
                        _motion.ToPointWaitTillEnd(_motion.MotorZ, _motion.HomePosition.ZPos);
                        _motion.ToPointXWaitTillEnd(_motion.HomePosition.XPos);
                        _motion.ToPointWaitTillEnd(_motion.MotorR, _motion.HomePosition.RPos);
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

                        _motion.ToPointWaitTillEnd(_motion.MotorY, _motion.HomePosition.YPos);
                        _motion.ToPointWaitTillEnd(_motion.MotorZ, _motion.HomePosition.ZPos);
                        _motion.ToPointXWaitTillEnd(_motion.HomePosition.XPos);
                        _motion.ToPointWaitTillEnd(_motion.MotorR, _motion.HomePosition.RPos);

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

        private void GetRobotPose(TargetPosition currentPosition)
        {
            currentPosition.XPos = _motion.GetPositionX();
            currentPosition.YPos = _motion.GetPosition(_motion.MotorY);
            currentPosition.ZPos = _motion.GetPosition(_motion.MotorZ);
            currentPosition.RPos = _motion.GetPosition(_motion.MotorR);
        }

        private void HomeGrippers()
        {
            if (_gripperIsOnline)
            {
                _steppers.HomeMotor(Gripper.One, -6);
                _steppers.HomeMotor(Gripper.Two, -2);
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
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ApproachHeight);
            _motion.ToPointWaitTillEnd(_motion.MotorY, _motion.PickPosition.YPos);
        }

        private void Exchange(TargetPosition target, Gripper gripper)
        {
            MoveToTargetPosition(gripper, target);

            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ApproachHeight);
            SwitchGripper(target, gripper);

            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ApproachHeight);
            _motion.ToPointWaitTillEnd(_motion.MotorY, _motion.PickPosition.YPos);
        }

        public void SwitchGripper(TargetPosition target, Gripper gripper)
        {
            gripper = gripper== Gripper.One ? Gripper.Two : Gripper.One;
            MoveGripperTillEnd(target, gripper);
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
        /// <param name="holder">One of holders in Motion</param>
        public void Load(Gripper gripper, TargetPosition holder)
        {

        }


        private void MoveToTargetPosition(Gripper gripper, TargetPosition target)
        {

            TargetPosition currentPosition = new TargetPosition();
            GetRobotPose(currentPosition);

            //if (RobotHomeComplete==false)
            //{
            //    throw new Exception("Robot is not home complete");
            //}

            //Todo
            //if (target.Id==Location.Bin && door is opened, then that's not cool )
            {

            }

            if (SetupComplete == false)
            {
                throw new Exception("Robot is not SetupComplete");
            }

            if (currentPosition.YPos>YIsNearHome)
            {
                throw new Exception("Y is out after previous movement");
            } //Dangerous, may have to 
           
            if ( currentPosition.XPos > _motion.ConveyorRightPosition.XPos) //Now on right side.
            {
                #region Move from right to ...
                if (target.XPos > _motion.ConveyorRightPosition.XPos) //To right. 
                {
                    MoveFromRightToRight(gripper, target);
                }
                else 
                {
                    if (target.XPos < _motion.ConveyorLeftPosition.XPos) //To left.
                    {
                        if (target.ZPos > _motion.PickPosition.ApproachHeight) //To left top.
                        {
                            MoveFromRightToLeftTop(gripper, target, currentPosition);
                        }
                        else //To left bottom.
                        {
                            MoveFromRightToLeftBottom(target);
                        }
                    }
                    else //To Conveyor.
                    {
                        MoveFromRightToConveyor(gripper, target, currentPosition);
                    }
                } 
                #endregion
            }
            else
            {              
                if (currentPosition.XPos < _motion.ConveyorLeftPosition.XPos) //Now on left side. 
                {
                    #region Move from left to ...
                    if (target.XPos > _motion.ConveyorRightPosition.XPos)//To right
                    {
                        if (target.ZPos < _motion.PickPosition.ApproachHeight) // To right bottom.
                        {
                            MoveFromLeftToRightBottom(gripper, target);
                        }
                        else //To right top.
                        {
                            MoveFromLeftToRightTop(gripper, target, currentPosition);
                        }
                    }
                    else
                    {
                        if (target.XPos < _motion.ConveyorLeftPosition.XPos)//To left.
                        {
                            MoveFromLeftToLeft(gripper, target);
                        }
                        else//To conveyor.
                        {
                            MoveFromLeftToConveyor(gripper, target, currentPosition);
                        }
                    }
                    #endregion
                }
                else //Now on conveyor side.
                {
                    #region Move from conveyor to ...
                    if (target.XPos > _motion.ConveyorRightPosition.XPos) //To right.                
                    {
                        if (target.ZPos < _motion.PickPosition.ApproachHeight) // To right bottom.
                        {
                            MoveFromConveyorToRightBottom(gripper, target);
                        }
                        else //To right top.
                        {
                            MoveFromConveyorToRightTop(gripper, target);
                        }
                    }
                    else
                    {
                        if (target.XPos < _motion.ConveyorLeftPosition.XPos) //To left.
                        {
                            if (target.ZPos < _motion.PickPosition.ApproachHeight)// To left bottom.
                            {
                                MoveFromConveyorToLeftBottom(gripper, target);
                            }
                            else //To left top.
                            {
                                MoveFromConveyorToLeftTop(gripper, target);
                            }
                        }
                        else //To conveyor.
                        {
                            MoveFromConveyorToConveyor(gripper, target);
                        }
                    }
                    #endregion
                }
            }
        }

        private void MoveFromRightToConveyor(Gripper gripper, TargetPosition target, TargetPosition currentPosition)
        {
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            if (currentPosition.ZPos < _motion.PickPosition.ApproachHeight)
            {
                _motion.WaitTillZBiggerThan(_motion.PickPosition.ApproachHeight - 20);
            }

            _motion.ToPointX(target.XPos);

            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
            }

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorR);
            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _motion.BreakToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromRightToLeftBottom(TargetPosition target)
        {
            _motion.ToPoint(_motion.MotorZ, _motion.PickPosition.ApproachHeight);
            _motion.ToPointX(_motion.ConveyorRightPosition.XPos);
            _motion.WaitTillZBiggerThan(_motion.PickPosition.ApproachHeight - 10);
            _motion.BreakToPointX(target.XPos);

            _motion.WaitTillXSmallerThan(_motion.ConveyorLeftPosition.XPos);

            _motion.BreakToPoint(_motion.MotorZ, target.ApproachHeight);

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorR);

            _motion.WaitTillEnd(_motion.MotorZ);
            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromRightToLeftTop(Gripper gripper, TargetPosition target, TargetPosition currentPosition)
        {
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            _motion.ToPointX(_motion.ConveyorRightPosition.XPos);
            if (currentPosition.ZPos < _motion.PickPosition.ApproachHeight)
            {
                _motion.WaitTillZBiggerThan(_motion.PickPosition.ApproachHeight - 10);
            }

            _motion.BreakToPointX(target.XPos);

            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
            }

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorR);
            _motion.WaitTillEnd(_motion.MotorZ);
            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromRightToRight(Gripper gripper, TargetPosition target)
        {
            //Todo , detail exception.
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            _motion.ToPointX(target.XPos);

            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
                _steppers.ToPoint(Gripper.One, target.APos);
                _steppers.ToPoint(Gripper.Two, 0);
                _steppers.WaitTillEnd(Gripper.One, target.APos);
                _steppers.WaitTillEnd(Gripper.Two);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
                _steppers.ToPoint(Gripper.Two, target.APos);
                _steppers.ToPoint(Gripper.One, 0);
                _steppers.WaitTillEnd(Gripper.One);
                _steppers.WaitTillEnd(Gripper.Two, target.APos);
            }

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorR);
            _motion.WaitTillEnd(_motion.MotorZ);

            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromLeftToRightTop(Gripper gripper, TargetPosition target, TargetPosition currentPosition)
        {
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            if (currentPosition.ZPos < _motion.PickPosition.ApproachHeight)
            {
                _motion.WaitTillZBiggerThan(_motion.PickPosition.ApproachHeight - 10);
            }

            _motion.ToPointX(target.XPos);

            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
            }

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorR);
            _motion.WaitTillEnd(_motion.MotorZ);
            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromLeftToRightBottom(Gripper gripper, TargetPosition target)
        {
            _motion.ToPoint(_motion.MotorZ, _motion.PickPosition.ApproachHeight);
            _motion.WaitTillZBiggerThan(_motion.PickPosition.ApproachHeight - 10);
            _motion.ToPointX(target.XPos);

            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
            }

            _motion.WaitTillXBiggerThan(_motion.ConveyorRightPosition.XPos);
            _motion.BreakToPoint(_motion.MotorZ, target.ApproachHeight);


            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorR);

            _motion.WaitTillEnd(_motion.MotorZ);
            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromLeftToLeft(Gripper gripper, TargetPosition target)
        {
            _motion.ToPointX(target.XPos);
            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
            }
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorR);
            _motion.WaitTillEnd(_motion.MotorZ);
            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _motion.BreakToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromLeftToConveyor(Gripper gripper, TargetPosition target, TargetPosition currentPosition)
        {
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            if (currentPosition.ZPos < _motion.PickPosition.ApproachHeight)
            {
                _motion.WaitTillZBiggerThan(_motion.PickPosition.ApproachHeight - 10);
            }

            _motion.ToPointX(target.XPos);

            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
            }

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorR);
            _motion.WaitTillEnd(_motion.MotorZ);
            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromConveyorToRightTop(Gripper gripper, TargetPosition target)
        {
            _motion.ToPointX(target.XPos);
            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
            }
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorR);
            _motion.WaitTillEnd(_motion.MotorZ);
            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromConveyorToRightBottom(Gripper gripper, TargetPosition target)
        {
            _motion.ToPointX(target.XPos);
            _motion.ToPoint(_motion.MotorZ, _motion.PickPosition.ApproachHeight);

            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
            }

            _motion.WaitTillXBiggerThan(_motion.ConveyorRightPosition.XPos);
            _motion.BreakToPoint(_motion.MotorZ, target.ApproachHeight);


            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorR);

            _motion.WaitTillEnd(_motion.MotorZ);
            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromConveyorToLeftTop(Gripper gripper, TargetPosition target)
        {
            _motion.ToPointX(target.XPos);
            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
            }
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorR);
            _motion.WaitTillEnd(_motion.MotorZ);
            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromConveyorToLeftBottom(Gripper gripper, TargetPosition target)
        {
            _motion.ToPointX(target.XPos);
            _motion.ToPoint(_motion.MotorZ, _motion.PickPosition.ApproachHeight);

            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
            }

            _motion.WaitTillXSmallerThan(_motion.ConveyorLeftPosition.XPos);
            _motion.BreakToPoint(_motion.MotorZ, target.ApproachHeight);

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorR);

            _motion.WaitTillEnd(_motion.MotorZ);
            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromConveyorToConveyor(Gripper gripper, TargetPosition target)
        {
            _motion.ToPointX(target.XPos);
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);

            MoveGripperTillEnd(target, gripper);

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorZ);

            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _motion.BreakToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveGripperTillEnd(TargetPosition target, Gripper gripper)
        {
            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
                _steppers.ToPoint(Gripper.One, target.APos);
                _steppers.ToPoint(Gripper.Two, 0);
                _steppers.WaitTillEnd(Gripper.One, target.APos);
                _steppers.WaitTillEnd(Gripper.Two);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
                _steppers.ToPoint(Gripper.Two, target.APos);
                _steppers.ToPoint(Gripper.One, 0);
                _steppers.WaitTillEnd(Gripper.One);
                _steppers.WaitTillEnd(Gripper.Two, target.APos);
            }

            _motion.WaitTillEnd(_motion.MotorR);
        }

        public void Unload(Gripper gripper, TargetPosition holder)
        {

        }

        public void UnloadAndLoad(Gripper gripper, TargetPosition holder)
        {

        }
    }
}
