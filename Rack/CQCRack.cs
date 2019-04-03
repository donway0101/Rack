using System;
using System.Threading.Tasks;
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
    public class CqcRack
    {
        private readonly Api _ch = new Api();
        public EthercatMotion _motion;
        public Stepper _gripper;
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
     
            if (_gripperIsOnline)
            {
                if (_gripper == null)
                {
                    _gripper = new Stepper("COM3");                   
                }
                _gripper.Setup();
            }

            SetSpeed(10);

            SetupComplete = true;
        }

        public void Stop()
        {
            _motion.DisableAll();
            //_motion.KillAll();
        }

        public void Test()
        {
            SetSpeed(20);

            //PickAndLoad();
            //LoadGold();
            //UnloadAndBin();
            //TestRun();
            TestUnloadAndLoadHolders();

            //Task.Run(() =>
            //{
            //    Place(Gripper.One);
            //    Place(Gripper.Two);
            //});
        }

        private void TestLoadGold()
        {
            Task.Run(() =>
            {
                Load(Gripper.One, _motion.Gold1);
                Load(Gripper.One, _motion.Holder1);
                Load(Gripper.One, _motion.Gold2);
                Load(Gripper.One, _motion.Holder2);
                Load(Gripper.One, _motion.Gold3);
                Load(Gripper.One, _motion.Holder3);
                Load(Gripper.One, _motion.Gold4);
                Load(Gripper.One, _motion.Holder4);
                Load(Gripper.One, _motion.Gold5);
                Load(Gripper.One, _motion.Holder5);
                Load(Gripper.One, _motion.Gold1);
                Load(Gripper.One, _motion.Holder6);
                Load(Gripper.One, _motion.HomePosition);
            });
        }

        private void TestUnloadAndBin()
        {
            Task.Run(() =>
            {
                SetSpeed(20);
                Unload(Gripper.Two, _motion.Holder1);
                Unload(Gripper.Two, _motion.BinPosition);
                Unload(Gripper.Two, _motion.Holder2);
                Unload(Gripper.Two, _motion.BinPosition);
                Unload(Gripper.Two, _motion.Holder3);
                Unload(Gripper.Two, _motion.BinPosition);
                Unload(Gripper.Two, _motion.Holder4);
                Unload(Gripper.Two, _motion.BinPosition);
                Unload(Gripper.Two, _motion.Holder5);
                Unload(Gripper.Two, _motion.BinPosition);
                Unload(Gripper.Two, _motion.Holder6);
                Unload(Gripper.Two, _motion.BinPosition);
                Unload(Gripper.Two, _motion.HomePosition);
            });
        }

        private void TestUnloadAndLoadHolders()
        {
            Task.Run(() =>
            {
                SetSpeed(20);
                UnloadAndLoad(_motion.PickPosition, Gripper.One);
                //UnloadAndLoad(_motion.Holder1, Gripper.Two);
                //UnloadAndLoad(_motion.PickPosition, Gripper.One);
                //UnloadAndLoad(_motion.Holder2, Gripper.Two);
                //UnloadAndLoad(_motion.PickPosition, Gripper.One);
                //UnloadAndLoad(_motion.Holder3, Gripper.Two);
                //UnloadAndLoad(_motion.PickPosition, Gripper.One);
                //UnloadAndLoad(_motion.Holder4, Gripper.Two);
                //UnloadAndLoad(_motion.PickPosition, Gripper.One);
                //UnloadAndLoad(_motion.Holder5, Gripper.Two);
                //UnloadAndLoad(_motion.PickPosition, Gripper.One);
                //UnloadAndLoad(_motion.Holder6, Gripper.Two);
            });
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
                _gripper.SetSpeed(Gripper.One, stepperSpeed);
                _gripper.SetSpeed(Gripper.Two, stepperSpeed); 
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
                _gripper.HomeMotor(Gripper.One, -6);
                _gripper.HomeMotor(Gripper.Two, -2);
            }
        }

        public void SwitchGripper(TargetPosition target, Gripper gripper)
        {
            gripper = gripper== Gripper.One ? Gripper.Two : Gripper.One;
            ToPointWaitTillEndGripper(target, gripper);
        }

        public void Pick(Gripper gripper)
        {
            //If system is OK, gripper is free and opened, conveyor is ready
            //If the other gripper is holding a phone, then conveyor can not reload.
            MoveToTargetPosition(gripper, _motion.PickPosition);
            //Close cylinder.
            _motion.ToPointWaitTillEnd(_motion.MotorZ, _motion.PickPosition.ApproachHeight);
            //Check.
        }

        public void Place(Gripper gripper)
        {
            //After place, conveyor can reload.
            //TODO make sure pick position is empty, conveyor is not stucked and gripper is full.
            TargetPosition placePosition = new TargetPosition()
            {
                APos = _motion.PickPosition.APos,
                ApproachHeight = _motion.PickPosition.ApproachHeight,
                Id = _motion.PickPosition.Id,
                RPos = _motion.PickPosition.RPos,
                XPos = _motion.PickPosition.XPos+1,
                YPos = _motion.PickPosition.YPos,
                ZPos = _motion.PickPosition.ZPos
            };
            
            MoveToTargetPosition(gripper, placePosition);
            //Open cylinder.
            _motion.ToPointWaitTillEnd(_motion.MotorZ, _motion.PickPosition.ApproachHeight);
            //Check phone is on conveyor.
        }

        public void Bin(Gripper gripper)
        {
            //TODO make sure pick position is empty, conveyor is not stucked and gripper is full.
            MoveToTargetPosition(gripper, _motion.BinPosition);
            //Open cylinder.
            _motion.ToPointWaitTillEnd(_motion.MotorZ, _motion.BinPosition.ApproachHeight);
            //Check phone is on conveyor.
        }

        public void Load(Gripper gripper, TargetPosition holder)
        {
            //Todo make sure box is open.
            MoveToTargetPosition(gripper, holder);
            //Open gripper
            _motion.ToPointWaitTillEnd(_motion.MotorZ, holder.ApproachHeight);
            //Box is OK to close.
            _motion.ToPointWaitTillEnd(_motion.MotorY, _motion.PickPosition.YPos);
        }

        public void Unload(Gripper gripper, TargetPosition holder)
        {
            //Todo make sure box is open.
            MoveToTargetPosition(gripper, holder);
            //Close gripper
            _motion.ToPointWaitTillEnd(_motion.MotorZ, holder.ApproachHeight);
            //Box is OK to close.
            _motion.ToPointWaitTillEnd(_motion.MotorY, _motion.PickPosition.YPos);
        }

        public void UnloadAndLoad(TargetPosition target, Gripper gripper)
        {
            //Todo make sure box is open.
            MoveToTargetPosition(gripper, target);
            //Open gripper
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ApproachHeight); //Up.
            SwitchGripper(target, gripper); //Switch gripper.
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ZPos); //Down.
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ApproachHeight); //Up.
            _motion.ToPointWaitTillEnd(_motion.MotorY, _motion.PickPosition.YPos); //Back.
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
                            MoveFromRightToLeftBottom(gripper, target);
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
            _motion.ToPointX(_motion.ConveyorRightPosition.XPos);
            ToPointGripper(target, gripper);

            _motion.WaitTillZBiggerThan(_motion.PickPosition.ApproachHeight - 20);
            _motion.BreakToPointX(target.XPos);
        
            _motion.WaitTillEndX();
            WaitTillEndGripper(target, gripper);

            MotorYOutThenBreakMotorZDown(target);
        }

        private void MotorYOutThenBreakMotorZDown(TargetPosition target)
        {
            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _gripper.CheckEnabled();
            _motion.BreakToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromRightToLeftBottom(Gripper gripper, TargetPosition target)
        {
            _motion.ToPoint(_motion.MotorZ, _motion.PickPosition.ApproachHeight);
            _motion.ToPointX(_motion.ConveyorRightPosition.XPos);
            ToPointGripper(target, gripper);

            _motion.WaitTillZBiggerThan(_motion.PickPosition.ApproachHeight - 20);
            _motion.BreakToPointX(target.XPos);

            _motion.WaitTillXSmallerThan(_motion.ConveyorLeftPosition.XPos);
            _motion.BreakToPoint(_motion.MotorZ, target.ApproachHeight);

            _motion.WaitTillEndX();          
            _motion.WaitTillEnd(_motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromRightToLeftTop(Gripper gripper, TargetPosition target, TargetPosition currentPosition)
        {
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            _motion.ToPointX(_motion.ConveyorRightPosition.XPos);
            ToPointGripper(target, gripper);
 
            _motion.WaitTillZBiggerThan(_motion.PickPosition.ApproachHeight - 20);
            _motion.BreakToPointX(target.XPos);

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromRightToRight(Gripper gripper, TargetPosition target)
        {
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            _motion.ToPointX(target.XPos);
            ToPointWaitTillEndGripper(target, gripper);
            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorZ);
            MotorYOutThenMotorZDown(target);
        }

        private void MotorYOutThenMotorZDown(TargetPosition target)
        {
            _motion.ToPointWaitTillEnd(_motion.MotorY, target.YPos);
            _gripper.CheckEnabled();
            _motion.ToPointWaitTillEnd(_motion.MotorZ, target.ZPos);
        }

        private void MoveFromLeftToRightTop(Gripper gripper, TargetPosition target, TargetPosition currentPosition)
        {
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            ToPointGripper(target, gripper);
            _motion.WaitTillZBiggerThan(_motion.PickPosition.ApproachHeight - 20);
            _motion.ToPointX(target.XPos);

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromLeftToRightBottom(Gripper gripper, TargetPosition target)
        {
            _motion.ToPoint(_motion.MotorZ, _motion.PickPosition.ApproachHeight);
            _motion.WaitTillZBiggerThan(_motion.PickPosition.ApproachHeight - 20);

            _motion.ToPointX(target.XPos);
            ToPointGripper(target, gripper);

            _motion.WaitTillXBiggerThan(_motion.ConveyorRightPosition.XPos);
            _motion.BreakToPoint(_motion.MotorZ, target.ApproachHeight);

            _motion.WaitTillEndX();           
            _motion.WaitTillEnd(_motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromLeftToLeft(Gripper gripper, TargetPosition target)
        {
            _motion.ToPointX(target.XPos);
            ToPointGripper(target, gripper);
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorZ);
            WaitTillEndGripper(target, gripper);
            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromLeftToConveyor(Gripper gripper, TargetPosition target, TargetPosition currentPosition)
        {
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            ToPointGripper(target, gripper);
            _motion.WaitTillZBiggerThan(_motion.PickPosition.ApproachHeight - 20);
            _motion.ToPointX(target.XPos);

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromConveyorToRightTop(Gripper gripper, TargetPosition target)
        {
            _motion.ToPointX(target.XPos);
            ToPointGripper(target, gripper);
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorZ);
            WaitTillEndGripper(target, gripper);
            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromConveyorToRightBottom(Gripper gripper, TargetPosition target)
        {
            _motion.ToPointX(target.XPos);
            _motion.ToPoint(_motion.MotorZ, _motion.PickPosition.ApproachHeight);
            ToPointGripper(target, gripper);

            _motion.WaitTillXBiggerThan(_motion.ConveyorRightPosition.XPos);
            _motion.BreakToPoint(_motion.MotorZ, target.ApproachHeight);

            _motion.WaitTillEndX();          
            _motion.WaitTillEnd(_motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromConveyorToLeftTop(Gripper gripper, TargetPosition target)
        {
            _motion.ToPointX(target.XPos);
            ToPointGripper(target, gripper);
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorZ);
            WaitTillEndGripper(target, gripper);
            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromConveyorToLeftBottom(Gripper gripper, TargetPosition target)
        {
            _motion.ToPointX(target.XPos);
            _motion.ToPoint(_motion.MotorZ, _motion.PickPosition.ApproachHeight);
            ToPointGripper(target, gripper);

            _motion.WaitTillXSmallerThan(_motion.ConveyorLeftPosition.XPos);
            _motion.BreakToPoint(_motion.MotorZ, target.ApproachHeight);

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromConveyorToConveyor(Gripper gripper, TargetPosition target)
        {
            _motion.ToPointX(target.XPos);
            _motion.ToPoint(_motion.MotorZ, target.ApproachHeight);
            ToPointGripper(target, gripper);

            _motion.WaitTillEndX();
            _motion.WaitTillEnd(_motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void ToPointWaitTillEndGripper(TargetPosition target, Gripper gripper)
        {
            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
                _gripper.ToPoint(Gripper.One, target.APos);
                _gripper.ToPoint(Gripper.Two, 0);
                _motion.WaitTillEnd(_motion.MotorR);
                _gripper.WaitTillEnd(Gripper.One, target.APos);
                _gripper.WaitTillEnd(Gripper.Two, 0);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
                _gripper.ToPoint(Gripper.Two, target.APos);
                _gripper.ToPoint(Gripper.One, 0);
                _motion.WaitTillEnd(_motion.MotorR);
                _gripper.WaitTillEnd(Gripper.One, 0);
                _gripper.WaitTillEnd(Gripper.Two, target.APos);
            }         
        }

        private void WaitTillEndGripper(TargetPosition target, Gripper gripper)
        {
            _motion.WaitTillEnd(_motion.MotorR);
            if (gripper == Gripper.One)
            {
                _gripper.WaitTillEnd(Gripper.One, target.APos);
                _gripper.WaitTillEnd(Gripper.Two, 0);
            }
            else
            {
                _gripper.WaitTillEnd(Gripper.One, 0);
                _gripper.WaitTillEnd(Gripper.Two, target.APos);
            }
        }

        private void ToPointGripper(TargetPosition target, Gripper gripper)
        {
            if (gripper == Gripper.One)
            {
                _motion.ToPoint(_motion.MotorR, target.RPos);
                _gripper.ToPoint(Gripper.One, target.APos);
                _gripper.ToPoint(Gripper.Two, 0);
            }
            else
            {
                _motion.ToPoint(_motion.MotorR, target.RPos - 60);
                _gripper.ToPoint(Gripper.Two, target.APos);
                _gripper.ToPoint(Gripper.One, 0);
            }
        }

        public void SaveTeachPosition(TeachPos selectedTeachPos)
        {
            XmlReaderWriter.SetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.XPos,
                _motion.GetPositionX().ToString());
            XmlReaderWriter.SetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.YPos,
                _motion.GetPosition(_motion.MotorY).ToString());
            XmlReaderWriter.SetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.ZPos,
                _motion.GetPosition(_motion.MotorZ).ToString());
            XmlReaderWriter.SetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.RPos,
                _motion.GetPosition(_motion.MotorR).ToString());

            XmlReaderWriter.SetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.APos,
                _motion.GetPosition(_motion.MotorR) > 0
                    ? _gripper.GetPosition(Gripper.One).ToString()
                    : _gripper.GetPosition(Gripper.Two).ToString());
        }

        public void SaveApproachHeight(TeachPos selectedTeachPos)
        {
            XmlReaderWriter.SetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.ApproachHeight,
                _motion.GetPosition(_motion.MotorZ).ToString());
        }
    }
}
