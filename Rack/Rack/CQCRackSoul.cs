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
                            secondPhone.NextTargetPosition.TeachPos == thirdPhone.CurrentTargetPosition.TeachPos )
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
                            //One combo movement.
                            var firstPhone = luckyPhones.First();
                            var secondPhone = luckyPhones.ElementAt(1);
                            if (firstPhone.NextTargetPosition.TeachPos == secondPhone.CurrentTargetPosition.TeachPos)
                            {
                                //Unload();
                                if (firstPhone.ShieldBox!=null) //Inside a box.
                                {
                                    var box1 = firstPhone.ShieldBox;

                                    firstPhone.ShieldBox = null;

                                    box1.Phone = null;
                                    box1.Available = true;
                                    box1.Empty = true;
                                }

                                //UnloadAndLoad();
                                luckyPhones.Remove(firstPhone);

                                var box2 = secondPhone.ShieldBox;

                                firstPhone.ShieldBox = box2;
                                firstPhone.TargetPositionFootprint.Add(box2.Position);
                                firstPhone.CurrentTargetPosition = box2.Position;

                                secondPhone.ShieldBox = null;

                                box2.Phone = firstPhone;
                                box2.Available = false;
                                box2.Empty = false;

                                //Load();
                                luckyPhones.Remove(secondPhone);
                            }
                            else
                            {
                                throw new Exception("Error 549860315484");
                            }
                        }
                        else
                        {
                            //Only one phone at a time.
                            var theOnlyPhone = luckyPhones.First();
                            
                            switch (theOnlyPhone.Procedure)
                            {
                                case RackProcedure.Bin:
                                    #region Bin a phone.
                                    if (theOnlyPhone.Type == PhoneType.Normal)
                                    {
                                        ShieldBox box = theOnlyPhone.ShieldBox;
                                        //Unload
                                        //Bin()
                                        Print("Has bin a phone.");

                                        box.Phone = null;
                                        box.Available = true;
                                        box.Empty = true;
                                    }
                                    else //A gold phone.
                                    {
                                        ShieldBox box = theOnlyPhone.ShieldBox;
                                        //Unload from box.

                                        TargetPosition toLoadPosition = ConvertGoldIdToTargetPosition(theOnlyPhone.Id);
                                        //Load to gold postion.
                                        Print("Has put back gold phone.");

                                        box.Phone = null;
                                        box.Available = true;
                                        box.Empty = true;
                                    }
                                    break; 
                                #endregion

                                case RackProcedure.Place:
                                    #region Place a phone.
                                    if (theOnlyPhone.Type == PhoneType.Normal)
                                    {
                                        ShieldBox box = theOnlyPhone.ShieldBox;
                                        //Unload

                                        //Place()
                                        Print("Has place a phone.");

                                        box.Phone = null;
                                        box.Available = true;
                                        box.Empty = true;
                                    }
                                    else //A gold phone.
                                    {
                                        ShieldBox box = theOnlyPhone.ShieldBox;
                                        //Unload from box.

                                        TargetPosition toLoadPosition = ConvertGoldIdToTargetPosition(theOnlyPhone.Id);
                                        //Load to gold postion.
                                        Print("Has put back gold phone.");

                                        box.Phone = null;
                                        box.Available = true;
                                        box.Empty = true;
                                        box.GoldPhoneChecked = true;
                                    }
                                    break; 
                                #endregion

                                case RackProcedure.Retry:
                                    #region Retry a phone.
                                    //Some retry don't need robot to move the phone.
                                    if (theOnlyPhone.Type == PhoneType.Normal)
                                    {
                                        ShieldBox box = GetBoxForRetryPhone(theOnlyPhone);

                                        //Todo if retry box is not empty, then put it into gold
                                        // buffer, set box to null, set current position to gold position.
                                        // if buffer is not empty, then throw an exception.

                                        //Unload from one box.
                                        theOnlyPhone.ShieldBox.Available = true;
                                        theOnlyPhone.ShieldBox.Empty = true;
                                        theOnlyPhone.ShieldBox.Phone = null;

                                        //Load to other box.
                                        Print("Has retry a phone.");
                                        theOnlyPhone.ShieldBox = box;
                                        theOnlyPhone.TargetPositionFootprint.Add(box.Position);
                                        theOnlyPhone.CurrentTargetPosition = box.Position;

                                        box.Available = false;
                                        box.Empty = false;
                                        box.Phone = theOnlyPhone;
                                    }
                                    else //A gold phone.
                                    {
                                        //Todo Put the phone back to gold position, or retry the test.
                                        //If retry, just need to reclose a box.
                                        ShieldBox box = theOnlyPhone.ShieldBox;
                                        
                                        box.Available = true;
                                        box.Empty = true;
                                        box.Phone = null;
                                    }
                                    break; 
                                #endregion

                                case RackProcedure.Pick:
                                    #region Pick a phone.
                                    if (theOnlyPhone.Type == PhoneType.Normal)
                                    {
                                        //If no box, then do nothing but wait.
                                        ShieldBox box = GetEmptyBox();

                                        if (box.Empty == false)
                                        {
                                            //Should not be just new phone and found a box with phone.
                                            throw new Exception("Error 654478451");
                                        }

                                        //Pick(RackGripper.One);
                                        //Load(RackGripper.One, box.Position);
                                        //Use a task to close box. If error, info user.
                                        Print("Has load a new phone.");

                                        //For the phone.
                                        theOnlyPhone.ShieldBox = box; //Contain info of current position.
                                        theOnlyPhone.TargetPositionFootprint.Add(box.Position); //For retry.
                                        theOnlyPhone.CurrentTargetPosition = box.Position;

                                        //For box.
                                        box.Available = false;
                                        box.Empty = false;
                                        box.Phone = theOnlyPhone;
                                        //After test, shield box will send back result and put phone to serve list.
                                    }
                                    else //A gold phone.
                                    {
                                        ShieldBox box = GetBoxForGoldPhone();

                                        //Unload(RackGripper.One, theOnlyPhone.CurrentTargetPosition);
                                        //Load(RackGripper.One, box.Position);
                                        Print("Has load a gold phone.");

                                        theOnlyPhone.ShieldBox = box;
                                        theOnlyPhone.CurrentTargetPosition = box.Position;

                                        box.Available = false;
                                        box.Empty = false;
                                        box.Phone = theOnlyPhone;
                                    }
                                    
                                    break;
                                #endregion

                                default:
                                    throw new Exception("Error 6417987");
                            }

                            luckyPhones.Remove(theOnlyPhone);
                        }
                    }
                    //
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

        /// <summary>
        /// 
        /// </summary>

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
                if (box.Enabled & box.Empty)
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
        /// Maybe it's not empty.
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
