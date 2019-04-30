using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rack;
using System.Threading;

namespace Rack
{
    public partial class CqcRack
    {       
        /// <summary>
        /// 
        /// </summary>
        /// //When to run belt? As long as no picking or placing.
        private void ConveyorManager()
        {
            while (true)
            {
                _conveyorWorkingManualResetEvent.WaitOne();

                try
                {
                    ConveyorIsBusy = true;

                    if (LatestPhone==null)
                    {
                        if (Conveyor.PickBufferHasPhone)
                        {
                            if (OkToLetInNewPhone())
                            {
                                Conveyor.InposForPicking();
                                AddNewPhone();
                            }
                        }
                    }

                    if (LatestPhone == null || Conveyor.PickBufferHasPhone==false || Conveyor.HasPlaceAPhone)
                    {
                        Conveyor.RunBeltPick();
                    }

                    if (LatestPhone != null && Conveyor.PickBufferHasPhone && Conveyor.HasPlaceAPhone == false)
                    {
                        Conveyor.StopBeltPick();
                    }

                    ConveyorIsBusy = false;

                }
                catch (Exception e)
                {
                    OnErrorOccured(444, e.Message);
                    Delay(5000);
                }

                Delay(100);
            }
        }

        private bool OkToLetInNewPhone()
        {
            int failPhoneNum = 0;
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled)
                {
                    if (box.Phone != null)
                    {
                        if (box.Phone.TestResult == TestResult.Fail)
                        {
                            failPhoneNum++;
                        }
                    }
                }
            }

            if (failPhoneNum>1)
            {
                return false;
            }

            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled && box.Empty && box.Type == ShieldBoxType.Rf)
                {
                    return true;
                }
            }

            return false;
        }

        public void RobotTakeControlOnConveyor(int timeout=30000)
        {
            _conveyorWorkingManualResetEvent.Reset();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (ConveyorIsBusy != false)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                    throw new Exception("RobotTakeControlOnConveyor timeout");

                Delay(10);
            }
        }

        public void RobotReleaseControlOnConveyor()
        {
            _conveyorWorkingManualResetEvent.Set();
        }

        public void StartConveyorManager()
        {
            if (_conveyorManagerThread == null)
            {
                _conveyorManagerThread = new Thread(ConveyorManager)
                {
                    IsBackground = true
                };
            }

            if (_conveyorManagerThread.IsAlive == false)
            {
                _conveyorManagerThread.Start();
            }

            _conveyorWorkingManualResetEvent.Set();
        }

        private void Conveyor_ErrorOccured(object sender, int code, string description)
        {
            OnErrorOccured(code, description);
        }

    }
}
