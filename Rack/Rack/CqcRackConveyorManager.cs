﻿using System;
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
        /// <see cref="Conveyor.Monitor"/>
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
                                //if (ScannerOnline)
                                //{
                                //    if (Scanner.ScanSuccessful == false)
                                //    {
                                //        throw new Exception("Scan fail, please remove phone manually.");
                                //    }
                                //}                               
                                Conveyor.InposForPicking();
                                //Conveyor is still stop, so no new SN would enter.
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
                }
                catch (Exception e)
                {
                    OnErrorOccured(40007, e.Message);
                    Delay(3000);
                }
                finally
                {
                    ConveyorIsBusy = false;
                }

                Delay(100);
            }
        }

        private bool OkToLetInNewPhone()
        {
            if (Conveyor.HasPlaceAPhone)
            {
                return false;
            }

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
                if (box.Enabled && box.Empty && box.Type == ShieldBoxType.Rf && box.GoldPhoneCheckRequest == false)
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
