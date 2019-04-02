using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IO;

namespace Conveyor
{
    public class PickAndPlaceConveyor
    {
        private EthercatIO IO;
        private Thread ConveyorWorkingThread;

        #region Error occured Event
        public delegate void ErrorOccuredEventHandler(object sender, string description);

        public event ErrorOccuredEventHandler ErrorOccured;

        protected void OnErrorOccured(string description)
        {
            ErrorOccured?.Invoke(this, description);
        }
        #endregion

        /// <summary>
        /// Release clamp for picking.
        /// After Picking, reset it by host.
        /// </summary>
        public bool ReadyForPicking { get; set; }

        /// <summary>
        /// Phone already under the sensor, tightened.
        /// After Picking, reset it by host.
        /// </summary>
        public bool InposForPicking { get; set; }

        public bool CommandInposForPicking { get; set; }

        public bool CommandReadyForPicking { get; set; }

        public PickAndPlaceConveyor(EthercatIO io)
        {
            IO = io;
        }

        public void SetCylinder(Output output, Input input, bool inputValue, int timeout=1000)
        {
            IO.SetOutput(output, true);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (IO.GetInput(input) != inputValue)
            {
                if (stopwatch.ElapsedMilliseconds>timeout)
                {
                    throw new Exception("Set" + output + " timeout");
                }
                Thread.Sleep(10);
            }  
        }

        public void SetCylinder(Output output, Input input, int timeout = 1000)
        {
            SetCylinder(output, input, true, timeout);
        }

        public void ResetCylinder(Output output, Input input, bool inputValue, int timeout = 1000)
        {
            IO.SetOutput(output, false);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (IO.GetInput(input) != inputValue)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    throw new Exception("Set" + output + " timeout");
                }
                Thread.Sleep(10);
            }
        }

        public void ResetCylinder(Output output, Input input, int timeout = 1000)
        {
            ResetCylinder(output, input, true, timeout);
        }

        public void WaitTill(Input input, bool value, int timeout=60000)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (IO.GetInput(input) != value)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    throw new Exception("Wait" + input + " timeout");
                }
                Thread.Sleep(10);
            }
        }

        public void Delay(int millisecond)
        {
            Thread.Sleep(millisecond);
        }

        public async Task SetCylinderAsync(Output output, Input input)
        {
            await Task.Run(() => {
                SetCylinder(output, input);
            });
        }

        public async Task ResetCylinderAsync(Output output, Input input)
        {
            await Task.Run(() => {
                ResetCylinder(output, input);
            });
        }

        public void Run()
        {
            //Stop();
            
            ConveyorWorkingThread = new Thread(DoWork);
            ConveyorWorkingThread.IsBackground = true;
            ConveyorWorkingThread.Start();
        }

        private void DoWork()
        {
            ReadyForPicking = false;
            InposForPicking = false;
            while (true)
            {
                try
                {
                    if (CommandReadyForPicking == true & InposForPicking == true & ReadyForPicking == false)
                    {
                        CommandReadyForPicking = false;

                        //IO.SetOutput(Output.Belt, false);
                        //Delay(200);
                        //SetCylinder(Output.Push, Input.PushIn, false);
                        //Delay(500);
                        //ResetCylinder(Output.Push, Input.PushIn, true);
                        //SetCylinder(Output.BlockPick, Input.BlockPickUp, false);
                        //ResetCylinder(Output.ClampPick, Input.ClampLoose);

                        ReadyForPicking = true;
                    }

                    if (CommandInposForPicking == true & InposForPicking == false)
                    {
                        CommandInposForPicking = false;

                        //ResetCylinder(Output.BlockPick, Input.BlockPickUp, true);
                        //IO.SetOutput(Output.Belt, true);
                        //WaitTill(Input.PhoneInPick, true, 120000);
                        //Delay(1000);
                        //SetCylinder(Output.ClampPick, Input.ClampTight);

                        InposForPicking = true;
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccured(ex.Message);
                    break;
                }
            }
        }

        public void Stop()
        {
            if (ConveyorWorkingThread != null)
            {
                ConveyorWorkingThread.Abort();
                ConveyorWorkingThread.Join();
            }
        }
    }
}
