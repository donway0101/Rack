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
        public void CloseGripper(Gripper gripper, int timeout=1000)
        {
            Io.SetOutput(gripper == GripperStepper.Gripper.One ? Output.GripperOne : Output.GripperTwo, false);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Input sensor = gripper == GripperStepper.Gripper.One ? Input.Gripper01Tight : Input.Gripper02Tight;
            while (!Io.GetInput(sensor))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }
        }

        public void OpenGripper(Gripper gripper, int timeout= 1000)
        {
            Io.SetOutput(gripper == GripperStepper.Gripper.One ? Output.GripperOne : Output.GripperTwo, true);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Input sensor = gripper == GripperStepper.Gripper.One ? Input.Gripper01Loose : Input.Gripper02Loose;
            while (!Io.GetInput(sensor))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }
        }

        //Todo add offset to gripper one and gripper two.
        private void ToPointWaitTillEndGripper(TargetPosition target, Gripper gripper)
        {
            if (gripper == GripperStepper.Gripper.One)
            {
                Motion.ToPoint(Motion.MotorR, target.RPos);
                Stepper.ToPoint(GripperStepper.Gripper.One, target.APos);
                Stepper.ToPoint(GripperStepper.Gripper.Two, 0);
                Motion.WaitTillEnd(Motion.MotorR);
                Stepper.WaitTillEnd(GripperStepper.Gripper.One, target.APos);
                Stepper.WaitTillEnd(GripperStepper.Gripper.Two, 0);
            }
            else
            {
                Motion.ToPoint(Motion.MotorR, target.RPos - 60);
                Stepper.ToPoint(GripperStepper.Gripper.Two, target.APos);
                Stepper.ToPoint(GripperStepper.Gripper.One, 0);
                Motion.WaitTillEnd(Motion.MotorR);
                Stepper.WaitTillEnd(GripperStepper.Gripper.One, 0);
                Stepper.WaitTillEnd(GripperStepper.Gripper.Two, target.APos);
            }
        }

        private void WaitTillEndGripper(TargetPosition target, Gripper gripper)
        {
            Motion.WaitTillEnd(Motion.MotorR);
            if (gripper == GripperStepper.Gripper.One)
            {
                Stepper.WaitTillEnd(GripperStepper.Gripper.One, target.APos);
                Stepper.WaitTillEnd(GripperStepper.Gripper.Two, 0);
            }
            else
            {
                Stepper.WaitTillEnd(GripperStepper.Gripper.One, 0);
                Stepper.WaitTillEnd(GripperStepper.Gripper.Two, target.APos);
            }
        }

        private void ToPointGripper(TargetPosition target, Gripper gripper)
        {
            if (gripper == GripperStepper.Gripper.One)
            {
                Motion.ToPoint(Motion.MotorR, target.RPos);
                Stepper.ToPoint(GripperStepper.Gripper.One, target.APos);
                Stepper.ToPoint(GripperStepper.Gripper.Two, 0);
            }
            else
            {
                Motion.ToPoint(Motion.MotorR, target.RPos - 60);
                Stepper.ToPoint(GripperStepper.Gripper.Two, target.APos);
                Stepper.ToPoint(GripperStepper.Gripper.One, 0);
            }
        }
       
    }
}
