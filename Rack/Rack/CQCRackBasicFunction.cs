using System;

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
            //Conveyor.ErrorOccured += Conveyor_ErrorOccured;
        }

        public void Start()
        {
            SetupComplete = false;

            if (EthercatOnline)
            {
                if (_ch.IsConnected == false)
                {
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
            }

            if (ConveyorOnline)
            {
                if (Conveyor == null)
                {
                    Conveyor = new Conveyor(EcatIo);
                }
                Conveyor.Start();
                Conveyor.PickBufferPhoneComing += Conveyor_PickBufferPhoneComing;
            }
            
            if (StepperOnline)
            {
                if (Steppers == null)
                {
                    Steppers = new Stepper("COM3");
                }
                Steppers.Setup();
                SetStepperSpeed(DefaultRobotSpeed);
            }

            if (ShieldBoxOnline)
            {
                ShieldBoxSetup();
            }

            if (TesterOnline)
            {
                TesterSetup();
            }

            SelfChecking();

            SetupComplete = true;
        }

        private void Conveyor_PickBufferPhoneComing(object sender, string description)
        {
            //AddNewPhone();
        }

        public void SelfChecking()
        {
            //Todo if gripper is not empty, then exception.
        }

        public void Stop()
        {
            Motion.DisableAll();
            //_motion.KillAll();
        }

        public void HomeRobot(double homeSpeed = 5)
        {
            if (SetupComplete == false)
            {
                throw new Exception("Setup not Complete.");
            }
            Motion.SetSpeed(homeSpeed);

            Motion.EnableAll();
            Steppers.Enable(RackGripper.One);
            Steppers.Enable(RackGripper.Two);

            //Careful is robot is holding a phone.

            //Box state should either be open or close.

            var currentPosition = GetRobotCurrentPose();

            if (currentPosition.XPos < Motion.ConveyorRightPosition.XPos &
                currentPosition.XPos > Motion.ConveyorLeftPosition.XPos) //Robot is in conveyor zone.
            {
                if (currentPosition.YPos > YIsInBox) //Y is dangerous
                {
                    TargetPosition currentHolder = new TargetPosition() { TeachPos = TeachPos.Home };
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

                    if (currentHolder.TeachPos != TeachPos.Home)
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
                        Motion.ToPointWaitTillEnd(Motion.MotorZ, Motion.HomePosition.ZPos);
                        Motion.ToPointWaitTillEnd(Motion.MotorY, Motion.HomePosition.YPos);
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

                    TargetPosition currentHolder = new TargetPosition() { TeachPos = TeachPos.Home };
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

                    if (currentHolder.TeachPos != TeachPos.Home)
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

        public void Pick(RackGripper gripper)
        {
            if ( Conveyor.ReadyForPicking & TestRun==false)
            {
                Conveyor.ReadyForPicking = false;
            }
            else
            {
                throw new Exception("Phone is not ready.");
            }

            if ( gripper == RackGripper.One)
            {
                if ( !EcatIo.GetInput(Input.Gripper01Loose))
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

            TargetPosition target = Motion.PickPosition;
            if (gripper == RackGripper.Two)
            {
                target.XPos = target.XPos + Motion.PickOffset.XPos;
            }
            
            //If system is OK, gripper is free and opened, conveyor is ready
            //If the other gripper is holding a phone, then conveyor can not reload.
            MoveToTargetPosition(gripper, target);
            //Close cylinder.
            CloseGripper(gripper);
            Motion.ToPointWaitTillEnd(Motion.MotorZ, Motion.PickPosition.ApproachHeight);

            Conveyor.PickBeltOkToRun = true;
            //Check.
        }

        public void Place(RackGripper gripper)
        {
            //After place, conveyor can reload.
            //TODO make sure pick position is empty, conveyor is not stucked and gripper is full.
            TargetPosition placePosition = Motion.PickPosition;
            placePosition.XPos = placePosition.XPos + 0.5;

            MoveToTargetPosition(gripper, placePosition);
            //Open cylinder.
            OpenGripper(gripper);
            Motion.ToPointWaitTillEnd(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            //Check phone is on conveyor.
        }

        public void Bin(RackGripper gripper)
        {
            //TODO make sure pick position is empty, conveyor is not stucked and gripper is full.
            MoveToTargetPosition(gripper, Motion.BinPosition);
            //Open cylinder.
            Motion.ToPointWaitTillEnd(Motion.MotorZ, Motion.BinPosition.ApproachHeight);
            //Check phone is on conveyor.
        }

        public void Load(RackGripper gripper, TargetPosition holder)
        {
            //Todo make sure box is open.
            MoveToTargetPosition(gripper, holder);
            OpenGripper(gripper);
            Motion.ToPointWaitTillEnd(Motion.MotorZ, holder.ApproachHeight);
            //Box is OK to close.
            Motion.ToPointWaitTillEnd(Motion.MotorY, Motion.PickPosition.YPos);
        }

        public void Load(RackGripper gripper, ShieldBox shieldBox)
        {
            //Todo if not test run, than check empty and shield box open.
            TargetPosition holder = ConvertShieldBoxToTargetPosition(shieldBox);
            if (shieldBox.State != ShieldBoxState.Open)
            {
                throw new Exception("Box " + shieldBox.Id + " is not opened");
            }
            //Todo make sure box is open.
            MoveToTargetPosition(gripper, holder);
            OpenGripper(gripper);
            Motion.ToPointWaitTillEnd(Motion.MotorZ, holder.ApproachHeight);
            //Box is OK to close.
            Motion.ToPointWaitTillEnd(Motion.MotorY, Motion.PickPosition.YPos);
        }        

        public void Unload(RackGripper gripper, TargetPosition holder)
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
            //Todo make sure box is open.
            MoveToTargetPosition(gripper, holder);
            CloseGripper(gripper);
            Motion.ToPointWaitTillEnd(Motion.MotorZ, holder.ApproachHeight);
            //Box is OK to close.
            Motion.ToPointWaitTillEnd(Motion.MotorY, Motion.PickPosition.YPos);
        }

        public void UnloadAndLoad(TargetPosition target, RackGripper gripper)
        {
            //Todo make sure box is open.
            MoveToTargetPosition(gripper, target);
            CloseGripper(gripper);
            Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ApproachHeight); //Up.
            //Todo add offset.
            SwitchGripper(target, ref gripper); //Switch gripper.
            Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ZPos); //Down.
            OpenGripper(gripper);
            Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ApproachHeight); //Up.
            Motion.ToPointWaitTillEnd(Motion.MotorY, Motion.PickPosition.YPos); //Back.
        }

        private void SwitchGripper(TargetPosition target, ref RackGripper gripper)
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
            if (gripper == RackGripper.Two & target.TeachPos != TeachPos.Pick)
            {
                target.XPos = target.XPos + Motion.G1ToG2Offset.XPos;
                target.YPos = target.YPos - Motion.G1ToG2Offset.YPos;
                target.APos = target.APos - Motion.G1ToG2Offset.APos;
            }

            return target;
        }

    }
}
