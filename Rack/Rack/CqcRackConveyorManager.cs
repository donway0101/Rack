using System;
using System.Collections.Generic;
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
        /// While robot is doing a placing or picking.
        /// </summary>
        public bool RobotWorkingOnConveyor { get; set; }

        private void ConveyorManager()
        {
            while (true)
            {
                try
                {
                    if (RobotWorkingOnConveyor == false && LatestPhone==null)
                    {
                        //if (Conveyor._pickBufferHasPhone)
                        {
                            Conveyor.CommandInposForPicking = true;
                        }

                        //When to run belt? As long as no picking or placing.
                        //When to ready for picking? make it a subroutine? When pick, call it.
                        //Other time, no phone, prepare it.
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

        public void StartConveyorManager()
        {
            if (ConveyorOnline)
            {
                if (Conveyor == null)
                {
                    Conveyor = new Conveyor(EcatIo);
                }

                Conveyor.Start();

                Conveyor.PickBufferPhoneComing -= Conveyor_PickBufferPhoneComing;
                Conveyor.PickBufferPhoneComing += Conveyor_PickBufferPhoneComing;
                Conveyor.PhoneReadyForPicking -= ConveyorOnPhoneReadyForPicking;
                Conveyor.PhoneReadyForPicking += ConveyorOnPhoneReadyForPicking;
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
            //throw new NotImplementedException();
        }

        private void ConveyorOnPhoneReadyForPicking(object sender, string description)
        {
            //throw new NotImplementedException();
        }

        private void Conveyor_PickBufferPhoneComing(object sender, string description)
        {
            //AddNewPhone();
        }

    }
}
