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
        /// Generate movement by phones.
        /// </summary>
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
        /// Thoughts.......
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
        /// Retry testing phone better goes into bin phone or place phone,
        /// which can has unload and load movement.
        /// Equal to empty box.
        /// 
        private List<Phone> ArrangeAbcMode(int maxFailCount = 3)
        {
            List<Phone> luckyPhones = new List<Phone>();

            List<Phone> binOrPlacePhone = new List<Phone>();
            List<Phone> retryPhone = new List<Phone>();
            List<Phone> pickPhone = new List<Phone>();
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
                            binOrPlacePhone.Add(phone);
                        }
                        else
                        {
                            //Phone to place.
                            if (phone.TestResult == ShieldBoxTestResult.Pass)
                            {
                                phone.NextTargetPosition = Motion.PickPosition;
                                phone.Procedure = RackProcedure.Place;
                                binOrPlacePhone.Add(phone);
                            }
                            else
                            {
                                //Phone to retry.
                                if (phone.TestResult == ShieldBoxTestResult.Fail)
                                {
                                    //phone.NextTargetPosition=?
                                    phone.Procedure = RackProcedure.Retry;
                                    retryPhone.Add(phone);
                                }
                                else
                                {
                                    //Phone to pick or it's gold.
                                    if (phone.TestResult == ShieldBoxTestResult.None)
                                    {
                                        phone.Procedure = RackProcedure.Pick;
                                        pickPhone.Add(phone);
                                    }
                                    else
                                    {
                                        throw new Exception("Error from method ArrangeAbcMode()");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Retry testing a phone, build a unload and load movement.
            if (retryPhone.Count>0)
            {
                
            }
            else
            {
                //Bin or place a phone build a unload and load movement.
                if (binOrPlacePhone.Count>0)
                {
                    //No retry. Just place and pick.
                    if (pickPhone.Count>0)
                    {
                        //
                    }
                    else
                    {
                        //Just bin or place.
                        luckyPhones.Add(binOrPlacePhone.First());
                        return luckyPhones;
                    }
                }
                else
                {
                    //Just pick.
                    //If it's a gold phone, return it, main thread will deal with its type.
                    //Todo implement in work thread.
                    luckyPhones.Add(pickPhone.First());
                    return luckyPhones;
                }
            }

            

            //
            
            bool hasToBin, hasToPlace, hasToRetry, hasToGold, hasToPick, retryIsUnloadAndLoad;
            
            
            lock (_phoneToBeServedLocker)
            {
                if (PhoneToBeServed.Count > 0)
                {
                    foreach (var phone in PhoneToBeServed)
                    {
                       

                        #region Find a box for retry testing.
                        //Phone to retry testing.
                        //First try to find a box which need bin or place, if fail, 
                        // try to find an empty box.
                        if (phone.TestResult == ShieldBoxTestResult.Fail & phone.FailCount < maxFailCount)
                        {
                            phone.Procedure = RackProcedure.Retry;
                            retryPhone.Add(phone);
                            hasToRetry = true;
                            //Assume found it.
                            bool foundABox = true;
                            TeachPos retryCandidateBox = TeachPos.NoWhere;
                            ShieldBox retryBox = null;

                            //Better put phone in a bining or placing box.
                            foreach (var bOpPhone in binOrPlacePhone)
                            {
                                //Give box a chance very single time.
                                foundABox = true;
                                retryCandidateBox = bOpPhone.CurrentTargetPosition.TeachPos;
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
                                    //Add a bin or place phone to list, first blood.
                                    luckyPhones.Add(bOpPhone);
                                    retryIsUnloadAndLoad = true;
                                    //binOrPlacePhone.Remove(bOpPhone);
                                    break;
                                }
                            }
                        }
                        #endregion

                        ////Found a bin or place box for retry.
                        //if (foundABox==false)
                        //{ 
                        //    //No bin or place box
                        //    //Find a empty box for retry.
                        //    retryCandidateBox = TeachPos.NoWhere;
                        //    foreach (var box in ShieldBoxs)
                        //    {
                        //        if (box.Enabled & box.Available & box.Empty)
                        //        {

                        //        }
                        //    }
                        //}

                        //if (foundABox)
                        //{
                        //    //Find box by techPos.
                        //    foreach (var box in ShieldBoxs)
                        //    {
                        //        if (retryCandidateBox == box.TeachPos)
                        //        {
                        //            retryBox = box;
                        //            break;
                        //        }
                        //    }

                        //    if (retryBox == null)
                        //    {
                        //        throw new Exception("Find box error code: 5468648945");
                        //    }
                        //}




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
