using System;

namespace Rack
{
    public partial class CqcRack
    {
        private void MoveToTargetPosition(StepperMotor gripper, TargetPosition target)
        {
            target = AddOffset(gripper, target);

            var currentPosition = GetRobotCurrentPose();

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

            if (currentPosition.YPos > YIsNearHome)
            {
                throw new Exception("Y is out after previous movement");
            } //Dangerous, may have to 

            if (currentPosition.XPos > Motion.ConveyorRightPosition.XPos) //Now on right side.
            {
                #region Move from right to ...
                if (target.XPos > Motion.ConveyorRightPosition.XPos) //To right. 
                {
                    MoveFromRightToRight(gripper, target);
                }
                else
                {
                    if (target.XPos < Motion.ConveyorLeftPosition.XPos) //To left.
                    {
                        if (target.ZPos > Motion.PickPosition.ApproachHeight) //To left top.
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
                if (currentPosition.XPos < Motion.ConveyorLeftPosition.XPos) //Now on left side. 
                {
                    #region Move from left to ...
                    if (target.XPos > Motion.ConveyorRightPosition.XPos)//To right
                    {
                        if (target.ZPos < Motion.PickPosition.ApproachHeight) // To right bottom.
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
                        if (target.XPos < Motion.ConveyorLeftPosition.XPos)//To left.
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
                    if (target.XPos > Motion.ConveyorRightPosition.XPos) //To right.                
                    {
                        if (target.ZPos < Motion.PickPosition.ApproachHeight) // To right bottom.
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
                        if (target.XPos < Motion.ConveyorLeftPosition.XPos) //To left.
                        {
                            if (target.ZPos < Motion.PickPosition.ApproachHeight)// To left bottom.
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

        private void MoveFromRightToConveyor(StepperMotor gripper, TargetPosition target, TargetPosition currentPosition)
        {
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.ToPointX(Motion.ConveyorRightPosition.XPos);
            ToPointGripper(target, gripper);

            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);
            Motion.BreakToPointX(target.XPos);

            Motion.WaitTillEndX();
            WaitTillEndGripper(target, gripper);

            MotorYOutThenBreakMotorZDown(target);
        }

        private void MotorYOutThenBreakMotorZDown(TargetPosition target)
        {
            Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
            Stepper.CheckEnabled();
            Motion.BreakToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
        }

        private void MoveFromRightToLeftBottom(StepperMotor gripper, TargetPosition target)
        {
            Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            Motion.ToPointX(Motion.ConveyorRightPosition.XPos);
            ToPointGripper(target, gripper);

            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);
            Motion.BreakToPointX(target.XPos);

            Motion.WaitTillXSmallerThan(Motion.ConveyorLeftPosition.XPos);
            Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromRightToLeftTop(StepperMotor gripper, TargetPosition target, TargetPosition currentPosition)
        {
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.ToPointX(Motion.ConveyorRightPosition.XPos);
            ToPointGripper(target, gripper);

            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);
            Motion.BreakToPointX(target.XPos);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromRightToRight(StepperMotor gripper, TargetPosition target)
        {
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.ToPointX(target.XPos);
            ToPointWaitTillEndGripper(target, gripper);
            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            MotorYOutThenMotorZDown(target);
        }

        private void MotorYOutThenMotorZDown(TargetPosition target)
        {
            Motion.ToPointWaitTillEnd(Motion.MotorY, target.YPos);
            Stepper.CheckEnabled();
            Motion.ToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
        }

        private void MoveFromLeftToRightTop(StepperMotor gripper, TargetPosition target, TargetPosition currentPosition)
        {
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            ToPointGripper(target, gripper);
            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);
            Motion.ToPointX(target.XPos);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromLeftToRightBottom(StepperMotor gripper, TargetPosition target)
        {
            Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);

            Motion.ToPointX(target.XPos);
            ToPointGripper(target, gripper);

            Motion.WaitTillXBiggerThan(Motion.ConveyorRightPosition.XPos);
            Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromLeftToLeft(StepperMotor gripper, TargetPosition target)
        {
            Motion.ToPointX(target.XPos);
            ToPointGripper(target, gripper);
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            WaitTillEndGripper(target, gripper);
            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromLeftToConveyor(StepperMotor gripper, TargetPosition target, TargetPosition currentPosition)
        {
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            ToPointGripper(target, gripper);
            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);
            Motion.ToPointX(target.XPos);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromConveyorToRightTop(StepperMotor gripper, TargetPosition target)
        {
            Motion.ToPointX(target.XPos);
            ToPointGripper(target, gripper);
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            WaitTillEndGripper(target, gripper);
            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromConveyorToRightBottom(StepperMotor gripper, TargetPosition target)
        {
            Motion.ToPointX(target.XPos);
            Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            ToPointGripper(target, gripper);

            Motion.WaitTillXBiggerThan(Motion.ConveyorRightPosition.XPos);
            Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromConveyorToLeftTop(StepperMotor gripper, TargetPosition target)
        {
            Motion.ToPointX(target.XPos);
            ToPointGripper(target, gripper);
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            WaitTillEndGripper(target, gripper);
            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromConveyorToLeftBottom(StepperMotor gripper, TargetPosition target)
        {
            Motion.ToPointX(target.XPos);
            Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            ToPointGripper(target, gripper);

            Motion.WaitTillXSmallerThan(Motion.ConveyorLeftPosition.XPos);
            Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

        private void MoveFromConveyorToConveyor(StepperMotor gripper, TargetPosition target)
        {
            Motion.ToPointX(target.XPos);
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            ToPointGripper(target, gripper);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            WaitTillEndGripper(target, gripper);

            MotorYOutThenMotorZDown(target);
        }

    }
}
