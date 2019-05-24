using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Rack
{
    public class Conveyor
    {
        private readonly EthercatIo _io;
        private Thread _conveyorMonitorThread;
        private Thread _sensorMonitorThread;

        public bool PickBufferHasPhone { get; set; }

        public bool ConveyorMovingForward { get; set; } = true;

        /// <summary>
        /// Auto set false after a bin finished.
        /// </summary>
        public bool HasBinAPhone { get; set; }

        /// <summary>
        /// Auto set false after a place finished.
        /// </summary>
        public bool HasPlaceAPhone { get; set; }

        public bool NgFullWarning { get; set; }

        public int NgCount { get; set; }

        public int MaxNg { get; set; } = 10;

        public bool RobotBinning { get; set; }

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
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (_io.GetInput(input) != sensorState)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                    throw new Exception("SetCylinder " + output + " timeout");
                _io.SetOutput(output, true);
            }
        }

        public void ResetCylinder(Output output, Input input, bool sensorState = true, int timeout = 1000)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (_io.GetInput(input) != sensorState)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                    throw new Exception("ResetCylinder " + output + " timeout");
                _io.SetOutput(output, false);
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

        public void RunBeltPick()
        {
            _io.SetOutput(Output.BeltPick, true);
            //Todo 
            _io.SetOutput(Output.BeltConveyorOne, true);
        }

        public void StopBeltPick()
        {
            _io.SetOutput(Output.BeltPick, false);
            _io.SetOutput(Output.BeltConveyorOne, false);
        }

        public void RunBeltBin()
        {
            _io.SetOutput(Output.BeltBin, true);
        }

        public void StopBeltBin()
        {
            _io.SetOutput(Output.BeltBin, false);
        }

        public void RunBeltConveyorOne()
        {
            _io.SetOutput(Output.BeltConveyorOne, true);
        }

        public void StopBeltConveyorOne()
        {
            _io.SetOutput(Output.BeltConveyorOne, false);
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
            StopBeltPick();
            StopBeltBin();
            StopBeltConveyorOne();
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
            if (PickPhoneSensor())
            {
                throw new Exception("Clean phone up on the conveyor first.");
            }

            ResetOutputs();

            if (_conveyorMonitorThread == null)
                _conveyorMonitorThread = new Thread(Monitor)
                {
                    IsBackground = true
                };

            if (_conveyorMonitorThread.IsAlive == false)
                _conveyorMonitorThread.Start();

            if (_sensorMonitorThread == null)
                _sensorMonitorThread = new Thread(BufferPhoneSensorMonitor)
                {
                    IsBackground = true
                };

            if (_sensorMonitorThread.IsAlive == false)
                _sensorMonitorThread.Start();
        }

        public void ReadyForPicking()
        {
            StopBeltPick();
            UpBlockPick(false);
            Delay(200);
            PushPickInpos(true);
            Delay(200);
            PushPickInpos(false);
            Clamp(false);
        }

        public void InposForPicking()
        {
            UpBlockPick(true);

            RunBeltPick();
            Delay(1000);

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
                            Task.Run(() =>
                            {
                                Delay(20000);
                                StopBeltConveyorOne();
                            });
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
                            NgCount++;
                            Task.Run(() =>
                            {
                                Delay(10000);
                                StopBeltBin();
                            });
                        }
                    }

                    if (NgCount>=MaxNg)
                    {
                        NgFullWarning = true;
                        if (RobotBinning == false)
                        {
                            RunBeltBin();
                        }
                    }
                    else
                    {
                        if (NgCount==0)
                        {
                            NgFullWarning = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccured(40022, ex.Message);
                    Delay(3000);
                }

                Delay(100);
            }
        }

        private void BufferPhoneSensorMonitor()
        {
            int count = 0;
            while (true)
            {
                try
                {
                    if (PickBufferPhoneSensor())
                    {
                        count++;
                        if (count>10)
                        {
                            PickBufferHasPhone = true;
                        }
                    }
                    else
                    {
                        count = 0;
                        PickBufferHasPhone = false;
                    }

                    Delay(100);
                }
                catch (Exception ex)
                {

                    OnErrorOccured(40022, ex.Message);
                    Delay(1000);
                }
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