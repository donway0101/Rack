using System;
using System.Collections.Generic;
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
        /// A flag trigger conveyor to reload for picking.
        /// </summary>
        public bool ReadyForPicking { get; set; }



        public PickAndPlaceConveyor(EthercatIO io)
        {
            IO = io;
        }

        public void SetCylinder(Output output, Input input)
        {

        }

        public void ResetCylinder(Output output, Input input)
        {

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
            Stop();

            ConveyorWorkingThread = new Thread(DoWork);
            ConveyorWorkingThread.IsBackground = true;
            ConveyorWorkingThread.Start();
        }

        private void DoWork()
        {
            while (true)
            {
                try
                {
                    if (ReadyForPicking == false)
                    {
                        //Up block
                        //Run belt
                        //Inpos
                        //Clamp
                        //Push 

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
