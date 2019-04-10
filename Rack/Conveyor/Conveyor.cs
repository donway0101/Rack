using System;
using System.Diagnostics;
using System.Threading;

namespace Rack
{
    public class Conveyor
    {
        private readonly EthercatIo _io;
        private Thread _conveyorWorkingThread;

        public bool ConveyorMovingForward { get; set; } = true;

        /// <summary>
        ///     Release clamp for picking.
        ///     After Picking, reset it by host.
        /// </summary>
        public bool ReadyForPicking { get; set; }
        public bool PickBeltOkToRun { get; set; }

        /// <summary>
        ///     Phone already under the sensor, tightened.
        ///     After Picking, reset it by host.
        /// </summary>
        public bool InposForPicking { get; set; }

        public bool CommandInposForPicking { get; set; }

        public bool CommandReadyForPicking { get; set; }

        #region Events

        public delegate void ErrorOccuredEventHandler(object sender, string description);

        public event ErrorOccuredEventHandler ErrorOccured;

        protected void OnErrorOccured(string description)
        {
            ErrorOccured?.Invoke(this, description);
        }

        public delegate void PhoneReadyForPickingEventHandler(object sender, string description);

        public event PhoneReadyForPickingEventHandler PhoneReadyForPicking;

        protected void OnPhoneReadyForPicking(string description)
        {
            PhoneReadyForPicking?.Invoke(this, description);
        }

        public delegate void PickBufferHasPhoneEventHandler(object sender, string description);

        public event PickBufferHasPhoneEventHandler PickBufferHasPhone;

        protected void OnPickBufferHasPhone(string description)
        {
            PickBufferHasPhone?.Invoke(this, description);
        }

        #endregion

        public Conveyor(EthercatIo ethercatIo)
        {
            _io = ethercatIo;
        }

        public void SetCylinder(Output output, Input input, bool sensorState = true, int timeout = 1000)
        {
            _io.SetOutput(output, true);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (_io.GetInput(input) != sensorState)
            {
                if (stopwatch.ElapsedMilliseconds > timeout) throw new Exception("Set" + output + " timeout");
                Thread.Sleep(10);
            }
        }

        public void ResetCylinder(Output output, Input input, bool sensorState = true, int timeout = 1000)
        {
            _io.SetOutput(output, false);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (_io.GetInput(input) != sensorState)
            {
                if (stopwatch.ElapsedMilliseconds > timeout) throw new Exception("Set" + output + " timeout");
                Delay(10);
            }
        }

        public void UpBlockSeparate(bool state)
        {
            if (state)
            {
                if (ConveyorMovingForward)
                    ResetCylinder(Output.UpBlockSeparateForward, Input.UpBlockSeparateForward);
                else
                    ResetCylinder(Output.UpBlockSeparateBackward, Input.UpBlockSeparateBackward);
            }
            else
            {
                if (ConveyorMovingForward)
                    SetCylinder(Output.UpBlockSeparateForward, Input.UpBlockSeparateForward, false);
                else
                    SetCylinder(Output.UpBlockSeparateBackward, Input.UpBlockSeparateBackward, false);
            }
        }

        public void UpBlockPick(bool state)
        {
            if (state)
            {
                if (ConveyorMovingForward)
                    ResetCylinder(Output.UpBlockPickForward, Input.UpBlockPickForward);
                else
                    ResetCylinder(Output.UpBlockPickBackward, Input.UpBlockPickBackward);
            }
            else
            {
                if (ConveyorMovingForward)
                    SetCylinder(Output.UpBlockPickForward, Input.UpBlockPickForward, false);
                else
                    SetCylinder(Output.UpBlockPickBackward, Input.UpBlockPickBackward, false);
            }
        }

        public void SideBlockSeparate(bool state)
        {
            if (!state)
            {
                if (ConveyorMovingForward)
                    ResetCylinder(Output.SideBlockSeparateForward, Input.SideBlockSeparateForward);
                else
                    ResetCylinder(Output.SideBlockSeparateBackward, Input.SideBlockSeparateBackward);
            }
            else
            {
                if (ConveyorMovingForward)
                    SetCylinder(Output.SideBlockSeparateForward, Input.SideBlockSeparateForward, false);
                else
                    SetCylinder(Output.SideBlockSeparateBackward, Input.SideBlockSeparateBackward, false);
            }
        }

        public void RunBeltPick(bool state)
        {
            _io.SetOutput(Output.BeltPick, state);
        }

        public void PushPickInpos(bool state)
        {
            if (state)
                SetCylinder(Output.SideBlockPick, Input.SideBlockPick, false);
            else
                ResetCylinder(Output.SideBlockPick, Input.SideBlockPick);
        }

        public void Clamp(bool state)
        {
            if (state)
                SetCylinder(Output.ClampPick, Input.ClampTightPick);
            else
                ResetCylinder(Output.ClampPick, Input.ClampLoosePick);
        }

        public void WaitTill(Input input, bool state, int timeout = 60000)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (_io.GetInput(input) != state)
            {
                if (stopwatch.ElapsedMilliseconds > timeout) throw new Exception("Wait" + input + " timeout");
                Delay(10);
            }
        }

        public void Delay(int millisecond)
        {
            Thread.Sleep(millisecond);
        }

        public void InitialState()
        {
            Clamp(false);
            UpBlockSeparate(true);
            UpBlockPick(true);
            SideBlockSeparate(false);
            ConveyorMovingForward = !ConveyorMovingForward;
            UpBlockSeparate(false);
            UpBlockPick(false);
            SideBlockSeparate(false);
            ConveyorMovingForward = !ConveyorMovingForward;
        }

        public void Start()
        {
            InitialState();

            if (_conveyorWorkingThread == null)
                _conveyorWorkingThread = new Thread(DoWork)
                {
                    IsBackground = true
                };

            if (_conveyorWorkingThread.IsAlive == false) _conveyorWorkingThread.Start();
        }

        private void DoWork()
        {
            ReadyForPicking = false;
            InposForPicking = false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
                try
                {
                    if (CommandReadyForPicking)
                    {
                        ReadyForPicking = false;
                        PickBeltOkToRun = false;
                        if (InposForPicking)
                        {
                            RunBeltPick(false);
                            Delay(200);
                            PushPickInpos(true);
                            Delay(200);
                            PushPickInpos(false);
                            Clamp(false);

                            CommandReadyForPicking = false;
                            ReadyForPicking = true;
                            OnPhoneReadyForPicking("");
                        }
                        else
                        {
                            CommandInposForPicking = true;
                        }
                    }

                    //Todo combine other condition.
                    if (PickBeltOkToRun)
                    {
                        RunBeltPick(true);
                    }

                    if (CommandInposForPicking & (InposForPicking == false))
                    {
                        CommandInposForPicking = false;
                        ReadyForPicking = false;
                        
                        if (_io.GetInput(Input.PickHasPhone) == false)
                        {
                            WaitTill(
                                ConveyorMovingForward
                                    ? Input.PickBufferHasPhoneForward
                                    : Input.PickBufferHasPhoneBackward, true, 30000);
                            UpBlockPick(true);
                        }

                        SideBlockSeparate(true);
                        UpBlockSeparate(false);

                        WaitTill(Input.PickHasPhone, true, 5000);
                        Delay(1000);
                        Clamp(true);

                        InposForPicking = true;
                        OnPickBufferHasPhone("");

                        UpBlockSeparate(true);
                        SideBlockSeparate(false);
                        UpBlockPick(false);
                    }

                    Delay(10);
                }
                catch (Exception ex)
                {
                    OnErrorOccured(ex.Message);
                }
        }

        public void Stop()
        {
            if (_conveyorWorkingThread != null)
            {
                _conveyorWorkingThread.Abort();
                _conveyorWorkingThread.Join();
            }
        }


    }
}