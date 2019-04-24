using System;
using System.Diagnostics;
using System.Threading;

namespace Rack
{
    public class Conveyor
    {
        private readonly EthercatIo _io;
        private Thread _conveyorMonitorThread;

        public bool PickBufferHasPhone { get; set; }

        public bool ConveyorMovingForward { get; set; } = true;

        /// <summary>
        /// Auto set false after a bin finished.
        /// </summary>
        public bool HasBinAPhone { get; set; }

        public bool HasPlaceAPhone { get; set; }

        #region Events

        public delegate void ErrorOccuredEventHandler(object sender, int code, string description);

        public event ErrorOccuredEventHandler ErrorOccured;

        protected void OnErrorOccured(int code, string description)
        {
            ErrorOccured?.Invoke(this, code, description);
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

        public void Clamp(bool state, int timeout=3000)
        {
            if (state)
                SetCylinder(Output.ClampPick, Input.ClampTightPick, true, timeout);
            else
                ResetCylinder(Output.ClampPick, Input.ClampLoosePick, true, timeout);
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

        public void ResetOutputs()
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
            ResetOutputs();

            if (_conveyorMonitorThread == null)
                _conveyorMonitorThread = new Thread(Monitor)
                {
                    IsBackground = true
                };

            if (_conveyorMonitorThread.IsAlive == false) _conveyorMonitorThread.Start();
        }

        public void ReadyForPicking()
        {
            RunBeltPick(false);
            Delay(200);
            PushPickInpos(true);
            Delay(200);
            PushPickInpos(false);
            Clamp(false);
        }

        public void InposForPicking()
        {
            RunBeltPick(true);
            if (PickPhoneSensor() == false)
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

            UpBlockSeparate(true);
            SideBlockSeparate(false);
            UpBlockPick(false);
        }

        private void Monitor()
        {
            int pickBufferSensorCount = 0;
            bool placedPhoneDetected = false;
            bool binedPhoneDetected = false;

            while (true)
            {
                try
                {
                    if (HasPlaceAPhone)
                    {
                        if (PlaceBufferPhoneSensor())
                        {
                            placedPhoneDetected = true;
                        }

                        if (placedPhoneDetected && PlaceBufferPhoneSensor() == false)
                        {
                            HasPlaceAPhone = false;
                            placedPhoneDetected = false;
                        }
                    }

                    if (HasBinAPhone)
                    {
                        if (_io.GetInput(Input.ConveyorBinIn))
                        {
                            binedPhoneDetected = true;
                        }

                        if (binedPhoneDetected && _io.GetInput(Input.ConveyorBinIn) == false)
                        {
                            HasBinAPhone = false;
                            binedPhoneDetected = false;
                        }
                    }

                    if (PickBufferPhoneSensor())
                    {
                        pickBufferSensorCount++;
                        if (pickBufferSensorCount > 10)
                        {
                            PickBufferHasPhone = true;
                            pickBufferSensorCount = 0;
                        }
                    }
                    else
                    {
                        pickBufferSensorCount++;
                        if (pickBufferSensorCount > 10)
                        {
                            PickBufferHasPhone = false;
                            pickBufferSensorCount = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccured(444, ex.Message);
                    Delay(5000);
                }

                Delay(100);
            }
        }

        private bool PickBufferPhoneSensor()
        {
            if (ConveyorMovingForward)
               return _io.GetInput(Input.PickBufferHasPhoneForward);
            else
               return _io.GetInput(Input.PickBufferHasPhoneBackward);
        }

        private bool PlaceBufferPhoneSensor()
        {
            if (ConveyorMovingForward)
                return _io.GetInput(Input.PickBufferHasPhoneBackward);
            else
                return _io.GetInput(Input.PickBufferHasPhoneForward);
        }

        public bool PickPhoneSensor()
        {
            return _io.GetInput(Input.PickHasPhone);
        }

        public void Stop()
        {
            if (_conveyorMonitorThread != null)
            {
                _conveyorMonitorThread.Abort();
                _conveyorMonitorThread.Join();
            }
        }
    }
}