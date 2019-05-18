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
                    Steppers.HomeMotor(RackGripper.One, -6.0);
                    Steppers.HomeMotor(RackGripper.Two, -2.0);
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
            while (!EcatIo.GetInput(sensor))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
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
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }
        }

        private void ToPointWaitTillEndGripper(TargetPosition target, RackGripper gripper)
        {
            if (gripper == RackGripper.One)
            {
                Motion.ToPoint(Motion.MotorR, target.RPos);
                Steppers.ToPoint(RackGripper.One, target.APos);
                Steppers.ToPoint(RackGripper.Two, 0);
                Motion.WaitTillEnd(Motion.MotorR);
                Steppers.WaitTillEnd(RackGripper.One, target.APos);
                Steppers.WaitTillEnd(RackGripper.Two, 0);
            }
            else
            {
                Motion.ToPoint(Motion.MotorR, target.RPos - 60);
                Steppers.ToPoint(RackGripper.Two, target.APos);
                Steppers.ToPoint(RackGripper.One, 0);
                Motion.WaitTillEnd(Motion.MotorR);
                Steppers.WaitTillEnd(RackGripper.One, 0);
                Steppers.WaitTillEnd(RackGripper.Two, target.APos);
            }
        }

        private void WaitTillEndGripper(TargetPosition target, RackGripper gripper, bool phoneConveyorVertical = false)
        {
            if (gripper == RackGripper.One)
            {
                Steppers.WaitTillEnd(RackGripper.One, target.APos);
                if (phoneConveyorVertical)
                {
                    Steppers.WaitTillEnd(RackGripper.Two, target.APos);
                }
                else
                {
                    Steppers.WaitTillEnd(RackGripper.Two, 0);
                }
            }
            else
            {
                if (phoneConveyorVertical)
                {
                    Steppers.WaitTillEnd(RackGripper.One, target.APos);
                }
                else
                {
                    Steppers.WaitTillEnd(RackGripper.One, 0);
                }
                Steppers.WaitTillEnd(RackGripper.Two, target.APos);
            }
        }

        private void ToPointGripper(TargetPosition target, RackGripper gripper, bool phoneConveyorVertical = false)
        {
            if (gripper == RackGripper.One)
            {
                Steppers.ToPoint(RackGripper.One, target.APos);
                if (phoneConveyorVertical)
                {
                    Steppers.ToPoint(RackGripper.Two, target.APos);
                }
                else
                {
                    Steppers.ToPoint(RackGripper.Two, 0);
                }
                
            }
            else
            {
                if (phoneConveyorVertical)
                {
                    Steppers.ToPoint(RackGripper.One, target.APos);
                }
                else
                {
                    Steppers.ToPoint(RackGripper.One, 0);
                }
                Steppers.ToPoint(RackGripper.Two, target.APos);
            }
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
