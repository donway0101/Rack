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
        /// //When to ready for picking? make it a subroutine? When pick, call it.
        /// //Other time, no phone, prepare it.
        private void ConveyorManager()
        {
            while (true)
            {
                _conveyorWorkingManualResetEvent.WaitOne();

                try
                {
                    if (Conveyor.PickBufferHasPhone && LatestPhone==null)
                    {                       
                        ConveyorIsBusy = true;
                        Conveyor.InposForPicking();
                        AddNewPhone();
                        ConveyorIsBusy = false;
                    }
                }
                catch (Exception e)
                {
                    OnErrorOccured(444, e.Message);
                    Delay(5000);
                }
                Delay(100);
            }
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
            if (ConveyorOnline)
            {
                if (Conveyor == null)
                {
                    Conveyor = new Conveyor(EcatIo);
                }

                Conveyor.Start();

                Conveyor.ErrorOccured -= Conveyor_ErrorOccured;
                Conveyor.ErrorOccured += Conveyor_ErrorOccured;
            }

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
        }

        private void Conveyor_ErrorOccured(object sender, int code, string description)
        {
            OnErrorOccured(code, description);
        }

    }
}
