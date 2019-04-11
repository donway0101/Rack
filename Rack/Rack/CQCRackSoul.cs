using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rack
{
    public partial class CqcRack
    {
        private void PhoneServer()
        {           
            while (true)
            {
                PhoneServerManualResetEvent.WaitOne();

                Delay(500);
                if (HasNoPhoneToBeServed())
                    continue;

                List<Phone> luckyPhones = SortPhones(); 

                try
                {
                    //luckyPhones = FindPhones();
                    //Phone to be served
                    Phone mostLuckyPhone = luckyPhones.First();


                    //Collect fails
                    //After motion finish, update position of phone.
                    ;


                }
                catch (Exception e)
                {
                    if (luckyPhones.Count>0)
                    {
                        RecyclePhones(luckyPhones);
                    }
                    //Todo error code
                    OnErrorOccured(0,e.Message);
                    PhoneServerManualResetEvent.Reset();
                }
                
            }
        }

        private bool HasNoPhoneToBeServed()
        {
            lock (_phoneToBeServedLocker)
            {
                return PhoneToBeServed.Count == 0;
            }
        }

        /// <summary>
        /// Optimize movement efficiency.
        /// </summary>
        /// Follow rule of test mode of rack.
        ///  if AA mode,  need to reload the phone?
        private List<Phone> SortPhones()
        {
            GetAvailableBox();
            switch (TestMode)
            {
                case RackTestMode.AB:

                    return null;
                case RackTestMode.AAB:
                    //In use.
                    return null;
                case RackTestMode.ABC:
                    //In use.
                    return ArrangeAbcMode();
                    
                case RackTestMode.ABA:
                    return null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// Priority bin, place, retry, gold, pick
        /// Rules:
        /// like a bus, out first.
        /// Find the movement circle.
        /// If empty box is enough, load first, place later.
        /// retry than pick
        /// Gold go back to gold.
        /// Todo match shieldbox test step and phone's step.
        /// Has to avoid conflict, next target position can not be same.
        /// ABA mode, for one phone, definition of box AB can be different.
        /// Find a procedure circle, those phones's start and ending is both on conveyor.
        private List<Phone> ArrangeAbcMode(int maxFailCount = 3)
        {
            //Retry testing phone better goes into bin phone or place phone,
            // which can has unload and load movement.
            // Equal to empty box.
            List<TeachPos> binOrPlaceBox = new List<TeachPos>();
            bool hasToBin, hasToPlace, hasToRetry, hasToGold, hasToPick;
            List<Phone> luckyPhones = new List<Phone>();
            
            lock (_phoneToBeServedLocker)
            {
                if (PhoneToBeServed.Count > 0)
                {
                    foreach (var phone in PhoneToBeServed)
                    {
                        //Phone to bin.
                        if (phone.FailCount >= maxFailCount)
                        {
                            phone.NextTargetPosition = Motion.BinPosition;
                            phone.Procedure = RackProcedure.Bin;
                            //Better for retry
                            binOrPlaceBox.Add(phone.CurrentTargetPosition.TeachPos);
                            continue;
                        }

                        //Phone to place.
                        if (phone.TestResult == ShieldBoxTestResult.Pass)
                        {
                            phone.NextTargetPosition = Motion.PickPosition;
                            phone.Procedure = RackProcedure.Place;
                            binOrPlaceBox.Add(phone.CurrentTargetPosition.TeachPos);
                            continue;
                        }

                        #region Find a box for retry testing.
                        //Phone to retry testing.
                        //First try to find a box which need bin or place, if fail, 
                        // try to find an empty box.
                        if (phone.TestResult == ShieldBoxTestResult.Fail & phone.FailCount < maxFailCount)
                        {
                            phone.Procedure = RackProcedure.Retry;
                            //Assume found it.
                            bool foundABox = true;
                            TeachPos retryCandidateBox = TeachPos.NoWhere;
                            ShieldBox retryBox = null;

                            //Better put phone in a bining or placing box.
                            foreach (var boxTeachPose in binOrPlaceBox)
                            {
                                //Give box a chance very single time.
                                foundABox = true;
                                retryCandidateBox = boxTeachPose;
                                foreach (var footprint in phone.TargetPositionFootprint)
                                {
                                    if (retryCandidateBox == footprint.TeachPos)
                                    {
                                        foundABox = false;
                                    }
                                }

                                if (foundABox)
                                {
                                    //Todo lock to these two phones.
                                    binOrPlaceBox.Remove(boxTeachPose);
                                    break;
                                }
                            }

                            //Found a bin or place box for retry.
                            if (foundABox==false)
                            { 
                                //No bin or place box
                                //Find a empty box for retry.
                                retryCandidateBox = TeachPos.NoWhere;
                                foreach (var box in ShieldBoxs)
                                {
                                    if (box.Enabled & box.Available & box.Empty)
                                    {
                                        
                                    }
                                }
                            }

                            if (foundABox)
                            {
                                //Find box by techPos.
                                foreach (var box in ShieldBoxs)
                                {
                                    if (retryCandidateBox == box.TeachPos)
                                    {
                                        retryBox = box;
                                        break;
                                    }
                                }

                                if (retryBox == null)
                                {
                                    throw new Exception("Find box error code: 5468648945");
                                }
                            }
                        }
                        #endregion

                    }
                }
            }
        }

        private void GetAvailableBox()
        {
            lock (_availableBoxLocker)
            {
                AvailableBox.Clear();
                foreach (var box in ShieldBoxs)
                {
                    if (box.Enabled & box.Available)
                    {
                        AvailableBox.Add(box);
                    }
                }
            }           
        }

        private void RecyclePhones(IEnumerable<Phone> phones)
        {
            lock (_phoneToBeServedLocker)
            {
                PhoneToBeServed.AddRange(phones);
            }
        }

        private void AddPhoneToBeServed(Phone phone)
        {
            lock (_phoneToBeServedLocker)
            {
                PhoneToBeServed.Add(phone);
            }
        }

        private void RemovePhoneToBeServed(Phone phone)
        {
            lock (_phoneToBeServedLocker)
            {
                PhoneToBeServed.Remove(phone);
            }
        }

        public void StartPhoneServer()
        {
            if (_phoneServerThread == null)
            {
                _phoneServerThread = new Thread(PhoneServer)
                {
                    IsBackground = true
                };
            }

            if (_phoneServerThread.IsAlive == false)
            {
                _phoneServerThread.Start();
            }
        }

        private void Delay(int millisec)
        {
            Thread.Sleep(millisec);
        }

        public void StopPhoneServer()
        {
            if (_phoneServerThread != null)
            {
                _phoneServerThread.Abort();
                _phoneServerThread.Join();
            }
        }
    }
}
