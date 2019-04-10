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
                List<Phone> luckyPhones = new List<Phone>();

                try
                {
                    luckyPhones = FindPhones();
                    //Phone to be served
                    Phone mostLuckyPhone = luckyPhones.First();


                    //Collect fails
                    
                    ;


                }
                catch (Exception e)
                {
                    if (luckyPhones.Count>0)
                    {
                        RecyclePhones(luckyPhones);
                    }
                    OnErrorOccured(e.Message);
                    PhoneServerManualResetEvent.Reset();
                }

                Delay(500);
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
            List<Phone> phonesToUnloadAndLoad = new List<Phone>();
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
                            case RackProcedure.UnloadAndLoad:
                                phonesToUnloadAndLoad.Add(phone);
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
            List<TeachPos> availableBox = new List<TeachPos>();
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled & box.Available)
                {
                    availableBox.Add(box.TeachPos);
                }
            }

            //Like a bus, out first.
            //The first phone's next target position better match the second 
            // phone's current position.
            //Can't full fill all box cause no AB mode available.

            //Find the circle.
            if (phonesToBin.Count > 0)
            {
                foreach (var phone in phonesToBin)
                {
                    //Imaginely release.
                    availableBox.Add(phone.CurrentTeachPos);
                }
            }
            else
            {
                if (phonesToPlace.Count > 0)
                {
                    chosenPhones.Add(phonesToPlace.First());
                }
            }

            if (phonesToUnloadAndLoad.Count > 0)
            {
                chosenPhones.Add(phonesToUnloadAndLoad.First());
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
