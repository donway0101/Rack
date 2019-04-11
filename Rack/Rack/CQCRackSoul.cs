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

                ArrangeNextTargetPosition();

                List<Phone> luckyPhones = new List<Phone>();

                try
                {
                    luckyPhones = FindPhones();
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
        private void ArrangeNextTargetPosition()
        {
            GetAvailableBox();
            switch (TestMode)
            {
                case RackTestMode.AB:
                    
                    break;
                case RackTestMode.AAB:
                    //In use.
                    break;
                case RackTestMode.ABC:
                    //In use.
                    ArrangeAbcMode();
                    break;
                case RackTestMode.ABA:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// Priority bin, place, retry, gold, pick
        /// Rules:
        /// 1. like a bus, out first.
        /// retry than pick
        /// Gold go back to gold.
        /// Todo match shieldbox test step and phone's step.
        /// Has to avoid conflict, next target position can not be same.
        private void ArrangeAbcMode(int maxFailCount = 3)
        {
            //Retry testing phone better goes into bin phone or place phone,
            // which can has unload and load movement.
            // Equal to empty box.
            List<TeachPos> binOrPlaceBox = new List<TeachPos>();
            
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
                        if (phone.TestResult== ShieldBoxTestResult.Pass)
                        {
                            phone.NextTargetPosition = Motion.PickPosition;
                            phone.Procedure = RackProcedure.Place;
                            binOrPlaceBox.Add(phone.CurrentTargetPosition.TeachPos);
                            continue;
                        }

                        //Phone to retry testing.
                        //First try to find a box which need bin or place, if fail, 
                        // try to find an empty box.
                        if (phone.TestResult == ShieldBoxTestResult.Fail & phone.FailCount< maxFailCount)
                        {
                            //Assume found it.
                            bool foundABox = true;
                            TeachPos candidateBox;

                            //Better put phone in a bining or placing box.
                            foreach (var boxTeachPose in binOrPlaceBox)
                            {
                                //Give box a chance very single time.
                                foundABox = true;
                                candidateBox = boxTeachPose;
                                foreach (var footprint in phone.TargetPositionFootprint)
                                {
                                    if (candidateBox == footprint.TeachPos)
                                    {
                                        foundABox = false;
                                    }
                                }
                                if (foundABox)
                                {
                                    break;
                                }
                            }

                            //Found a bin or place box for retry.
                            if (foundABox)
                            {
                                foreach (var location in Motion.Locations)
                                {
                                    
                                }
                                //phone.NextTargetPosition
                            }
                            else
                            {
                                //Find a empty box for retry.

                            }


                            lock (_availableBoxLocker)
                            {
                                foreach (var box in AvailableBox)
                                {
                                    //candidateBox = box;
                                    //foreach (var footprint in phone.TargetPositionFootprint)
                                    //{
                                    //    //Todo match shield box type.
                                    //    if (footprint.TeachPos == box.TeachPos)
                                    //    {
                                    //        foundABox = false;
                                    //    }
                                    //}
                                }
                            }



                        }


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

        /// <summary>
        /// 
        /// </summary>
        /// ABA mode, for one phone, definition of box AB can be different.
        /// Find a procedure circle, those phones's start and ending is both on conveyor.
        /// How deep combination of procedure varys, depands on efficency.
        /// <returns></returns>
        private List<Phone> FindPhones()
        {
            List<Phone> chosenPhones = new List<Phone>(); 

            List<Phone> phonesToBin = new List<Phone>();
            List<Phone> phonesToPlace = new List<Phone>();
            //If previous phone is empty, then no load, likewise, no phone next, no unload.
            List<Phone> phonesToAnotherBox = new List<Phone>();
            List<Phone> phonesToPick = new List<Phone>();

            //Sort phones by next procedure.
            lock (_phoneToBeServedLocker)
            {
                if (PhoneToBeServed.Count > 0)
                {
                    foreach (var phone in PhoneToBeServed)
                    {
                        switch (phone.Procedure)
                        {
                            case RackProcedure.Bin:
                                phonesToBin.Add(phone);
                                break;
                            case RackProcedure.Place:
                                phonesToPlace.Add(phone);
                                break;
                            case RackProcedure.AnotherBox:
                                phonesToAnotherBox.Add(phone);
                                break;
                            case RackProcedure.Pick:
                                phonesToPick.Add(phone);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }

            // For optimize robot move efficiency.
            // Release box imaginely and redistribute it for best robot routine.
            // Find out available boxes.
            


            //Like a bus, out first.         
            //Can't full fill all box cause no AB mode available.
            //After chosen, set next target position of phone, so the motion could decide where to go.
            // and for phone which want to go to another box, choose the right box for it.
            //Find the circle.
            List<TeachPos> binFromBox = new List<TeachPos>();
            if (phonesToBin.Count > 0)
            {
                //Imaginely release position.
                //availableBox.Add(phone.CurrentTeachPos);

                //The first phone's next target position better match the second 
                // phone's current position. This combine a unload and load movement.
                if (phonesToAnotherBox.Count > 0)
                {
                    foreach (var phoneBin in phonesToBin)
                    {
                        //availableBox.Add(phone.CurrentTeachPos);
                        //Choose next target by test mode.
                        foreach (var phoneUnL in phonesToAnotherBox)
                        {
                            if (phoneBin.CurrentTargetPosition.TeachPos == phoneUnL.NextTargetPosition.TeachPos)
                            {

                            }
                        }
                    }
                }
            }
            else
            {
                if (phonesToPlace.Count > 0)
                {
                    chosenPhones.Add(phonesToPlace.First());
                }
            }

            if (phonesToAnotherBox.Count > 0)
            {
                chosenPhones.Add(phonesToAnotherBox.First());
            }

            if (phonesToPick.Count > 0)
            {
            }

            //Remove chosen phones.
            lock (_phoneToBeServedLocker)
            {
                if (chosenPhones.Count>0)
                {
                    foreach (var phone in chosenPhones)
                    {
                        PhoneToBeServed.Remove(phone);
                    }
                }
            }
            return chosenPhones;
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
