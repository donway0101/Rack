using System;
using System.Diagnostics;
using System.Threading;

namespace Rack
{
    public partial class CqcRack
    {
        private void HomeGrippers()
        {
            if (_gripperIsOnline)
            {
                Steppers.HomeMotor(StepperMotor.One, -6);
                Steppers.HomeMotor(StepperMotor.Two, -2);
            }
        }

        public void CloseGripper(StepperMotor gripper, int timeout=1000)
        {
            EcatIo.SetOutput(gripper == StepperMotor.One ? Output.GripperOne : Output.GripperTwo, false);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Input sensor = gripper == StepperMotor.One ? Input.Gripper01Tight : Input.Gripper02Tight;
            while (!EcatIo.GetInput(sensor))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }
        }

        public void OpenGripper(StepperMotor gripper, int timeout= 1000)
        {
            EcatIo.SetOutput(gripper == StepperMotor.One ? Output.GripperOne : Output.GripperTwo, true);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Input sensor = gripper == StepperMotor.One ? Input.Gripper01Loose : Input.Gripper02Loose;
            while (!EcatIo.GetInput(sensor))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }
        }

        //Todo add offset to gripper one and gripper two.
        private void ToPointWaitTillEndGripper(TargetPosition target, StepperMotor gripper)
        {
            if (gripper == StepperMotor.One)
            {
                Motion.ToPoint(Motion.MotorR, target.RPos);
                Steppers.ToPoint(StepperMotor.One, target.APos);
                Steppers.ToPoint(StepperMotor.Two, 0);
                Motion.WaitTillEnd(Motion.MotorR);
                Steppers.WaitTillEnd(StepperMotor.One, target.APos);
                Steppers.WaitTillEnd(StepperMotor.Two, 0);
            }
            else
            {
                Motion.ToPoint(Motion.MotorR, target.RPos - 60);
                Steppers.ToPoint(StepperMotor.Two, target.APos);
                Steppers.ToPoint(StepperMotor.One, 0);
                Motion.WaitTillEnd(Motion.MotorR);
                Steppers.WaitTillEnd(StepperMotor.One, 0);
                Steppers.WaitTillEnd(StepperMotor.Two, target.APos);
            }
        }

        private void WaitTillEndGripper(TargetPosition target, StepperMotor gripper)
        {
            Motion.WaitTillEnd(Motion.MotorR);
            if (gripper == StepperMotor.One)
            {
                Steppers.WaitTillEnd(StepperMotor.One, target.APos);
                Steppers.WaitTillEnd(StepperMotor.Two, 0);
            }
            else
            {
                Steppers.WaitTillEnd(StepperMotor.One, 0);
                Steppers.WaitTillEnd(StepperMotor.Two, target.APos);
            }
        }

        private void ToPointGripper(TargetPosition target, StepperMotor gripper)
        {
            if (gripper == StepperMotor.One)
            {
                Motion.ToPoint(Motion.MotorR, target.RPos);
                Steppers.ToPoint(StepperMotor.One, target.APos);
                Steppers.ToPoint(StepperMotor.Two, 0);
            }
            else
            {
                Motion.ToPoint(Motion.MotorR, target.RPos - 60);
                Steppers.ToPoint(StepperMotor.Two, target.APos);
                Steppers.ToPoint(StepperMotor.One, 0);
            }
        }
       
    }
}
