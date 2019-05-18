using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rack
{
    public partial class CqcRack
    {
        private void MoveToTargetPosition(RackGripper gripper, TargetPosition target, bool phoneSlipIn)
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
                    MoveFromRightToRight(gripper, target, phoneSlipIn);
                }
                else
                {
                    if (target.XPos < Motion.ConveyorLeftPosition.XPos) //To left.
                    {
                        if (target.ZPos > Motion.PickPosition.ApproachHeight) //To left top.
                        {
                            MoveFromRightToLeftTop(gripper, target, currentPosition, phoneSlipIn);
                        }
                        else //To left bottom.
                        {
                            MoveFromRightToLeftBottom(gripper, target, phoneSlipIn);
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
                            MoveFromLeftToRightBottom(gripper, target, phoneSlipIn);
                        }
                        else //To right top.
                        {
                            MoveFromLeftToRightTop(gripper, target, currentPosition, phoneSlipIn);
                        }
                    }
                    else
                    {
                        if (target.XPos < Motion.ConveyorLeftPosition.XPos)//To left.
                        {
                            MoveFromLeftToLeft(gripper, target, phoneSlipIn);
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
                            MoveFromConveyorToRightBottom(gripper, target, phoneSlipIn);
                        }
                        else //To right top.
                        {
                            MoveFromConveyorToRightTop(gripper, target, phoneSlipIn);
                        }
                    }
                    else
                    {
                        if (target.XPos < Motion.ConveyorLeftPosition.XPos) //To left.
                        {
                            if (target.ZPos < Motion.PickPosition.ApproachHeight)// To left bottom.
                            {
                                MoveFromConveyorToLeftBottom(gripper, target, phoneSlipIn);
                            }
                            else //To left top.
                            {
                                MoveFromConveyorToLeftTop(gripper, target, phoneSlipIn);
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
            WaitTillEndGripper(target, gripper);
            Motion.WaitTillEnd(Motion.MotorY);

            Motion.BreakToPointWaitTillEnd(Motion.MotorZ, target.ZPos);
        }

        private void MoveFromRightToLeftBottom(RackGripper gripper, TargetPosition target, bool phoneSlipIn)
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

            MotorYOutThenMotorZDown(target, gripper, phoneSlipIn);
        }

        private void MoveFromRightToLeftTop(RackGripper gripper, TargetPosition target, 
            TargetPosition currentPosition, bool phoneSlipIn)
        {
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.ToPointX(Motion.ConveyorRightPosition.XPos);
            ToPointR(target, gripper);

            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);
            Motion.BreakToPointX(target.XPos);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);

            MotorYOutThenMotorZDown(target, gripper, phoneSlipIn);
        }

        private void MoveFromRightToRight(RackGripper gripper, TargetPosition target, bool phoneSlipIn)
        {
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.ToPointX(target.XPos);
            ToPointR(target, gripper);
            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);
            MotorYOutThenMotorZDown(target, gripper, phoneSlipIn);
        }

        private void MotorYOutThenMotorZDown(TargetPosition target, RackGripper gripper, bool phoneSlipIn)
        {
            Motion.ToPoint(Motion.MotorY, target.YPos);
            ToPointGripper(target, gripper);
            WaitTillEndGripper(target, gripper);
            Motion.WaitTillEnd(Motion.MotorY);   
            
            if (phoneSlipIn)
            {
                Task.Run(() => {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (stopwatch.ElapsedMilliseconds < 5000)
                    {
                        if (Motion.GetPosition(Motion.MotorZ) < target.ZPos + SlipInHeight)
                        {
                            try
                            {
                                OpenGripper(gripper);
                            }
                            catch (Exception)
                            {
                                break;
                            }
                        }
                        Delay(5);
                    }
                });
            }

            MoveToPointTillEnd(Motion.MotorZ, target.ZPos);
        }

        private void MoveFromLeftToRightTop(RackGripper gripper, TargetPosition target, 
            TargetPosition currentPosition, bool phoneSlipIn)
        {
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            ToPointR(target, gripper);
            Motion.WaitTillZBiggerThan(Motion.PickPosition.ApproachHeight - 20);
            Motion.ToPointX(target.XPos);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);

            MotorYOutThenMotorZDown(target, gripper, phoneSlipIn);
        }

        private void MoveFromLeftToRightBottom(RackGripper gripper, TargetPosition target, bool phoneSlipIn)
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

            MotorYOutThenMotorZDown(target, gripper, phoneSlipIn);
        }

        private void MoveFromLeftToLeft(RackGripper gripper, TargetPosition target, bool phoneSlipIn)
        {
            Motion.ToPointX(target.XPos);
            ToPointR(target, gripper);
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);
            MotorYOutThenMotorZDown(target, gripper, phoneSlipIn);
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

        private void MoveFromConveyorToRightTop(RackGripper gripper, TargetPosition target, bool phoneSlipIn)
        {
            Motion.ToPointX(target.XPos);
            ToPointR(target, gripper);
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);
            MotorYOutThenMotorZDown(target, gripper, phoneSlipIn);
        }

        private void MoveFromConveyorToRightBottom(RackGripper gripper, TargetPosition target, bool phoneSlipIn)
        {
            Motion.ToPointX(target.XPos);
            Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            ToPointR(target, gripper);

            Motion.WaitTillXBiggerThan(Motion.ConveyorRightPosition.XPos);
            Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);

            MotorYOutThenMotorZDown(target, gripper, phoneSlipIn);
        }

        private void MoveFromConveyorToLeftTop(RackGripper gripper, TargetPosition target, bool phoneSlipIn)
        {
            Motion.ToPointX(target.XPos);
            ToPointR(target, gripper);
            Motion.ToPoint(Motion.MotorZ, target.ApproachHeight);
            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);
            MotorYOutThenMotorZDown(target, gripper, phoneSlipIn);
        }

        private void MoveFromConveyorToLeftBottom(RackGripper gripper, TargetPosition target, bool phoneSlipIn)
        {
            Motion.ToPointX(target.XPos);
            Motion.ToPoint(Motion.MotorZ, Motion.PickPosition.ApproachHeight);
            ToPointR(target, gripper);

            Motion.WaitTillXSmallerThan(Motion.ConveyorLeftPosition.XPos);
            Motion.BreakToPoint(Motion.MotorZ, target.ApproachHeight);

            Motion.WaitTillEndX();
            Motion.WaitTillEnd(Motion.MotorZ);
            Motion.WaitTillEnd(Motion.MotorR);

            MotorYOutThenMotorZDown(target, gripper, phoneSlipIn);
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
