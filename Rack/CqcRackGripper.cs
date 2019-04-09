using EcatIo;
using GripperStepper;
using System;
using System.Diagnostics;
using System.Threading;
using Motion;

namespace Rack
{
    public partial class CqcRack
    {
        public void CloseGripper(StepperMotor gripper, int timeout=1000)
        {
            Io.SetOutput(gripper == StepperMotor.One ? Output.GripperOne : Output.GripperTwo, false);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Input sensor = gripper == StepperMotor.One ? Input.Gripper01Tight : Input.Gripper02Tight;
            while (!Io.GetInput(sensor))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }
        }

        public void OpenGripper(StepperMotor gripper, int timeout= 1000)
        {
            Io.SetOutput(gripper == StepperMotor.One ? Output.GripperOne : Output.GripperTwo, true);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Input sensor = gripper == StepperMotor.One ? Input.Gripper01Loose : Input.Gripper02Loose;
            while (!Io.GetInput(sensor))
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
                Stepper.ToPoint(StepperMotor.One, target.APos);
                Stepper.ToPoint(StepperMotor.Two, 0);
                Motion.WaitTillEnd(Motion.MotorR);
                Stepper.WaitTillEnd(StepperMotor.One, target.APos);
                Stepper.WaitTillEnd(StepperMotor.Two, 0);
            }
            else
            {
                Motion.ToPoint(Motion.MotorR, target.RPos - 60);
                Stepper.ToPoint(StepperMotor.Two, target.APos);
                Stepper.ToPoint(StepperMotor.One, 0);
                Motion.WaitTillEnd(Motion.MotorR);
                Stepper.WaitTillEnd(StepperMotor.One, 0);
                Stepper.WaitTillEnd(StepperMotor.Two, target.APos);
            }
        }

        private void WaitTillEndGripper(TargetPosition target, StepperMotor gripper)
        {
            Motion.WaitTillEnd(Motion.MotorR);
            if (gripper == StepperMotor.One)
            {
                Stepper.WaitTillEnd(StepperMotor.One, target.APos);
                Stepper.WaitTillEnd(StepperMotor.Two, 0);
            }
            else
            {
                Stepper.WaitTillEnd(StepperMotor.One, 0);
                Stepper.WaitTillEnd(StepperMotor.Two, target.APos);
            }
        }

        private void ToPointGripper(TargetPosition target, StepperMotor gripper)
        {
            if (gripper == StepperMotor.One)
            {
                Motion.ToPoint(Motion.MotorR, target.RPos);
                Stepper.ToPoint(StepperMotor.One, target.APos);
                Stepper.ToPoint(StepperMotor.Two, 0);
            }
            else
            {
                Motion.ToPoint(Motion.MotorR, target.RPos - 60);
                Stepper.ToPoint(StepperMotor.Two, target.APos);
                Stepper.ToPoint(StepperMotor.One, 0);
            }
        }
       
    }
}
