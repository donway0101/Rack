using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Policy;
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
        /// Remove phone before unload or load movement, because if move fail,
        ///  people deal with it.
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
                    if (luckyPhones.Count > 3)
                    {
                        throw new Exception("Error 4984639789151");
                    }

                    if (luckyPhones.Count == 3)
                    {
                        //If there is three phones, it has two combo move, 
                        // and the last phone must either be place or bin.
                        var firstPhone = luckyPhones.First();
                        var secondPhone = luckyPhones.ElementAt(1);
                        var thirdPhone = luckyPhones.ElementAt(2);
                        if (firstPhone.NextTargetPosition.TeachPos == secondPhone.CurrentTargetPosition.TeachPos &&
                            secondPhone.NextTargetPosition.TeachPos == thirdPhone.CurrentTargetPosition.TeachPos)
                        {
                            //Step 1: unload the first phone.
                            //Unload();
                            if (firstPhone.ShieldBox != null) //Inside a box.
                            {
                                var box1 = firstPhone.ShieldBox;

                                firstPhone.ShieldBox = null;

                                box1.Phone = null;
                                box1.Available = true;
                                box1.Empty = true;
                            }

                            //Step 2: unload the second phone and load the first phone.
                            //UnloadAndLoad();
                            luckyPhones.Remove(firstPhone);

                            var box2 = secondPhone.ShieldBox;

                            firstPhone.ShieldBox = box2;
                            firstPhone.TargetPositionFootprint.Add(box2.Position);
                            firstPhone.CurrentTargetPosition = box2.Position;

                            box2.Phone = firstPhone;
                            box2.Available = false;
                            box2.Empty = false;

                            secondPhone.ShieldBox = null;

                            //Step 3: unload the third phone and load the second phone.
                            //UnloadAndLoad();
                            luckyPhones.Remove(secondPhone);

                            var box3 = thirdPhone.ShieldBox;

                            secondPhone.ShieldBox = box3;
                            secondPhone.TargetPositionFootprint.Add(box3.Position);
                            secondPhone.CurrentTargetPosition = box3.Position;

                            box3.Phone = secondPhone;
                            box3.Available = false;
                            box3.Empty = false;

                            thirdPhone.ShieldBox = null;

                            //Step 3: load the third phone
                            //Load();
                            //Todo if phone goes into a box.
                            luckyPhones.Remove(thirdPhone);
                        }
                        else
                        {
                            throw new Exception("Error 16229461984");
                        }
                    }
                    else
                    {
                        if (luckyPhones.Count == 2)
                        {
                            var firstPhone = luckyPhones.First();
                            var secondPhone = luckyPhones.ElementAt(1);
                            if (firstPhone.NextTargetPosition.TeachPos == 
                                secondPhone.CurrentTargetPosition.TeachPos)
                            {
                                //Deal with first phone, pick or retry.
                                switch (firstPhone.Procedure)
                                {  
                                    case RackProcedure.Retry:
                                        break;
                                    case RackProcedure.Pick:
                                        ComboPickAPhone(firstPhone);
                                        break;
                                    default:
                                        throw new Exception("Error 984616941611"); 
                                }
                                luckyPhones.Remove(firstPhone);

                                switch (secondPhone.Procedure)
                                {
                                    case RackProcedure.Bin:
                                        ComboBinAPhone(firstPhone, secondPhone);
                                        break;
                                    case RackProcedure.Place:
                                        ComboPlaceAPhone(firstPhone, secondPhone);
                                        break;
                                    case RackProcedure.Retry:
                                        break;
                                    default:
                                        throw new Exception("Error 989491878165"); 
                                }
                                luckyPhones.Remove(secondPhone);
                            }
                            else
                            {
                                throw new Exception("Error 549860315484");
                            }
                        }
                        else
                        {
                            var theOnlyPhone = luckyPhones.First();
                            switch (theOnlyPhone.Procedure)
                            {
                                case RackProcedure.Bin:
                                    BinAPhone(theOnlyPhone);
                                    break;
                                case RackProcedure.Place:
                                    PlaceAPhone(theOnlyPhone);
                                    break;
                                case RackProcedure.Retry:
                                    RetryAPhone(theOnlyPhone);
                                    break;
                                case RackProcedure.Pick:
                                    PickAPhone(theOnlyPhone);
                                    break;
                                default:
                                    throw new Exception("Error 6417987");
                            }

                            luckyPhones.Remove(theOnlyPhone);
                        }
                    }

                    //Todo comment out.
                    PrintStateOfBoxes();
                }
                catch (ShieldBoxNotFoundException e)
                {
                    //Todo error code
                    OnInfoOccured(0, e.Message);
                    //PhoneServerManualResetEvent.Reset();
                }
                catch (Exception e)
                {
                    OnErrorOccured(0, e.Message);
                    PhoneServerManualResetEvent.Reset();
                }
                finally
                {
                    if (luckyPhones.Count > 0)
                    {
                        RecyclePhones(luckyPhones);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// Some retry don't need robot to move the phone.
        /// Todo if retry box is not empty, then put it into gold
        /// buffer, set box to null, set current position to gold position.
        /// if buffer is not empty, then throw an exception.
        private void RetryAPhone(Phone phone)
        {
            ShieldBox box = phone.ShieldBox;

            if (phone.Type == PhoneType.Normal)
            {
                ShieldBox newBox = GetBoxForRetryPhone(phone);
                Unlink(phone, box);
                //Unload.
                Print("Has unload a fail phone.");
                //load new box.
                Print("Has retry a phone in box." + newBox.Id);

                Link(phone, newBox);
            }
            else //A gold phone.
            {
                //If retry, just need to reclose a box.
                //Unload box
                //Load gold
                Print("No retry for a gold phone now. Just put it back.");

                Unlink(phone, box);
            }
        }

        private void ComboRetryAPhone(Phone phone)
        {
            ShieldBox box = phone.ShieldBox;

            if (phone.Type == PhoneType.Normal)
            {
                ShieldBox newBox = GetBoxForRetryPhone(phone);
                Unlink(phone, box);
                //Unload.
                Print("Has unload a fail phone.");
                //load new box.
                Print("Has retry a phone in box." + newBox.Id);

                Link(phone, newBox);
            }
            else //A gold phone.
            {
                //If retry, just need to reclose a box.
                //Unload box
                //Load gold
                Print("No retry for a gold phone now. Just put it back.");

                Unlink(phone, box);
            }
        }

        private void PickAPhone(Phone phone)
        {
            if (phone.Type == PhoneType.Normal)
            {
                //If a solo pick but no box, it should's happen.
                ShieldBox box = GetEmptyBox();
                //Pick();
                Print("Has pick a new phone.");
                //Load();
                //Use a task to close box. If error, info user.
                Print("Has load a new phone to box." + box.Id);
                Link(phone, box);
            }
            else //A gold phone.
            {
                ShieldBox box = GetBoxForGoldPhone();
                //Unload();
                Print("Has unload a gold phone.");
                //Load();
                Print("Has load a gold phone to box." + box.Id);
                Link(phone, box);
            }
        }

        /// <summary>
        /// Just pick up a phone.
        /// </summary>
        /// <param name="phone"></param>
        private void ComboPickAPhone(Phone phone)
        {
            if (phone.Type == PhoneType.Normal)
            {
                //Pick();
                Print("Has pick a new phone.");
                //Link(phone, box);
            }
            else //A gold phone.
            {
                //Unload();
                Print("Has unload a gold phone.");
                //Link(phone, box);
            }
        }

        private static void Link(Phone phone, ShieldBox box)
        {
            phone.ShieldBox = box; //Contain info of current position.
            phone.TargetPositionFootprint.Add(box.Position); //For retry.
            phone.CurrentTargetPosition = box.Position;
            box.Available = false;
            box.Empty = false;
            box.Phone = phone;
        }

        private static void Unlink(Phone phone, ShieldBox box)
        {
            phone.ShieldBox = null; //Contain info of current position.
            box.Available = true;
            box.Empty = true;
            box.Phone = null;
        }

        private void PlaceAPhone(Phone phone)
        {
            ShieldBox box = phone.ShieldBox;
            Unlink(phone, box);
            //Unload
            Print("Has unload a phone from box." + box.Id);

            if (phone.Type == PhoneType.Normal)
            {               
                //Place()
                Print("Has place a phone.");
            }
            else //A gold phone.
            {
                TargetPosition toLoadPosition = ConvertGoldIdToTargetPosition(phone.Id);
                //Load()
                Print("Has put back gold phone.");
                box.GoldPhoneChecked = true;
            }
        }

        private void ComboPlaceAPhone(Phone phoneIn, Phone phoneOut)
        {
            ShieldBox box = phoneOut.ShieldBox;
            //Unload and load
            Unlink(phoneOut, box);
            Link(phoneIn,box);
            Print("Has unload and load for box." + box.Id);

            if (phoneIn.Type == PhoneType.Normal)
            {
                //Place()
                Print("Has place a phone.");
            }
            else //A gold phone.
            {
                TargetPosition toLoadPosition = ConvertGoldIdToTargetPosition(phoneIn.Id);
                //Load()
                Print("Has put back gold phone.");
                box.GoldPhoneChecked = true;
            }
        }

        private void BinAPhone(Phone phone)
        {
            ShieldBox box = phone.ShieldBox;
            Unlink(phone, box);
            if (phone.Type == PhoneType.Normal)
            {
                //Unload()
                //Todo need to check if door is open.
                //Bin()
                Print("Has bin a phone.");
            }
            else //A gold phone.
            {
                //Unload
                TargetPosition toLoadPosition = ConvertGoldIdToTargetPosition(phone.Id);
                //Put back.
                Print("Has put back gold phone.");
            }
        }

        private void ComboBinAPhone(Phone phoneIn, Phone phoneOut)
        {
            ShieldBox box = phoneOut.ShieldBox;
            //Unload and load
            Unlink(phoneOut, box);
            Link(phoneIn, box);
            if (phoneIn.Type == PhoneType.Normal)
            {
                //Unload()
                //Todo need to check if door is open.
                //Bin()
                Print("Has bin a phone.");
            }
            else //A gold phone.
            {
                //Unload
                TargetPosition toLoadPosition = ConvertGoldIdToTargetPosition(phoneIn.Id);
                //Put back.
                Print("Has put back gold phone.");
            }
        }

        private void Print(string info)
        {
            Console.WriteLine(info);
        }

        private void PrintStateOfBoxes()
        {
            foreach (var box in ShieldBoxs)
            {
                Console.Write("Box{0} Available:{1} Empty:{2} Checked:{3}", 
                    box.Id, box.Available, box.Empty, box.GoldPhoneChecked);
                if (box.Phone != null)
                {
                    Console.Write(" Phone:{0} Type:{1}", box.Phone.Id, box.Phone.Type);
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
                    foreach (var rPhone in retryPhone)
                    {
                        foreach (var bOpPhone in binOrPlacePhone)
                        {
                            if (bOpPhone.AtBoxType == rPhone.AtBoxType)
                            {
                                rPhone.NextTargetPosition = bOpPhone.CurrentTargetPosition;
                                
                                rPhoneCouple.Add(rPhone);
                                bOpPhoneCouple.Add(bOpPhone); //Order matters.
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
                                    
                                    luckyPhones.Add(pPhone);
                                    luckyPhones.Add(rPhone);
                                    luckyPhones.Add(bOpPhoneCouple.ElementAt(rPhoneCouple.IndexOf(rPhone))); //Order matters.
                                    return luckyPhones; //Find two combo movement.
                                }
                            }
                        }

                        // Can't build unload and load with pick.
                        //So just leave pick alone.
                        
                        luckyPhones.Add(rPhoneCouple.First());
                        luckyPhones.Add(bOpPhoneCouple.First());
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
                                    
                                    luckyPhones.Add(pPhone);
                                    luckyPhones.Add(rPhone);//Order matters.
                                    return luckyPhones;
                                }
                            }
                        }

                        //No combo movement at all.
                        luckyPhones.Add(rPhoneCouple.First());
                        luckyPhones.Add(bOpPhoneCouple.First());
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
                                    
                                    luckyPhones.Add(rPhone);
                                    luckyPhones.Add(bOpPhone); //Order matters.
                                    return luckyPhones;
                                }
                            }
                        }
                        
                        luckyPhones.Add(retryPhone.First());
                        luckyPhones.Add(binOrPlacePhone.First());
                        return luckyPhones;
                    }
                    else
                    {
                        if (pickPhone.Count > 0)
                        {
                            //Can pick and retry.
                            if (MoreThanTwoEmptyBoxes()) //In case no room for retry.
                            {
                                foreach (var pPhone in pickPhone)
                                {
                                    foreach (var rPhone in retryPhone)
                                    {
                                        if (pPhone.AtBoxType == rPhone.AtBoxType)
                                        {
                                            pPhone.NextTargetPosition = rPhone.CurrentTargetPosition;

                                            luckyPhones.Add(pPhone);
                                            luckyPhones.Add(rPhone); //Order matters.
                                            return luckyPhones;
                                        }
                                    }
                                }
                            }

                            //Choose two ramdon phones.
                            // Todo if all phone fail, need to take a phone to buffer?
                            // where is the buffer?
                            luckyPhones.Add(retryPhone.First());
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

                                    luckyPhones.Add(pPhone);
                                    luckyPhones.Add(bOpPhone); //Order matters.
                                    return luckyPhones; //Unload and load.                                  
                                }
                            }
                        }
                        //No UnloadAndLoad Move;
                        luckyPhones.Add(pickPhone.First());
                        luckyPhones.Add(binOrPlacePhone.First());
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
                            try
                            {
                                GetBoxForGoldPhone();
                                luckyPhones.Add(phone);
                                return luckyPhones;
                            }
                            catch (Exception e)
                            {
                                //Console.WriteLine(e);
                                break;
                            }
                        }
                    }

                    //If no gold phone, then a regular phone.
                    foreach (var phone in pickPhone)
                    {
                        if (phone.Type != PhoneType.Golden)
                        {
                            luckyPhones.Add(phone);
                            return luckyPhones;
                        }
                    }

                    luckyPhones.Add(pickPhone.First());
                    return luckyPhones;
                }
            }
        }

        private bool MoreThanTwoEmptyBoxes()
        {
            int count = 0; 
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled & box.Empty)
                {
                    count++;
                }
            }

            return count > 1;
        }

        private ShieldBox GetEmptyBox()
        {
            //Try to find a empty box.
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled & box.Available & box.Empty)
                {
                    return box;
                }
            }

            throw new ShieldBoxNotFoundException("GetEmptyBox fail.");
        }

        /// <summary>
        /// Maybe it's not empty.
        /// </summary>
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

        /// <summary>
        /// A new available box for current phone, but maybe it's not empty.
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        private ShieldBox GetBoxForRetryPhone(Phone phone)
        {
            lock (_availableBoxLocker)
            {
                foreach (var box in ShieldBoxs)
                {
                    var foundBox = true;
                    if (box.Enabled & box.Available)
                    {
                        foreach (var footprint in phone.TargetPositionFootprint)
                        {
                            if (box.Position.TeachPos == footprint.TeachPos)
                            {
                                foundBox = false;
                            }
                        }
                    }
                    else
                    {
                        foundBox = false;
                    }

                    if (foundBox)
                    {
                        return box;
                    }
                }
                throw new ShieldBoxNotFoundException("GetBoxForRetryPhone fail.");
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
