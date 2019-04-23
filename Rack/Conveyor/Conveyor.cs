using System;
using System.Diagnostics;
using System.Threading;

namespace Rack
{
    public class Conveyor
    {
        private readonly EthercatIo _io;
        private Thread _conveyorWorkingThread, _conveyorMonitorThread;
                                                                                                                                                                                             
        public bool PickBufferHasPhone { get; set; }

        public bool ConveyorMovingForward { get; set; } = true;

        /// <summary>
        /// Release clamp for picking.
        /// </summary>
        /// Todo After Picking, reset it by host.
        public bool ReadyForPicking { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// Todo set by Rack after picking.
        public bool RobotHasPickedAPhone { get; set; } = true;

        public bool PickBeltOkToRun { get; set; }

        /// <summary>
        ///  Phone clamped under pick position.
        /// </summary>
        /// Todo After Picking, reset it by host.
        public bool InposForPicking { get; set; }

        /// <summary>
        /// About to pick a phone.
        /// </summary>
        public bool CommandInposForPicking { get; set; }

        /// <summary>
        /// Stop conveyor, Open clamp for picking.
        /// </summary>
        public bool CommandReadyForPicking { get; set; }

        //Todo set it in motion.
        public bool HasBinAPhone { get; set; }

        public bool HasPlaceAPhone { get; set; }

        // Todo set it in motion.
        public bool LastPlaceSuccessful { get; set; } = true;

        public bool LastBinSuccessful { get; set; } = true;

        private bool _placedPhoneDetected;
        private bool _binedPhoneDetected;

        #region Events

        public delegate void ErrorOccuredEventHandler(object sender, int code, string description);

        public event ErrorOccuredEventHandler ErrorOccured;

        protected void OnErrorOccured(int code, string description)
        {
            ErrorOccured?.Invoke(this, code, description);
        }

        public delegate void PhoneReadyForPickingEventHandler(object sender, string description);

        public event PhoneReadyForPickingEventHandler PhoneReadyForPicking;

        protected void OnPhoneReadyForPicking(string description)
        {
            PhoneReadyForPicking?.Invoke(this, description);
        }

        public delegate void PickBufferPhoneComingEventHandler(object sender, string description);

        public event PickBufferPhoneComingEventHandler PickBufferPhoneComing;

        protected void OnPickBufferPhoneComing(string description)
        {
            PickBufferPhoneComing?.Invoke(this, description);
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

            if (_conveyorWorkingThread == null)
                _conveyorWorkingThread = new Thread(DoWork)
                {
                    IsBackground = true
                };

            if (_conveyorWorkingThread.IsAlive == false) _conveyorWorkingThread.Start();

            if (_conveyorMonitorThread == null)
                _conveyorMonitorThread = new Thread(Monitor)
                {
                    IsBackground = true
                };

            if (_conveyorMonitorThread.IsAlive == false) _conveyorMonitorThread.Start();
        }

        public bool OkForPlacing()
        {
            return (CommandReadyForPicking==false && CommandInposForPicking == false);
        }

        public void ReadyForPlacing()
        {

        }

        private void DoWork()
        {
            ReadyForPicking = false;
            InposForPicking = false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
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
                            OnPhoneReadyForPicking("");
                            ReadyForPicking = true;
                        }
                        else
                        {
                            CommandInposForPicking = true;
                        }
                    }

                    //Todo combine other condition.
                    //Todo When to run?
                    if (PickBeltOkToRun && ReadyForPicking == false)
                    {
                        RunBeltPick(true);
                    }

                    if (CommandInposForPicking && (InposForPicking == false) && PickBufferHasPhone)
                    {
                        ReadyForPicking = false;
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

                        InposForPicking = true;

                        UpBlockSeparate(true);
                        SideBlockSeparate(false);
                        UpBlockPick(false);

                        CommandInposForPicking = false;
                    }

                    Delay(50);
                }
                catch (Exception ex)
                {
                    //Todo write document about error code.
                    OnErrorOccured(0, ex.Message);
                }
            }
        }

        private void Monitor()
        {
            int pickBufferSensorCount = 0;
            bool reportAComingPhone = false;
            while (true)
            {
                try
                {
                    if (HasPlaceAPhone)
                    {
                        if (PickBufferPhoneSensor())
                        {
                            _placedPhoneDetected = true;
                        }

                        if (_placedPhoneDetected && PickBufferPhoneSensor() == false)
                        {
                            LastPlaceSuccessful = true;
                            HasPlaceAPhone = false;
                            _placedPhoneDetected = false;
                        }
                    }

                    if (HasBinAPhone)
                    {
                        if (_io.GetInput(Input.ConveyorBinIn))
                        {
                            _binedPhoneDetected = true;
                        }

                        if (_binedPhoneDetected && _io.GetInput(Input.ConveyorBinIn) == false)
                        {
                            LastBinSuccessful = true;
                            HasBinAPhone = false;
                            _binedPhoneDetected = false;
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

                        if (PickBufferHasPhone && reportAComingPhone==false)
                        {
                            OnPickBufferPhoneComing("");
                            reportAComingPhone = true;
                        }
                    }
                    else
                    {
                        pickBufferSensorCount++;
                        if (pickBufferSensorCount > 10)
                        {
                            PickBufferHasPhone = false;
                            reportAComingPhone = false;
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
               return _io.GetInput(Input.PickBufferHasPhoneForward);
        }

        public bool PickPhoneSensor()
        {
            return _io.GetInput(Input.PickHasPhone);
        }

        public void Stop()
        {
            if (_conveyorWorkingThread != null)
            {
                _conveyorWorkingThread.Abort();
                _conveyorWorkingThread.Join();
            }

            if (_conveyorMonitorThread != null)
            {
                _conveyorMonitorThread.Abort();
                _conveyorMonitorThread.Join();
            }
        }
    }
}