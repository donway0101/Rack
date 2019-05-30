using System;
using System.Diagnostics;
using System.Threading;

namespace Rack
{
    public partial class CqcRack
    {
        private void HomeGrippers()
        {
            if (StepperOnline)
            {
                try
                {
                    if (StepperHomeComplete)
                    {
                        Steppers.ToPoint(RackGripper.One, Motion.PickPosition.APos);
                        Steppers.ToPoint(RackGripper.Two, Motion.PickPosition.APos);
                        Steppers.WaitTillEnd(RackGripper.One, Motion.PickPosition.APos);
                        Steppers.WaitTillEnd(RackGripper.Two, Motion.PickPosition.APos);
                    }
                    else
                    {
                        StepperHomeComplete = false;
                        Steppers.HomeMotor(RackGripper.One, -6.0);
                        Steppers.HomeMotor(RackGripper.Two, -2.0);
                        StepperHomeComplete = true;
                    }                    
                }
                catch (Exception ex)
                {

                    throw new Exception("Error when homming stepper motors: " + ex.Message);
                }
            }
        }

        public void CloseGripper(RackGripper gripper, int timeout=1000)
        {
            EcatIo.SetOutput(gripper == RackGripper.One ? Output.GripperOne : Output.GripperTwo, false);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Input sensor = gripper == RackGripper.One ? Input.Gripper01Tight : Input.Gripper02Tight;
            int failCount = 0;
            while (!EcatIo.GetInput(sensor))
            {
                if (sw.ElapsedMilliseconds > timeout)
                {
                    throw new Exception("Close gripper " + gripper + " timeout");
                }                   
                Thread.Sleep(10);
                failCount++;
                if (failCount>20)
                {
                    EcatIo.SetOutput(gripper == RackGripper.One ? Output.GripperOne : Output.GripperTwo, false);
                    failCount = 0;
                }
            }
        }

        public void OpenGripper(RackGripper gripper, int timeout= 1000)
        {
            EcatIo.SetOutput(gripper == RackGripper.One ? Output.GripperOne : Output.GripperTwo, true);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Input sensor = gripper == RackGripper.One ? Input.Gripper01Loose : Input.Gripper02Loose;
            while (!EcatIo.GetInput(sensor))
            {
                if (sw.ElapsedMilliseconds > timeout)
                    throw new Exception("Open gripper timeout");
                Thread.Sleep(10);
            }
        }

        private void ToPointWaitTillEndGripper(TargetPosition target, RackGripper gripper)
        {
            if (gripper == RackGripper.One)
            {
                Motion.ToPoint(Motion.MotorR, target.RPos);
                Steppers.ToPoint(RackGripper.One, target.APos);
                Steppers.WaitTillEnd(RackGripper.One, target.APos);

                //Steppers.ToPoint(RackGripper.Two, 0);                
                //Steppers.WaitTillEnd(RackGripper.Two, 0);

                Motion.WaitTillEnd(Motion.MotorR);               
            }
            else
            {
                Motion.ToPoint(Motion.MotorR, target.RPos - 60);
                Steppers.ToPoint(RackGripper.Two, target.APos);
                Steppers.WaitTillEnd(RackGripper.Two, target.APos);

                //Steppers.ToPoint(RackGripper.One, 0);
                //Steppers.WaitTillEnd(RackGripper.One, 0);

                Motion.WaitTillEnd(Motion.MotorR);                
            }
        }

        private void WaitTillEndGripper(TargetPosition target, RackGripper gripper)
        {
            if (gripper == RackGripper.One)
            {
                Steppers.WaitTillEnd(RackGripper.One, target.APos);
                if (GripperIsAvailable(RackGripper.Two)==false)
                {
                    Steppers.WaitTillEnd(RackGripper.Two, target.APos);
                }
            }
            else
            {
                if (GripperIsAvailable(RackGripper.One) == false)
                {
                    Steppers.WaitTillEnd(RackGripper.One, target.APos);
                }
                Steppers.WaitTillEnd(RackGripper.Two, target.APos);
            }
        }

        private void ToPointGripper(TargetPosition target, RackGripper gripper)
        {
            Steppers.ToPoint(RackGripper.One, target.APos);
            Steppers.ToPoint(RackGripper.Two, target.APos);

            //if (gripper == RackGripper.One)
            //{
            //    Steppers.ToPoint(RackGripper.One, target.APos);
            //    if (GripperIsAvailable(RackGripper.Two) == false)
            //    {
            //        Steppers.ToPoint(RackGripper.Two, target.APos);
            //    }            
            //}
            //else
            //{
            //    if (GripperIsAvailable(RackGripper.One) == false)
            //    {
            //        Steppers.ToPoint(RackGripper.One, target.APos);
            //    }
            //    Steppers.ToPoint(RackGripper.Two, target.APos);
            //}
        }

        private void ToPointR(TargetPosition target, RackGripper gripper)
        {
            if (gripper == RackGripper.One)
            {
                Motion.ToPoint(Motion.MotorR, target.RPos);
            }
            else
            {
                Motion.ToPoint(Motion.MotorR, target.RPos - 60);
            }
        }

    }
}
