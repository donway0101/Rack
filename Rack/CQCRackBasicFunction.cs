using System;
using System.Threading.Tasks;
using ACS.SPiiPlusNET;
using Motion;
using GripperStepper;
using EcatIo;
using Conveyor;
using  ShieldBox;

namespace Rack
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>If estop button is on, then ethercat bus error occur, notify user, use reboot method</remarks>
    /// Power up, Ethercat error occur, wire problem? 
    public partial class CqcRack
    {      
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
                if ( !Io.GetInput(Input.Gripper01Loose))
                {
                    throw new Exception("Gripper one is not opened.");
                }
            }
            else
            {
                if (!Io.GetInput(Input.Gripper02Loose))
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

        public void Load(StepperMotor gripper, BpShieldBox shieldBox)
        {
            TargetPosition holder = ConvertShieldBoxToTargetPosition(shieldBox);
            if (shieldBox.State != State.Open)
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

        private TargetPosition ConvertShieldBoxToTargetPosition(BpShieldBox shieldBox)
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
                if (!Io.GetInput(Input.Gripper01Loose))
                {
                    throw new Exception("Gripper one is not opened.");
                }
            }
            else
            {
                if (!Io.GetInput(Input.Gripper02Loose))
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
            if (gripper == StepperMotor.Two & target.Id != Location.Pick)
            {
                target.XPos = target.XPos + Motion.G1ToG2Offset.XPos;
                target.YPos = target.YPos - Motion.G1ToG2Offset.YPos;
                target.APos = target.APos - Motion.G1ToG2Offset.APos;
            }

            return target;
        }

    }
}
