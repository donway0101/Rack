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
            Conveyor.ErrorOccured += Conveyor_ErrorOccured;
        }

        private void Conveyor_ErrorOccured(object sender, string description)
        {
            OnErrorOccured(description);
        }

        public void Start()
        {
            SetupComplete = false;

            if (_ch.IsConnected == false)
            {
                _ch.OpenCommEthernet(_ip, 701);
            }

            if (Motion == null)
            {
                Motion = new EthercatMotion(_ch, 5);
            }

            if (Motion.MotorSetupComplete == false)
            {
                Motion.Setup();
            }                        
            Motion.LoadPositions();

            if (EcatIo == null)
            {
                EcatIo = new EthercatIo(_ch, 72, 7, 4);
            }            
            EcatIo.Setup();

            if (Conveyor == null)
            {
                Conveyor = new Conveyor(EcatIo);
            }           
            Conveyor.Start();

            if (Steppers == null)
            {
                Steppers = new Stepper("COM3");
            }

            if (_gripperIsOnline)
            {
                Steppers.Setup();
            }

            //Todo read xml and setup boxes.
            ShieldBox1 = new ShieldBox(1, "COM3");

            //ShieldBoxs = new ShieldBox[1] { ShieldBox1 };

            SetSpeed(10);

            SetupComplete = true;
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

            //Careful is robot is holding a phone.

            //Box state should either be open or close.

            var currentPosition = GetRobotCurrentPose();

            if (currentPosition.XPos < Motion.ConveyorRightPosition.XPos &
                currentPosition.XPos > Motion.ConveyorLeftPosition.XPos) //Robot is in conveyor zone.
            {
                if (currentPosition.YPos > YIsInBox) //Y is dangerous
                {
                    TargetPosition currentHolder = new TargetPosition() { Id = TeachPos.Home };
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

                    if (currentHolder.Id != TeachPos.Home)
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

                    TargetPosition currentHolder = new TargetPosition() { Id = TeachPos.Home };
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

                    if (currentHolder.Id != TeachPos.Home)
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

        public void Pick(StepperMotor gripper)
        {
            if ( Conveyor.ReadyForPicking & _testRun==false)
            {
                Conveyor.ReadyForPicking = false;
            }
            else
            {
                throw new Exception("Phone is not ready.");
            }

            if ( gripper == StepperMotor.One)
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
            if (gripper == StepperMotor.Two)
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

        public void Place(StepperMotor gripper)
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

        public void Bin(StepperMotor gripper)
        {
            //TODO make sure pick position is empty, conveyor is not stucked and gripper is full.
            MoveToTargetPosition(gripper, Motion.BinPosition);
            //Open cylinder.
            Motion.ToPointWaitTillEnd(Motion.MotorZ, Motion.BinPosition.ApproachHeight);
            //Check phone is on conveyor.
        }

        public void Load(StepperMotor gripper, TargetPosition holder)
        {
            //Todo make sure box is open.
            MoveToTargetPosition(gripper, holder);
            OpenGripper(gripper);
            Motion.ToPointWaitTillEnd(Motion.MotorZ, holder.ApproachHeight);
            //Box is OK to close.
            Motion.ToPointWaitTillEnd(Motion.MotorY, Motion.PickPosition.YPos);
        }

        public void Load(StepperMotor gripper, ShieldBox shieldBox)
        {
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

        private TargetPosition ConvertShieldBoxToTargetPosition(ShieldBox shieldBox)
        {
            TargetPosition target = Motion.HomePosition;
            switch (shieldBox.Id)
            {
                case 1:
                    target = Motion.ShieldBox1;
                    break;
                case 2:
                    target = Motion.ShieldBox2;
                    break;
                case 3:
                    target = Motion.ShieldBox3;
                    break;
                case 4:
                    target = Motion.ShieldBox4;
                    break;
                case 5:
                    target = Motion.ShieldBox5;
                    break;
                case 6:
                    target = Motion.ShieldBox6;
                    break;
                default:
                    break;
            }

            return target;
        }

        public void Unload(StepperMotor gripper, TargetPosition holder)
        {
            if (gripper == StepperMotor.One)
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

        public void UnloadAndLoad(TargetPosition target, StepperMotor gripper)
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

        private void SwitchGripper(TargetPosition target, ref StepperMotor gripper)
        {
            gripper = gripper == StepperMotor.One ? StepperMotor.Two : StepperMotor.One;

            target = AddOffset(gripper, target);
            Motion.ToPointX(target.XPos);
            Motion.ToPoint(Motion.MotorY, target.YPos);

            ToPointWaitTillEndGripper(target, gripper);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorY);
        }

        private TargetPosition AddOffset(StepperMotor gripper, TargetPosition target)
        {
            if (gripper == StepperMotor.Two & target.Id != TeachPos.Pick)
            {
                target.XPos = target.XPos + Motion.G1ToG2Offset.XPos;
                target.YPos = target.YPos - Motion.G1ToG2Offset.YPos;
                target.APos = target.APos - Motion.G1ToG2Offset.APos;
            }

            return target;
        }

    }
}
