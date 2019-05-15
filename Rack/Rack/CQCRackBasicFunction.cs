﻿using System;
using System.Diagnostics;
using System.Threading;
using ACS.SPiiPlusNET;

namespace Rack
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>If estop button is on, then ethercat bus error occur, notify user, use reboot method</remarks>
    /// Power up, Ethercat error occur, wire problem? 
    public partial class CqcRack
    {
        public CqcRack(string controllerIp)
        {
            _ip = controllerIp;
        }

        public void Start()
        {
            SetupComplete = false;

            if (EthercatOnline)
            {
                if (_ch.IsConnected == false)
                {
                    EnableEvent();
                    _ch.OpenCommEthernet(_ip, 701);
                }
                if (EcatIo == null)
                {
                    EcatIo = new EthercatIo(_ch, 72, 7, 4);
                }
                EcatIo.Setup();
            }

            if (MotorsOnline)
            {
                if (Motion == null)
                {
                    Motion = new EthercatMotion(_ch, 5);
                }

                if (Motion.MotorSetupComplete == false)
                {
                    Motion.Setup();
                }
                Motion.LoadPositions();
                SetMotorSpeed(DefaultRobotSpeed);

                if (EthercatOnline)
                {
                    EcatIo.MapEtherCAT();
                }
            }
            
            if (StepperOnline)
            {
                if (Steppers == null)
                {
                    Steppers = new Stepper("COM6");
                }
                Steppers.Setup();
                SetStepperSpeed(DefaultRobotSpeed);
            }

            if (ConveyorOnline)
            {
                if (Conveyor == null)
                {
                    Conveyor = new Conveyor(EcatIo);
                }

                Conveyor.Start();

                Conveyor.ErrorOccured -= Conveyor_ErrorOccured;
                Conveyor.ErrorOccured += Conveyor_ErrorOccured;

                StartConveyorManager();
            }

            if (ShieldBoxOnline)
            {
                ShieldBoxSetup();
                if (TesterOnline)
                {
                    TesterSetup();
                }
            }
         
            if (ScannerOnline)
            {
                if (Scanner==null)
                {
                    Scanner = new Scanner("COM12");
                    Scanner.InfoOccured -= Scanner_InfoOccured;
                    Scanner.InfoOccured += Scanner_InfoOccured;
                    Scanner.ErrorOccured -= Scanner_ErrorOccured;
                    Scanner.ErrorOccured += Scanner_ErrorOccured;
                }
                Scanner.Start();               
            }

            SelfChecking();

            SetupComplete = true;
        }

        private void Scanner_ErrorOccured(object sender, int code, string description)
        {
            OnErrorOccured(code, description);
        }

        private void Scanner_InfoOccured(object sender, int code, string description)
        {
            OnInfoOccured(code, description);
        }

        public void StartPhoneServer()
        {
            if (_phoneServerThread == null)
            {
                _phoneServerThread = new Thread(PhoneServer)
                {
                    IsBackground = true
                };
            }

            if (_phoneServerThread.IsAlive == false)
            {
                _phoneServerThread.Start();
            }

            PhoneServerManualResetEvent.Set();
            OnInfoOccured(20009, "Start production.");
        }

        public void PausePhoneServer()
        {
            PhoneServerManualResetEvent.Reset();
            OnInfoOccured(20010, "Pause production.");
        }

        private void Delay(int millisec)
        {
            Thread.Sleep(millisec);
        }

        public void StopPhoneServer()
        {
            if (_phoneServerThread != null)
            {
                _phoneServerThread.Abort();
                _phoneServerThread.Join();
            }
        }

        private void EnableEvent()
        {
            if (_eventEnabled == false)
            {
                _ch.ETHERCATERROR += _ch_ETHERCATERROR;
                _eventEnabled = true;
            }
        }

        private void _ch_ETHERCATERROR(ulong param)
        {
            OnErrorOccured(40006, "Acs ethercat error（check Estop or network wire)." + param);
        }

        public void SelfChecking()
        {
            //Todo if gripper is not empty, then exception.
        }

        public void Stop()
        {
            Motion.DisableAll();
        }

        public void HomeRobot(double homeSpeed = 5.0)
        {
            if (SetupComplete == false)
            {
                throw new Exception("Setup not Complete.");
            }
            Motion.SetSpeed(homeSpeed);

            Motion.EnableAll();

            Steppers.Enable(RackGripper.One);
            Steppers.Enable(RackGripper.Two);

            var currentPosition = GetRobotCurrentPose();        

            if (currentPosition.XPos < Motion.ConveyorRightPosition.XPos &
                currentPosition.XPos > Motion.ConveyorLeftPosition.XPos) //Robot is in conveyor zone.
            {
                if (currentPosition.YPos > YIsInBox) //Y is dangerous
                {
                    TargetPosition currentHolder = new TargetPosition() { TeachPos = TeachPos.Home };
                    double tolerance = 50.0;
                    foreach (var pos in Motion.Locations)
                    {
                        if (Math.Abs(currentPosition.XPos - pos.XPos) < tolerance &
                            Math.Abs(currentPosition.YPos - pos.YPos) < tolerance &
                            (currentPosition.ZPos > pos.ZPos - tolerance && currentPosition.ZPos < pos.ApproachHeight + tolerance))
                        {
                            currentHolder = pos;
                        }
                    }

                    if (currentHolder.TeachPos != TeachPos.Home)
                    {
                        MoveToPointTillEnd(Motion.MotorZ, currentHolder.ApproachHeight);
                        MoveToPointTillEnd(Motion.MotorR, currentHolder.RPos);
                        MoveToPointTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
                        MoveToPointTillEnd(Motion.MotorZ, Motion.HomePosition.ZPos);
                        MoveX1X2ToPointTillEnd(Motion.HomePosition.XPos);
                        MoveToPointTillEnd(Motion.MotorR, Motion.HomePosition.RPos);
                        HomeGrippers();
                    }
                    else
                    {
                        throw new Exception("Gripper is in unknown conveyor area, please move Y and manually then retry.");
                    }
                }
                else
                {
                    if (currentPosition.YPos < YIsNearHome)
                    {
                        MoveToPointTillEnd(Motion.MotorZ, Motion.HomePosition.ZPos);
                        MoveToPointTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
                        MoveX1X2ToPointTillEnd(Motion.HomePosition.XPos);
                        MoveToPointTillEnd(Motion.MotorR, Motion.HomePosition.RPos);

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
                    //X Y Z tolerance 50mm. then is inside box
                    TargetPosition currentHolder = new TargetPosition() { TeachPos = TeachPos.Home };
                    double tolerance = 50;
                    foreach (var pos in Motion.Locations)
                    {
                        if (Math.Abs(currentPosition.XPos - pos.XPos) < tolerance &
                            Math.Abs(currentPosition.YPos - pos.YPos) < tolerance &
                            (currentPosition.ZPos > pos.ZPos - tolerance && currentPosition.ZPos < pos.ApproachHeight + tolerance))
                        {
                            currentHolder = pos;
                            break;
                        }
                    }

                    if (currentHolder.TeachPos != TeachPos.Home)
                    {
                        MoveToPointTillEnd(Motion.MotorZ, currentHolder.ApproachHeight);
                        MoveToPointTillEnd(Motion.MotorR, currentHolder.RPos);
                        MoveToPointTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
                        MoveToPointTillEnd(Motion.MotorZ, Motion.HomePosition.ZPos);
                        MoveX1X2ToPointTillEnd(Motion.HomePosition.XPos);
                        MoveToPointTillEnd(Motion.MotorR, Motion.HomePosition.RPos);
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

                        MoveToPointTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
                        MoveToPointTillEnd(Motion.MotorZ, Motion.HomePosition.ZPos);
                        MoveX1X2ToPointTillEnd(Motion.HomePosition.XPos);
                        MoveToPointTillEnd(Motion.MotorR, Motion.HomePosition.RPos);

                        //Disable one of the motor.
                        HomeGrippers();
                    }
                    else
                    {
                        throw new Exception("Gripper is in unknown position, please home Y and manually then retry.");
                    }
                }
            }

            if (EcatIo.GetInput(Input.Gripper01) | EcatIo.GetInput(Input.Gripper02))
            {
                throw new Exception("Has phone on gripper, take it down first.");
            }

            OpenGripper(RackGripper.One);
            OpenGripper(RackGripper.Two);

            RobotHomeComplete = true;
        }

        public void Pick(RackGripper gripper = RackGripper.One, bool releaseConveyor = true)
        {
            OnInfoOccured(20016, "About to pick phone on conveyor with " + gripper + ".");
            if (LatestPhone != null)
            {
                if (LatestPhone.OnGripper != RackGripper.None)
                {
                    OnInfoOccured(20016, "Already got a phone in " + gripper + ".");
                    return;
                }
            }
            else
            {
                throw new Exception("Phone is not ready.");
            }

            if (EcatIo.GetInput(Input.PickHasPhone) == false)
            {
                throw new Exception("No phone in pick position. Quit picking.");
            }

            RobotTakeControlOnConveyor();

            CheckSafety();

            CheckGripperAvailable(gripper);

            Conveyor.ReadyForPicking();

            TargetPosition target = Motion.PickPosition;
            if (gripper == RackGripper.Two)
            {
                target.XPos = target.XPos + Motion.PickOffset.XPos;
            }
            
            MoveToTargetPosition(gripper, target);
            CloseGripper(gripper);
            MoveToPointTillEnd(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            MoveToPointTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
            LatestPhone.OnGripper = gripper;

            if (releaseConveyor)
            {
                RobotReleaseControlOnConveyor();
            }

            OnInfoOccured(20017, "Pick phone succeed.");
        }

        private void MoveToPointTillEnd(Motor motor, double point, int timeout=120000)
        {
            if (RobotInSimulateMode)
                return;
            Motion.ToPointWaitTillEnd(motor, point, timeout);
        }

        private void MoveX1X2ToPointTillEnd(double point, int timeout = 120000)
        {
            if (RobotInSimulateMode)
                return;
            Motion.ToPointXWaitTillEnd(point, timeout);
        }

        private void OkToReloadOnConveyor()
        {
            LatestPhone = null;
        }

        /// <summary>
        ///
        /// </summary>
        /// If a new phone went into pick position, it has to be picked before
        ///    place a pass phone, in <see cref="ArrangePhones"/> , it serve new 
        ///    phone before wifi phone(second priority), and new phone will only come in
        ///    when there is empty enough Rf box.
        /// <param name="gripper"></param>
        public void Place(RackGripper gripper)
        {
            OnInfoOccured(20026, "Try placing with " + gripper + ".");
            if (Conveyor.HasPlaceAPhone==true || EcatIo.GetInput(Input.PickHasPhone) == true)
            {
                throw new Exception("Last place movement has't finished.");
            }

            RobotTakeControlOnConveyor();

            Conveyor.StopBeltPick();

            CheckSafety();

            if (Conveyor.PickPhoneSensor())
            {
                if (LatestPhone != null)
                {
                    RackGripper theOtherGripper = gripper == RackGripper.One ? RackGripper.Two : RackGripper.One;
                    Pick(theOtherGripper, false);
                }
                else
                {
                    throw new Exception("When place, has an unknown phone.");
                }
            }

            if (Conveyor.PickPhoneSensor())
            {
                throw new Exception("When place, has an unknown phone in pick position.");
            }

            if (EcatIo.GetInput(Input.UpBlockPickForward) ||
                EcatIo.GetInput(Input.UpBlockPickBackward) || EcatIo.GetInput(Input.ClampTightPick))
            {
                throw new Exception("When place, cylinder on conveyor is up.");
            }

            CheckPhoneLost(gripper);

            TargetPosition placePosition = Motion.PickPosition;
            placePosition.XPos = placePosition.XPos + 0.5;
            MoveToTargetPosition(gripper, placePosition);
            OpenGripper(gripper);
            MoveToPointTillEnd(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            MoveToPointTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
            Conveyor.HasPlaceAPhone = true;

            RobotReleaseControlOnConveyor();
            OnInfoOccured(20026, "Finish placing with " + gripper + ".");
        }

        /// <summary>
        /// Phone is not on gripper after deliver.
        /// </summary>
        /// <param name="gripper"></param>
        private void CheckPhoneLost(RackGripper gripper)
        {
            if (gripper == RackGripper.One)
            {
                if (!EcatIo.GetInput(Input.Gripper01))
                {
                    throw new Exception("Gripper one is empty when place.");
                }
            }
            else
            {
                if (!EcatIo.GetInput(Input.Gripper02))
                {
                    throw new Exception("Gripper two is empty when place.");
                }
            }
        }

        public void Bin(RackGripper gripper)
        {
            OnInfoOccured(20019, "Try binning phone with " + gripper + ".");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (Conveyor.NgFullWarning)
            {
                if (stopwatch.ElapsedMilliseconds > 2*60*60*1000) // 2 hour waiting.
                {
                    throw new Exception("Can't bin because ng conveyor is full.");
                }
                Delay(1000);
            }

            if (Conveyor.HasBinAPhone == true || EcatIo.GetInput(Input.ConveyorBinIn) == true ||
                EcatIo.GetInput(Input.ConveyorBinInTwo) == true)
            {
                throw new Exception("Last bin movement has't finished.");
            }

            Conveyor.RobotBinning = true;
            ShieldBox3.RobotBining = true;
            bool needReopen = false;

            if (ShieldBox3.IsClosed() == false)
            {
                ShieldBox3.CloseBox(5000, false);
                needReopen = true;
            }

            Conveyor.StopBeltBin();

            MoveToTargetPosition(gripper, Motion.BinPosition);
            OpenGripper(gripper);

            Conveyor.HasBinAPhone = true;

            MoveToPointTillEnd(Motion.MotorZ, Motion.BinPosition.ApproachHeight);
            MoveToPointTillEnd(Motion.MotorY, Motion.HomePosition.YPos);

            Conveyor.RunBeltBin();

            if (needReopen)
            {
                ShieldBox3.OpenBox(5000, true, true);
            }
            ShieldBox3.RobotBining = false;
            Conveyor.RobotBinning = false;

            OnInfoOccured(20019, "Finish binning phone with " + gripper + ".");
        }

        public void Load(RackGripper gripper, TargetPosition position)
        {
            MoveToTargetPosition(gripper, position);
            OpenGripper(gripper);
            MoveToPointTillEnd(Motion.MotorZ, position.ApproachHeight);
            MoveToPointTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
        }

        public void Load(RackGripper gripper, ShieldBox box)
        {
            OnInfoOccured(20023, "Try loading phone to box:" + box.Id + " with " + gripper + ".");
            TargetPosition targetPosition = ConvertShieldBoxToTargetPosition(box);
            if (box.IsClosed() == true)
            {
                throw new Exception("Box " + box.Id + " is not opened");
            }            
            MoveToTargetPosition(gripper, targetPosition);
            OpenGripper(gripper);
            MoveToPointTillEnd(Motion.MotorZ, targetPosition.ApproachHeight);
            MoveToPointTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
            OnInfoOccured(20023, "Finish loading phone to box:" + box.Id + ".");
        }        

        public void Unload(RackGripper gripper, TargetPosition position)
        {
            CheckGripperAvailable(gripper);
            MoveToTargetPosition(gripper, position);
            CloseGripper(gripper);
            MoveToPointTillEnd(Motion.MotorZ, position.ApproachHeight);
            MoveToPointTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
        }

        /// <summary>
        /// From current target position.
        /// </summary>
        /// <param name="gripper"></param>
        /// <param name="phone"></param>
        public void Unload(RackGripper gripper, Phone phone)
        {
            OnInfoOccured(20018, "Try unloading phone:" + phone.Id + " with " + gripper + ".");
            TargetPosition pos = phone.CurrentTargetPosition;
            MoveToTargetPosition(gripper, pos);
            CloseGripper(gripper);
            MoveToPointTillEnd(Motion.MotorZ, pos.ApproachHeight);
            MoveToPointTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
            OnInfoOccured(20018, "Finish unloading phone:" + phone.Id);
        }

        private RackGripper GetAvailableGripper()
        {
            if (EcatIo.GetInput(Input.Gripper01Loose) && !EcatIo.GetInput(Input.Gripper01))
            {
                return RackGripper.One;
            }

            if (EcatIo.GetInput(Input.Gripper02Loose) && !EcatIo.GetInput(Input.Gripper02))
            {
                return RackGripper.Two;
            }

            throw new Exception("GetAvailableGripper failed.");
        }

        private void CheckGripperAvailable(RackGripper gripper)
        {
            if (gripper == RackGripper.One)
            {
                if (!EcatIo.GetInput(Input.Gripper01Loose))
                {
                    throw new Exception("Gripper one is not opened.");
                }
            }
            else
            {
                if (!EcatIo.GetInput(Input.Gripper02Loose))
                {
                    throw new Exception("Gripper two is not opened.");
                }
            }

            if (gripper == RackGripper.One)
            {
                if (EcatIo.GetInput(Input.Gripper01))
                {
                    throw new Exception("Gripper one is not empty.");
                }
            }
            else
            {
                if (EcatIo.GetInput(Input.Gripper02))
                {
                    throw new Exception("Gripper two is not empty.");
                }
            }
        }

        private void CheckSafety()
        {
            if (SystemFault && TestRun==false)
            {
                throw new Exception("System Fault, need to reset first.");
            }

            if (ConveyorFault)
            {
                throw new Exception("Conveyor Fault, need to reset first.");
            }
        }

        //public void UnloadAndLoad(TargetPosition target, RackGripper gripper)
        //{
        //    CheckGripperAvailable(gripper);
        //    MoveToTargetPosition(gripper, target);
        //    CloseGripper(gripper);
        //    MoveToPointTillEnd(Motion.MotorZ, target.ApproachHeight); //Up.
        //    ChangeGripper(target, ref gripper); //Switch gripper.
        //    MoveToPointTillEnd(Motion.MotorZ, target.ZPos); //Down.
        //    OpenGripper(gripper);
        //    MoveToPointTillEnd(Motion.MotorZ, target.ApproachHeight); //Up.
        //    MoveToPointTillEnd(Motion.MotorY, Motion.HomePosition.YPos); //Back.
        //}

        public void UnloadAndLoad(ShieldBox box, RackGripper gripper)
        {
            OnInfoOccured(20025, "Try unload and load box: " + box.Id + " with " + gripper + ".");
            CheckGripperAvailable(gripper);
            if (box.IsClosed()==true)
            {
                throw new Exception("Box " + box.Id + " is not opened.");
            }
            MoveToTargetPosition(gripper, box.Position);
            CloseGripper(gripper);
            MoveToPointTillEnd(Motion.MotorZ, box.Position.ApproachHeight); //Up.
            ChangeGripper(box.Position, ref gripper); //Switch gripper.
            MoveToPointTillEnd(Motion.MotorZ, box.Position.ZPos); //Down.
            OpenGripper(gripper);
            MoveToPointTillEnd(Motion.MotorZ, box.Position.ApproachHeight); //Up.
            MoveToPointTillEnd(Motion.MotorY, Motion.HomePosition.YPos); //Back.
            OnInfoOccured(20025, "Finish unload and load box: " + box.Id + " with " + gripper + ".");
        }

        private void ChangeGripper(TargetPosition target, ref RackGripper gripper)
        {
            gripper = gripper == RackGripper.One ? RackGripper.Two : RackGripper.One;
            target = AddOffset(gripper, target);
            Motion.ToPointX(target.XPos);
            Motion.ToPoint(Motion.MotorY, target.YPos);
            ToPointWaitTillEndGripper(target, gripper);
            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorY);
        }

        private TargetPosition AddOffset(RackGripper gripper, TargetPosition target)
        {
            if (gripper == RackGripper.Two && target.TeachPos != TeachPos.Pick)
            {
                target.XPos = target.XPos + Motion.G1ToG2Offset.XPos;
                target.YPos = target.YPos - Motion.G1ToG2Offset.YPos;
                target.APos = target.APos - Motion.G1ToG2Offset.APos;
            }
            return target;
        }

        public void Reset()
        {
            Motion.SetSpeedImm(DefaultRobotSpeed);
            //SystemFault = false;
            RobotReleaseControlOnConveyor();
            OkToReloadOnConveyor();          
        }
    }
}
