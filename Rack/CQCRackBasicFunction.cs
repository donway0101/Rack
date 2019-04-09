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
            
            //If system is OK, gripper is free and opened, conveyor is ready
            //If the other gripper is holding a phone, then conveyor can not reload.
            MoveToTargetPosition(gripper, Motion.PickPosition);
            //Close cylinder.
            CloseGripper(gripper);
            Motion.ToPointWaitTillEnd(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            //Check.
        }

        public void Place(StepperMotor gripper)
        {
            //After place, conveyor can reload.
            //TODO make sure pick position is empty, conveyor is not stucked and gripper is full.
            TargetPosition placePosition = new TargetPosition()
            {
                APos = Motion.PickPosition.APos,
                ApproachHeight = Motion.PickPosition.ApproachHeight,
                Id = Motion.PickPosition.Id,
                RPos = Motion.PickPosition.RPos,
                XPos = Motion.PickPosition.XPos+0.5,
                YPos = Motion.PickPosition.YPos,
                ZPos = Motion.PickPosition.ZPos
            };
            
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
                target.XPos = target.XPos + 1.44467773437;
                target.YPos = target.YPos - 0.918862304687;
                target.APos = target.APos - 0.15;
            }

            return target;
        }

    }
}
