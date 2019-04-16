using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rack
{
    public partial class CqcRack
    {
        /// <summary>
        /// 
        /// </summary>
        /// Within the list, two close phones, If a phone's next target position match
        ///  a phone's current target postion, then unload and load could happen.
        /// Pick first and then bin or place.
        /// Todo what if all box fails and no break out?
        /// if next target is unsure, then has to find a box for it.
        ///   TargetPosition(){TeachPos = TeachPos.None};
        /// Can exchange between retrys.
        /// Make sure retry and pick has place to go, otherwise no move.
        /// After motion finish, update position of phone in box.
        private void PhoneServer()
        {           
            while (true)
            {
                PhoneServerManualResetEvent.WaitOne();

                Delay(500);
                if (HasNoPhoneToBeServed())
                    continue;

                List<Phone> luckyPhones = SortPhones();
                RemovePhoneToBeServed(luckyPhones);

                try
                {
                    while (luckyPhones.Count > 0)
                    {
                        //After finish, remove phone from the list.
                        //If there is three phones, most likely has two combo move.
                        if (luckyPhones.Count >= 2)
                        {
                            var firstPhone = luckyPhones.First();
                            var secondPhone = luckyPhones.First();
                            if (firstPhone.NextTargetPosition.TeachPos == secondPhone.CurrentTargetPosition.TeachPos)
                            {

                            }
                            else
                            {
                                //Maybe it's 
                            }
                        }
                        else
                        {
                            //Only one phone.
                            var theOnlyPhone = luckyPhones.First();
                            switch (theOnlyPhone.Procedure)
                            {
                                case RackProcedure.Bin:
                                    if (theOnlyPhone.Type == PhoneType.Normal)
                                    {

                                    }
                                    else //A gold phone.
                                    {

                                    }

                                    //Todo...
                                    luckyPhones.Remove(theOnlyPhone);

                                    break;
                                case RackProcedure.Place:
                                    if (theOnlyPhone.Type == PhoneType.Normal)
                                    {
                                        theOnlyPhone.NextTargetPosition = Motion.PickPosition;
                                        //Todo Put the phone back to gold position,
                                        ShieldBox box = theOnlyPhone.ShieldBox;
                                        box.Phone = null;
                                        box.Available = true;
                                        box.Empty = true;
                                        box.GoldPhoneChecked = true;
                                        luckyPhones.Remove(theOnlyPhone);
                                    }
                                    else //A gold phone.
                                    {
                                        theOnlyPhone.NextTargetPosition = ConvertGoldIdToTargetPosition(theOnlyPhone.Id);
                                        //Todo move it to gold postion.
                                        ShieldBox box = theOnlyPhone.ShieldBox;
                                        box.Phone = null;
                                        box.Available = true;
                                        box.Empty = true;
                                        box.GoldPhoneChecked = true;
                                        luckyPhones.Remove(theOnlyPhone);
                                    }

                                    break;
                                case RackProcedure.Retry:
                                    if (theOnlyPhone.Type == PhoneType.Normal)
                                    {
                                        ShieldBox goldBoxbox = theOnlyPhone.ShieldBox;
                                        goldBoxbox.Available = true;
                                        goldBoxbox.Empty = true;
                                        luckyPhones.Remove(theOnlyPhone);
                                    }
                                    else //A gold phone.
                                    {
                                        //Todo Put the phone back to gold position, or retry the test.
                                        ShieldBox box = theOnlyPhone.ShieldBox;
                                        box.Phone = null;
                                        box.Available = true;
                                        box.Empty = true;
                                        luckyPhones.Remove(theOnlyPhone);
                                    }

                                    break;
                                case RackProcedure.Pick:
                                    #region Pick a phone.
                                    if (theOnlyPhone.Type == PhoneType.Normal)
                                    {
                                        ShieldBox box = GetBoxForNewPhone();
                                        //Todo, if gold time arrived, then maybe not 
                                        // place new phone into unchecked box, so the gold phone
                                        // has chance to work.
                                        box.Available = false;
                                        box.Empty = false;
                                        box.Phone = theOnlyPhone;
                                        box.Phone.TestResult = TestResult.None;
                                        box.Phone.ShieldBox = box;
                                        luckyPhones.Remove(theOnlyPhone);
                                    }
                                    else //A gold phone.
                                    {
                                        ShieldBox box = GetBoxForGoldPhone();
                                        //Grap a phone
                                        //Todo load gold phone till finish.
                                        theOnlyPhone.CurrentTargetPosition = box.Position;
                                        box.Available = false;
                                        box.Empty = false;
                                        box.Phone = theOnlyPhone;
                                        box.Phone.TestResult = TestResult.None;
                                        box.Phone.ShieldBox = box;
                                        luckyPhones.Remove(theOnlyPhone);
                                    }
                                    #endregion
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                        }
                    }

                    //foreach (var footprint in phone.TargetPositionFootprint)
                    //{
                    //    if (retryCandidateBox == footprint.TeachPos)
                    //    {
                    //        foundABox = false;
                    //    }
                    //}

                    PrintStateOfBoxes();
                }
                catch (ShieldBoxNotFoundException e)
                {
                    //Todo Make sure no gold phone come in after all box checked.
                    if (luckyPhones.Count > 0)
                    {
                        RecyclePhones(luckyPhones);
                    }
                    //Todo error code
                    OnInfoOccured(0, e.Message);
                    //PhoneServerManualResetEvent.Reset();
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



        private void PrintStateOfBoxes()
        {
            foreach (var box in ShieldBoxs)
            {
                Console.Write("{0}{1}{2}", box.Id, box.Available, box.Empty);
                if (box.Phone != null)
                {
                    Console.Write("{0}{1}", box.Phone.Id, box.Phone.Type);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
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
        /// todo make sure give the right next target position.
        /// If gold phone time come, let gold phone come into unchecked box first.
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
                            if (phone.TestResult == TestResult.Pass)
                            {
                                phone.NextTargetPosition = Motion.PickPosition;
                                phone.Procedure = RackProcedure.Place;
                                binOrPlacePhone.Add(phone);
                            }
                            else
                            {
                                //Phone to retry.
                                if (phone.TestResult == TestResult.Fail)
                                {
                                    //phone.NextTargetPosition=?
                                    phone.Procedure = RackProcedure.Retry;
                                    retryPhone.Add(phone);
                                }
                                else
                                {
                                    //Phone to pick or it's gold.
                                    if (phone.TestResult == TestResult.None)
                                    {
                                        //phone.NextTargetPosition=?
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
            //Build with place or pick, or both.
            if (retryPhone.Count>0)
            {
                //May build two unload and load movement.
                if (binOrPlacePhone.Count > 0 & pickPhone.Count>0)
                {
                    List<Phone> bOpPhoneCouple = new List<Phone>();
                    List<Phone> rPhoneCouple = new List<Phone>();
                    List<Phone> pPhoneCouple = new List<Phone>();
                    foreach (var rPhone in retryPhone)
                    {
                        foreach (var bOpPhone in binOrPlacePhone)
                        {
                            if (bOpPhone.AtBoxType == rPhone.AtBoxType)
                            {
                                rPhone.NextTargetPosition = bOpPhone.CurrentTargetPosition;
                                bOpPhoneCouple.Add(bOpPhone); //Order matters.
                                rPhoneCouple.Add(rPhone);
                            }
                        }
                    }

                    //Has unload and load for bin or place phone.
                    if (rPhoneCouple.Count>0)
                    {
                        //Try to find the second unload and load.
                        foreach (var rPhone in rPhoneCouple)
                        {
                            foreach (var pPhone in pickPhone)
                            {
                                if (pPhone.AtBoxType == rPhone.AtBoxType)
                                {
                                    pPhone.NextTargetPosition = rPhone.CurrentTargetPosition;
                                    rPhone.NextTargetPosition = bOpPhoneCouple.ElementAt(rPhoneCouple.IndexOf(rPhone))
                                        .CurrentTargetPosition;
                                    luckyPhones.Add(bOpPhoneCouple.ElementAt(rPhoneCouple.IndexOf(rPhone))); //Order matters.
                                    luckyPhones.Add(rPhone);
                                    luckyPhones.Add(pPhone);
                                    return luckyPhones; //Find two unload and load movement.
                                }
                            }
                        }

                        // Can't build unload and load with pick.
                        //So just leave pick alone.
                        luckyPhones.Add(bOpPhoneCouple.First());
                        luckyPhones.Add(rPhoneCouple.First());
                        return luckyPhones; //Find two unload and load movement.
                    }
                    else //No unload and load for place phones.
                    {
                        foreach (var rPhone in retryPhone)
                        {
                            foreach (var pPhone in pickPhone)
                            {
                                if (pPhone.AtBoxType == rPhone.AtBoxType)
                                {
                                    pPhone.NextTargetPosition = rPhone.CurrentTargetPosition;
                                    luckyPhones.Add(rPhone);//Order matters.
                                    luckyPhones.Add(pPhone);
                                    return luckyPhones;
                                }
                            }
                        }

                        //No combo movement at all.
                        luckyPhones.Add(bOpPhoneCouple.First());
                        luckyPhones.Add(rPhoneCouple.First());
                        return luckyPhones;
                    }
                }
                else
                {
                    if (binOrPlacePhone.Count > 0)
                    {
                        //Just retry and bin or place.
                        foreach (var rPhone in retryPhone)
                        {
                            foreach (var bOpPhone in binOrPlacePhone)
                            {
                                if (bOpPhone.AtBoxType == rPhone.AtBoxType)
                                {
                                    rPhone.NextTargetPosition = bOpPhone.CurrentTargetPosition;
                                    luckyPhones.Add(bOpPhone); //Order matters.
                                    luckyPhones.Add(rPhone);
                                    return luckyPhones;
                                }
                            }
                        }
                        luckyPhones.Add(binOrPlacePhone.First());
                        luckyPhones.Add(retryPhone.First());
                        return luckyPhones;
                    }
                    else
                    {
                        if (pickPhone.Count > 0)
                        {
                            //Can pick and retry.
                            //Todo return pick and retry phone, in main thread, if no room for pick then just retry.
                            foreach (var pPhone in pickPhone)
                            {
                                foreach (var rPhone in retryPhone)
                                {
                                    if (pPhone.AtBoxType == rPhone.AtBoxType)
                                    {
                                        pPhone.NextTargetPosition = rPhone.CurrentTargetPosition;
                                        luckyPhones.Add(rPhone); //Order matters.
                                        luckyPhones.Add(pPhone);
                                        return luckyPhones;                                 
                                    }
                                }
                            }
                            //Choose two ramdon phones.
                            luckyPhones.Add(retryPhone.First());
                            luckyPhones.Add(pickPhone.First());
                            return luckyPhones;
                        }
                        else
                        {
                            //Just retry.
                            //Todo Find available box, if two box is both retry, then exchange.
                            luckyPhones.Add(retryPhone.First());
                            return luckyPhones;
                        }
                    }
                }
            }
            else
            {
                //Bin or place a phone build a unload and load movement.
                if (binOrPlacePhone.Count>0)
                {
                    //No retry. Just place and pick.
                    if (pickPhone.Count>0)
                    {
                        foreach (var pPhone in pickPhone)
                        {
                            foreach (var bOpPhone in binOrPlacePhone)
                            {
                                if (pPhone.AtBoxType == bOpPhone.AtBoxType)
                                {
                                    // pPhone.Procedure is set before.
                                    pPhone.NextTargetPosition = bOpPhone.CurrentTargetPosition;
                                    luckyPhones.Add(bOpPhone); //Order matters.
                                    luckyPhones.Add(pPhone);
                                    return luckyPhones; //Unload and load.                                  
                                }
                            }
                        }
                        //No UnloadAndLoad Move;
                        luckyPhones.Add(binOrPlacePhone.First());
                        luckyPhones.Add(pickPhone.First());
                        return luckyPhones; //Load and place or bin separately.
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
                    //Gold phone first.
                    foreach (var phone in pickPhone)
                    {
                        if (phone.Type == PhoneType.Golden)
                        {
                            luckyPhones.Add(phone);
                            return luckyPhones;
                        }
                    }

                    //If no gold phone, then a regular phone.
                    luckyPhones.Add(pickPhone.First());
                    return luckyPhones;
                }
            }
        }

        private ShieldBox GetBoxForNewPhone()
        {
            //Try to find a empty box.
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled & box.Empty)
                {
                    return box;
                }
            }

            //If not found a empty box, try to find a available box.
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled & box.Available)
                {
                    return box;
                }
            }

            throw new ShieldBoxNotFoundException("GetAvailableBoxForNewPhone fail.");
        }

        private ShieldBox GetBoxForGoldPhone()
        {
            //If not found a empty box, try to find a available box.
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled & box.Available & box.GoldPhoneChecked==false)
                {
                    return box;
                }
            }

            throw new ShieldBoxNotFoundException("GetBoxForGoldPhone fail.");
        }

        private void GetAvailableBoxForNewPhone3333()
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
        /// <param name="phones"></param>
        /// <seealso cref="RemovePhoneToBeServed"/>
        private void RecyclePhones(IEnumerable<Phone> phones)
        {
            lock (_phoneToBeServedLocker)
            {
                PhoneToBeServed.AddRange(phones);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phones"></param>
        /// <seealso cref="RecyclePhones"/>
        private void RemovePhoneToBeServed(IEnumerable<Phone> phones)
        {
            lock (_phoneToBeServedLocker)
            {
                foreach (var phone in phones)
                {
                    PhoneToBeServed.Remove(phone);
                }
            }
        }

        public void AddPhoneToBeServed(Phone phone)
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
