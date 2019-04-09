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
                Gripper.ToPoint(GripperStepper.Gripper.One, target.APos);
                Gripper.ToPoint(GripperStepper.Gripper.Two, 0);
                Motion.WaitTillEnd(Motion.MotorR);
                Gripper.WaitTillEnd(GripperStepper.Gripper.One, target.APos);
                Gripper.WaitTillEnd(GripperStepper.Gripper.Two, 0);
            }
            else
            {
                Motion.ToPoint(Motion.MotorR, target.RPos - 60);
                Gripper.ToPoint(GripperStepper.Gripper.Two, target.APos);
                Gripper.ToPoint(GripperStepper.Gripper.One, 0);
                Motion.WaitTillEnd(Motion.MotorR);
                Gripper.WaitTillEnd(GripperStepper.Gripper.One, 0);
                Gripper.WaitTillEnd(GripperStepper.Gripper.Two, target.APos);
            }
        }

        private void WaitTillEndGripper(TargetPosition target, Gripper gripper)
        {
            Motion.WaitTillEnd(Motion.MotorR);
            if (gripper == GripperStepper.Gripper.One)
            {
                Gripper.WaitTillEnd(GripperStepper.Gripper.One, target.APos);
                Gripper.WaitTillEnd(GripperStepper.Gripper.Two, 0);
            }
            else
            {
                Gripper.WaitTillEnd(GripperStepper.Gripper.One, 0);
                Gripper.WaitTillEnd(GripperStepper.Gripper.Two, target.APos);
            }
        }

        private void ToPointGripper(TargetPosition target, Gripper gripper)
        {
            if (gripper == GripperStepper.Gripper.One)
            {
                Motion.ToPoint(Motion.MotorR, target.RPos);
                Gripper.ToPoint(GripperStepper.Gripper.One, target.APos);
                Gripper.ToPoint(GripperStepper.Gripper.Two, 0);
            }
            else
            {
                Motion.ToPoint(Motion.MotorR, target.RPos - 60);
                Gripper.ToPoint(GripperStepper.Gripper.Two, target.APos);
                Gripper.ToPoint(GripperStepper.Gripper.One, 0);
            }
        }


        private void Timeout()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (true)
            {                
                if (sw.ElapsedMilliseconds > 5000) throw new TimeoutException();
            }
        }

        public void ReadyThePhone(int timeout=3000)
        {
            Io.SetOutput(Output.ClampPick, true);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!Io.GetInput(Input.ClampTightPick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            Io.SetOutput(Output.SideBlockPick, true);
            sw.Restart();
            while (Io.GetInput(Input.SideBlockPick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            Thread.Sleep(500);

            Io.SetOutput(Output.SideBlockPick, false);
            sw.Restart();
            while (!Io.GetInput(Input.SideBlockPick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            Io.SetOutput(Output.ClampPick, false);
            sw = new Stopwatch();
            sw.Restart();
            while (!Io.GetInput(Input.ClampLoosePick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            Conveyor.ReadyForPicking = true;
        }
    }
}
