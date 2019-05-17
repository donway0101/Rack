using System;

namespace Rack
{
    public partial class CqcRack
    {
        private void MoveToTargetPosition(RackGripper gripper, TargetPosition target)
        {
            if(RobotInSimulateMode)
                return;

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

        private void MoveFromRightToConveyor(RackGripper gripper, TargetPosition target, TargetPosition currentPosition)
        {
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.ToPointX(Motion.ConveyorRightPosition.XPos);
            ToPointR(target, gripper);

            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);
            Motion.BreakToPointX(target.XPos);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorR);

            MotorYOutThenBreakMotorZDown(target, gripper);
        }

        private void MotorYOutThenBreakMotorZDown(TargetPosition target, RackGripper gripper)
        {
            Motion.ToPoint(Motion.MotorY, target.YPos);
            ToPointGripper(target, gripper);
            Motion.WaitTillEnd(Motion.MotorY);
            WaitTillEndGripper(target, gripper);

            Motion.BreakToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
        }

        private void MoveFromRightToLeftBottom(RackGripper gripper, TargetPosition target)
        {
            Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            Motion.ToPointX(Motion.ConveyorRightPosition.XPos);
            ToPointR(target, gripper);

            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);
            Motion.BreakToPointX(target.XPos);

            Motion.WaitTillXSmallerThan(Motion.ConveyorLeftPosition.XPos);
            Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);

            MotorYOutThenMotorZDown(target, gripper);
        }

        private void MoveFromRightToLeftTop(RackGripper gripper, TargetPosition target, TargetPosition currentPosition)
        {
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.ToPointX(Motion.ConveyorRightPosition.XPos);
            ToPointR(target, gripper);

            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);
            Motion.BreakToPointX(target.XPos);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);

            MotorYOutThenMotorZDown(target, gripper);
        }

        private void MoveFromRightToRight(RackGripper gripper, TargetPosition target)
        {
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.ToPointX(target.XPos);
            ToPointR(target, gripper);
            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);
            MotorYOutThenMotorZDown(target, gripper);
        }

        private void MotorYOutThenMotorZDown(TargetPosition target, RackGripper gripper)
        {
            Motion.ToPoint(Motion.MotorY, target.YPos);
            ToPointGripper(target, gripper);
            Motion.WaitTillEnd(Motion.MotorY);
            WaitTillEndGripper(target, gripper);
            MoveToPointTillEnd(Motion.MotorZ, target.ZPos);
        }

        private void MoveFromLeftToRightTop(RackGripper gripper, TargetPosition target, TargetPosition currentPosition)
        {
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            ToPointR(target, gripper);
            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);
            Motion.ToPointX(target.XPos);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);

            MotorYOutThenMotorZDown(target, gripper);
        }

        private void MoveFromLeftToRightBottom(RackGripper gripper, TargetPosition target)
        {
            Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);

            Motion.ToPointX(target.XPos);
            ToPointR(target, gripper);

            Motion.WaitTillXBiggerThan(Motion.ConveyorRightPosition.XPos);
            Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);

            MotorYOutThenMotorZDown(target, gripper);
        }

        private void MoveFromLeftToLeft(RackGripper gripper, TargetPosition target)
        {
            Motion.ToPointX(target.XPos);
            ToPointR(target, gripper);
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);
            MotorYOutThenMotorZDown(target, gripper);
        }

        private void MoveFromLeftToConveyor(RackGripper gripper, TargetPosition target, TargetPosition currentPosition)
        {
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            ToPointR(target, gripper);
            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);
            Motion.ToPointX(target.XPos);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorR);

            MotorYOutThenBreakMotorZDown(target, gripper);
        }

        private void MoveFromConveyorToRightTop(RackGripper gripper, TargetPosition target)
        {
            Motion.ToPointX(target.XPos);
            ToPointR(target, gripper);
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);
            MotorYOutThenMotorZDown(target, gripper);
        }

        private void MoveFromConveyorToRightBottom(RackGripper gripper, TargetPosition target)
        {
            Motion.ToPointX(target.XPos);
            Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            ToPointR(target, gripper);

            Motion.WaitTillXBiggerThan(Motion.ConveyorRightPosition.XPos);
            Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);

            MotorYOutThenMotorZDown(target, gripper);
        }

        private void MoveFromConveyorToLeftTop(RackGripper gripper, TargetPosition target)
        {
            Motion.ToPointX(target.XPos);
            ToPointR(target, gripper);
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);
            MotorYOutThenMotorZDown(target, gripper);
        }

        private void MoveFromConveyorToLeftBottom(RackGripper gripper, TargetPosition target)
        {
            Motion.ToPointX(target.XPos);
            Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            ToPointR(target, gripper);

            Motion.WaitTillXSmallerThan(Motion.ConveyorLeftPosition.XPos);
            Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);

            MotorYOutThenMotorZDown(target, gripper);
        }

        private void MoveFromConveyorToConveyor(RackGripper gripper, TargetPosition target)
        {
            Motion.ToPointX(target.XPos);
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            ToPointR(target, gripper);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorR);

            MotorYOutThenBreakMotorZDown(target, gripper);
        }

    }
}
