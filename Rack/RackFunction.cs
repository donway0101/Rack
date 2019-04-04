using EcatIo;
using GripperStepper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Input = EcatIo.Input;


namespace Rack
{
    public partial class CqcRack
    {
        public void CloseGripper(Gripper gripper, int timeout=1000)
        {
            _io.SetOutput(gripper == Gripper.One ? Output.GripperOne : Output.GripperTwo, false);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Input sensor = gripper == Gripper.One ? Input.Gripper01Tight : Input.Gripper02Tight;
            while (!_io.GetInput(sensor))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }
        }

        public void OpenGripper(Gripper gripper, int timeout= 1000)
        {
            _io.SetOutput(gripper == Gripper.One ? Output.GripperOne : Output.GripperTwo, true);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Input sensor = gripper == Gripper.One ? Input.Gripper01Loose : Input.Gripper02Loose;
            while (!_io.GetInput(sensor))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
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
            _io.SetOutput(Output.ClampPick, true);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!_io.GetInput(Input.ClampTightPick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            _io.SetOutput(Output.SideBlockPick, true);
            sw.Restart();
            while (_io.GetInput(Input.SideBlockPick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            Thread.Sleep(500);

            _io.SetOutput(Output.SideBlockPick, false);
            sw.Restart();
            while (!_io.GetInput(Input.SideBlockPick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            _io.SetOutput(Output.ClampPick, false);
            sw = new Stopwatch();
            sw.Restart();
            while (!_io.GetInput(Input.ClampLoosePick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }
        }
    }
}
