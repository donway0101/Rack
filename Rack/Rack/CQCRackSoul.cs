using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
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

                List<Phone> luckyPhones = ArrangePhones();
                if (luckyPhones.Count==0)
                    continue;

                RemovePhoneToBeServed(luckyPhones);

                try
                {
                    #region Work on Rf phones.
                    if (luckyPhones.First().Step == RackTestStep.Rf)
                    {
                        if (luckyPhones.Count >= 3)
                        {
                            throw new Exception("Error 16229461984");
                        }
                        else
                        {
                            #region Two phones in a time.
                            if (luckyPhones.Count == 2)
                            {
                                var firstPhone = luckyPhones.First();
                                var secondPhone = luckyPhones.ElementAt(1);
                                RackGripper gripper;
                                if (firstPhone.NextTargetPosition.TeachPos ==
                                    secondPhone.CurrentTargetPosition.TeachPos)
                                {
                                    switch (firstPhone.Procedure)
                                    {
                                        case RackProcedure.Pick:
                                            gripper = GetAvailableGripper();
                                            Pick(gripper);
                                            ShieldBox box = ConverterTeachPosToShieldBox(
                                                firstPhone.NextTargetPosition.TeachPos);
                                            Pick(gripper);
                                            OkToReloadOnConveyor();
                                            break;

                                        case RackProcedure.Retry:
                                            ComboUnload(firstPhone);
                                            break;

                                        default:
                                            throw new Exception("Error 984616941611");
                                    }
                                    luckyPhones.Remove(firstPhone);

                                    switch (secondPhone.Procedure)
                                    {
                                        case RackProcedure.Bin:
                                            ComboUnloadAndLoad(firstPhone, secondPhone, out gripper);
                                            Bin(gripper);
                                            break;
                                           
                                        case RackProcedure.Retry:
                                            ComboUnloadAndLoad(firstPhone, secondPhone, out gripper);

                                            ShieldBox box = ConverterTeachPosToShieldBox(
                                                   secondPhone.NextTargetPosition.TeachPos);
                                            Load(gripper, box);
                                            Link(secondPhone, box);
                                            CloseBoxAsync(box);
                                            break;

                                        default:
                                            throw new Exception("Error 989491878165");
                                    }
                                    luckyPhones.Remove(secondPhone);
                                }
                                else //Arrange error.
                                {
                                    throw new Exception("Error 549860315484");
                                }
                            }
                            #endregion

                            #region Only one Rf phone.
                            else
                            {
                                var phone = luckyPhones.First();
                                switch (phone.Procedure)
                                {
                                    case RackProcedure.Bin:
                                        RackGripper gripper;
                                        ComboUnload(phone, out gripper);
                                        Bin(gripper);
                                        break;

                                    case RackProcedure.Place:
                                        phone.Step = RackTestStep.Wifi;
                                        //Set result will trigger add phone to server.
                                        phone.TestResult = TestResult.None;
                                        phone.FailCount = 0;
                                        break;

                                    case RackProcedure.Retry:
                                        MoveFromCurrentBoxToNext(phone);
                                        break;

                                    case RackProcedure.Pick:
                                        ServeNewRfPhone(phone);
                                        break;
                                    default:
                                        throw new Exception("Error 6417989416269");
                                }

                                luckyPhones.Remove(phone);
                            }
                            #endregion
                        }
                    }
                    #endregion

                    else
                    {
                        #region Work on wifi phones.
                        if (luckyPhones.First().Step == RackTestStep.Wifi)
                        {
                            if (luckyPhones.Count > 3)
                            {
                                throw new Exception("Error 4984639789151");
                            }

                            #region Three wifi phones in a time.
                            if (luckyPhones.Count == 3)
                            {
                                //If there is three phones, it has two combo move, 
                                // and the last phone must either be place or bin.
                                var firstPhone = luckyPhones.First();
                                var secondPhone = luckyPhones.ElementAt(1);
                                var thirdPhone = luckyPhones.ElementAt(2);
                                RackGripper gripper;
                                if (firstPhone.NextTargetPosition.TeachPos == secondPhone.CurrentTargetPosition.TeachPos &&
                                    secondPhone.NextTargetPosition.TeachPos == thirdPhone.CurrentTargetPosition.TeachPos)
                                {
                                    if (firstPhone.Procedure!= RackProcedure.Pick)
                                    {
                                        throw new Exception("Error 445668138754");
                                    }
                                    if (secondPhone.Procedure != RackProcedure.Retry)
                                    {
                                        throw new Exception("Error 456789123584641");
                                    }
                                    if (thirdPhone.Procedure != RackProcedure.Place && 
                                        thirdPhone.Procedure != RackProcedure.Bin)
                                    {
                                        throw new Exception("Error 456984616761654");
                                    }

                                    ComboUnload(firstPhone);
                                    luckyPhones.Remove(firstPhone);
                                    
                                    ComboUnloadAndLoad(firstPhone, secondPhone, out gripper);
                                    luckyPhones.Remove(secondPhone);

                                    switch (thirdPhone.Procedure)
                                    {
                                        case RackProcedure.Bin:
                                            Bin(gripper);
                                            break;
                                        case RackProcedure.Place:
                                            Place(gripper);
                                            break;
                                        default:
                                            break;
                                    }
                                    luckyPhones.Remove(thirdPhone);
                                }
                                else //Arrange error.
                                {
                                    throw new Exception("Error 16229461984");
                                }
                            }
                            #endregion

                            else
                            {
                                #region Two wifi phones in a time.
                                if (luckyPhones.Count == 2)
                                {
                                    var firstPhone = luckyPhones.First();
                                    var secondPhone = luckyPhones.ElementAt(1);
                                    RackGripper gripper;
                                    if (firstPhone.NextTargetPosition.TeachPos ==
                                        secondPhone.CurrentTargetPosition.TeachPos)
                                    {
                                        switch (firstPhone.Procedure)
                                        {
                                            case RackProcedure.Pick:
                                                ComboUnload(firstPhone);
                                                break;

                                            case RackProcedure.Retry:
                                                ComboUnload(firstPhone);
                                                break;      
                                                
                                            default:
                                                throw new Exception("Error 984616941611");
                                        }
                                        luckyPhones.Remove(firstPhone);

                                        switch (secondPhone.Procedure)
                                        {
                                            case RackProcedure.Bin:
                                                ComboUnloadAndLoad(firstPhone, secondPhone, out gripper);
                                                Bin(gripper);
                                                break;

                                            case RackProcedure.Place:
                                                ComboUnloadAndLoad(firstPhone, secondPhone, out gripper);
                                                Place(gripper);
                                                break;

                                            case RackProcedure.Retry:
                                                ComboUnloadAndLoad(firstPhone, secondPhone, out gripper);

                                                ShieldBox box = ConverterTeachPosToShieldBox(
                                                    secondPhone.NextTargetPosition.TeachPos);
                                                Load(gripper, box);
                                                Link(secondPhone, box);
                                                CloseBoxAsync(box);
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
                                #endregion

                                #region Only one wifi phone in a time.
                                else
                                {
                                    var phone = luckyPhones.First();
                                    switch (phone.Procedure)
                                    {
                                        case RackProcedure.Bin:
                                            BinOrPlace(phone);
                                            break;

                                        case RackProcedure.Place:
                                            BinOrPlace(phone);
                                            break;

                                        case RackProcedure.Retry:
                                            //Arrange algorithm already find a box for retry phone.
                                            MoveFromCurrentBoxToNext(phone);
                                            break;

                                        case RackProcedure.Pick:
                                            //Wifi pick phone comes from Rf pass phone.
                                            MoveFromCurrentBoxToNext(phone);
                                            break;

                                        default:
                                            throw new Exception("Error 846164946151679"); ;
                                    }

                                    luckyPhones.Remove(phone);                                    
                                }
                                #endregion
                            }
                        } 
                        #endregion
                    }
                                     
                    //Todo comment out.
                    PrintStateOfBoxes();
                }

                #region Exception
                catch (ShieldBoxNotFoundException e)
                {
                    //Todo error code
                    OnInfoOccured(0, e.Message);
                    //PhoneServerManualResetEvent.Reset();
                }
                catch (Exception e)
                {
                    OnErrorOccured(44444444, e.Message);
                    PhoneServerManualResetEvent.Reset();
                }
                finally
                {
                    if (luckyPhones.Count > 0)
                    {
                        RecyclePhones(luckyPhones);
                    }
                } 
                #endregion
            }
        }

        private void ClassifyPhones()
        {
            lock (_phoneToBeServedLocker)
            {
                RfPhones.Clear();
                WifiPhones.Clear();
                if (PhoneToBeServed.Count > 0)
                {
                    foreach (var phone in PhoneToBeServed)
                    {
                        if (phone.Step == RackTestStep.Rf)
                        {
                            RfPhones.Add(phone);
                        }

                        if (phone.Step == RackTestStep.Wifi)
                        {
                            WifiPhones.Add(phone);
                        }
                    }
                }
            }
        }

        private List<Phone> ArrangePhones()
        {
            ClassifyPhones();
            List<Phone> phones = new List<Phone>();

            //Load to Rf first, Rf pass is new phone to wifi.
            if (RfPhones.Count > 0)
            {
                phones = ArrangeRfPhones();
                if (phones.Count > 0)
                    return phones;
            }

            if (WifiPhones.Count>0)
            {
                return ArrangeWifiPhones();
            }
            else
            {
                return phones;
            }           
        }

        private List<Phone> ArrangeWifiPhones()
        {
            #region Define.
            int maxFailCount = 0;
            if (WifiTestMode == RackTestMode.AB)
            {
                maxFailCount = 2;
            }

            List<Phone> wifiLuckyPhones = new List<Phone>();
            List<Phone> wifiBinOrPlacePhone = new List<Phone>();
            List<Phone> wifiRetryPhone = new List<Phone>();
            List<Phone> wifiPickPhone = new List<Phone>(); 
            #endregion

            #region Classify phones by test result.
            foreach (var phone in WifiPhones)
            {
                if (phone.FailCount >= maxFailCount)
                {
                    phone.NextTargetPosition = Motion.BinPosition;
                    phone.Procedure = RackProcedure.Bin;
                    wifiBinOrPlacePhone.Add(phone);
                }
                else
                {
                    if (phone.TestResult == TestResult.Pass)
                    {
                        phone.NextTargetPosition = Motion.PickPosition;
                        phone.Procedure = RackProcedure.Place;
                        wifiBinOrPlacePhone.Add(phone);
                    }
                    else
                    {
                        if (phone.TestResult == TestResult.Fail)
                        {
                            phone.Procedure = RackProcedure.Retry;
                            wifiRetryPhone.Add(phone);
                        }
                        else
                        {
                            // (phone.TestResult == TestResult.None)
                            phone.Procedure = RackProcedure.Pick;
                            wifiPickPhone.Add(phone);
                        }
                    }
                }
            } 
            #endregion

            #region Has retry, maybe place, maybe pick.
            if (wifiRetryPhone.Count > 0)
            {
                #region Has place, has retry, has pick.
                if (wifiBinOrPlacePhone.Count > 0 && wifiPickPhone.Count > 0)
                {
                    #region Try combining retry with place(See also Has retry and place).
                    List<ShieldBox> boxesForRetryPhone = new List<ShieldBox>();
                    ShieldBox boxOfBinOrPlace;
                    for (int i = 0; i < wifiBinOrPlacePhone.Count; i++)
                    {
                        boxOfBinOrPlace = wifiBinOrPlacePhone.ElementAt(i).ShieldBox;
                        for (int j = 0; j < wifiRetryPhone.Count; j++)
                        {
                            try
                            {
                                boxesForRetryPhone = GetBoxesForRetryPhone(
                                    wifiRetryPhone.ElementAt(j), WifiTestMode, ShieldBoxType.Wifi);
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                            foreach (var box in boxesForRetryPhone)
                            {
                                if (box.Position.TeachPos == boxOfBinOrPlace.Position.TeachPos)
                                {
                                    var pPhone = wifiPickPhone.First();
                                    pPhone.NextTargetPosition = wifiRetryPhone.ElementAt(j).CurrentTargetPosition;
                                    wifiRetryPhone.ElementAt(j).NextTargetPosition =
                                        boxOfBinOrPlace.Position;

                                    wifiLuckyPhones.Add(pPhone);
                                    wifiLuckyPhones.Add(wifiRetryPhone.ElementAt(j));
                                    wifiLuckyPhones.Add(wifiBinOrPlacePhone.ElementAt(i));
                                    return wifiLuckyPhones;
                                }
                            }
                        }
                    }

                    //If retry and place can't be combo, then place first.
                    wifiLuckyPhones.Clear();
                    wifiLuckyPhones.Add(wifiBinOrPlacePhone.First());
                    return wifiLuckyPhones;
                    #endregion
                }
                #endregion

                #region Has retry, maybe place, maybe pick.
                else
                {
                    #region Has retry, has place, maybe pick.                    
                    if (wifiBinOrPlacePhone.Count > 0)
                    {
                        #region Try combining retry with place(See also Have place retry and pick).
                        List<ShieldBox> boxesForRetryPhone = new List<ShieldBox>();
                        ShieldBox boxOfBinOrPlace;
                        for (int i = 0; i < wifiBinOrPlacePhone.Count; i++)
                        {
                            boxOfBinOrPlace = wifiBinOrPlacePhone.ElementAt(i).ShieldBox;
                            for (int j = 0; j < wifiRetryPhone.Count; j++)
                            {
                                try
                                {
                                    boxesForRetryPhone = GetBoxesForRetryPhone(
                                        wifiRetryPhone.ElementAt(j), WifiTestMode, ShieldBoxType.Wifi);
                                }
                                catch (Exception)
                                {
                                    continue;
                                }

                                foreach (var box in boxesForRetryPhone)
                                {
                                    if (box.Position.TeachPos == boxOfBinOrPlace.Position.TeachPos)
                                    {
                                        wifiRetryPhone.ElementAt(j).NextTargetPosition =
                                            boxOfBinOrPlace.Position;
                                        wifiLuckyPhones.Add(wifiRetryPhone.ElementAt(j));
                                        wifiLuckyPhones.Add(wifiBinOrPlacePhone.ElementAt(i));
                                        return wifiLuckyPhones;
                                    }
                                }
                            }
                        }

                        //If retry and place can't be combo, then place first.
                        wifiLuckyPhones.Clear();
                        wifiLuckyPhones.Add(wifiBinOrPlacePhone.First());
                        return wifiLuckyPhones;
                        #endregion
                    }
                    #endregion

                    #region Has retry, maybe pick, no place.
                    else
                    {
                        #region More than one retry phones, maybe pick, no place.
                        if (wifiRetryPhone.Count > 1)
                        {
                            List<ShieldBox> boxesForPhone1 = new List<ShieldBox>();
                            List<ShieldBox> boxesForPhone2 = new List<ShieldBox>();

                            for (int i = 0; i < wifiRetryPhone.Count - 1; i++)
                            {
                                #region Try finding box for a phone.
                                try
                                {
                                    boxesForPhone1 = GetBoxesForRetryPhone(
                                        wifiRetryPhone.ElementAt(i), WifiTestMode, ShieldBoxType.Wifi);
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                                #endregion

                                for (int j = i + 1; j < wifiRetryPhone.Count; j++)
                                {
                                    #region Try finding box for another phone.
                                    try
                                    {
                                        boxesForPhone2 = GetBoxesForRetryPhone(
                                            wifiRetryPhone.ElementAt(j), WifiTestMode, ShieldBoxType.Wifi);
                                    }
                                    catch (Exception)
                                    {
                                        continue;
                                    }
                                    #endregion

                                    #region See if they match.
                                    foreach (var box1 in boxesForPhone1)
                                    {
                                        foreach (var box2 in boxesForPhone2)
                                        {
                                            //Two phone can exchange box.
                                            if (box1.Position.TeachPos == wifiRetryPhone.ElementAt(j)
                                                    .ShieldBox
                                                    .Position.TeachPos &&
                                                box2.Position.TeachPos == wifiRetryPhone.ElementAt(i)
                                                    .ShieldBox
                                                    .Position.TeachPos)
                                            {
                                                wifiRetryPhone.ElementAt(i).NextTargetPosition =
                                                    wifiRetryPhone.ElementAt(j).CurrentTargetPosition;
                                                wifiRetryPhone.ElementAt(j).NextTargetPosition =
                                                    wifiRetryPhone.ElementAt(i).CurrentTargetPosition;

                                                wifiLuckyPhones.Add(wifiRetryPhone.ElementAt(i));
                                                wifiLuckyPhones.Add(wifiRetryPhone.ElementAt(j));
                                                return wifiLuckyPhones;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }

                            #region No combo retry, just find box for a single retry.
                            foreach (var rPhone in wifiRetryPhone)
                            {
                                try
                                {
                                    List<ShieldBox> box = GetBoxesForRetryPhone(
                                        rPhone, WifiTestMode, ShieldBoxType.Wifi, true);
                                    rPhone.NextTargetPosition = box.First().Position;
                                    wifiLuckyPhones.Add(rPhone);
                                    return wifiLuckyPhones;
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                            }
                            #endregion

                            //Return empty list.
                            return wifiLuckyPhones;
                        }
                        #endregion

                        #region Only one retry phone, maybe pick, no place.
                        else
                        {
                            try
                            {
                                var phone = wifiRetryPhone.First();

                                List<ShieldBox> box = GetBoxesForRetryPhone(
                                    phone, WifiTestMode, ShieldBoxType.Wifi, true);
                                phone.NextTargetPosition = box.First().Position;

                                if (wifiPickPhone.Count > 0)
                                {
                                    var pPhone = wifiPickPhone.First();
                                    pPhone.NextTargetPosition = phone.CurrentTargetPosition;
                                    wifiLuckyPhones.Add(pPhone);
                                }

                                wifiLuckyPhones.Add(phone);
                                return wifiLuckyPhones;
                            }
                            catch (Exception)
                            {
                                return wifiLuckyPhones;
                            }
                        }
                        #endregion
                    }
                    #endregion
                } 
                #endregion
            }
            #endregion

            #region No retry, maybe place, maybe pick.
            else
            {
                #region No retry, has place, maybe pick.
                if (wifiBinOrPlacePhone.Count > 0)
                {
                    #region No retry, Have place and pick.
                    if (wifiPickPhone.Count > 0)
                    {
                        var pPhone = wifiPickPhone.First();
                        var bOpPhone = wifiBinOrPlacePhone.First();
                        pPhone.NextTargetPosition = bOpPhone.CurrentTargetPosition;
                        wifiLuckyPhones.Add(pPhone);
                        wifiLuckyPhones.Add(bOpPhone);
                        return wifiLuckyPhones;
                    }
                    #endregion

                    #region No retry, no pick, has place.
                    else
                    {
                        var phone = wifiBinOrPlacePhone.First();
                        wifiLuckyPhones.Add(phone);
                        return wifiLuckyPhones;
                    } 
                    #endregion
                }
                #endregion

                #region No retry, no place, has pick.
                else
                {
                    try
                    {
                        //If just one pick phone shows, it can only go into empty box.
                        ShieldBox box = GetEmptyBox(ShieldBoxType.Wifi);
                        var phone = wifiPickPhone.First();
                        phone.NextTargetPosition = box.Position;
                        wifiLuckyPhones.Add(phone);
                        return wifiLuckyPhones;
                    }
                    catch (Exception)
                    {
                        return wifiLuckyPhones;
                    }
                }
                #endregion
            } 
            #endregion
        }

        private List<Phone> ArrangeRfPhones()
        {
            #region Define
            int maxFailCount = 1;
            if (RfTestMode == RackTestMode.ABC)
            {
                maxFailCount = 3;
            }

            List<Phone> rfLuckyPhones = new List<Phone>();
            List<Phone> rfPlacePhone = new List<Phone>();
            List<Phone> rfBinPhone = new List<Phone>();
            List<Phone> rfRetryPhone = new List<Phone>();
            List<Phone> rfPickPhone = new List<Phone>(); 
            #endregion

            #region Classify phones by test result.
            foreach (var phone in RfPhones)
            {
                if (phone.FailCount >= maxFailCount)
                {
                    phone.NextTargetPosition = Motion.BinPosition;
                    phone.Procedure = RackProcedure.Bin;
                    rfBinPhone.Add(phone);
                }
                else
                {
                    if (phone.TestResult == TestResult.Pass)
                    {
                        phone.NextTargetPosition = Motion.PickPosition;
                        phone.Procedure = RackProcedure.Place;
                        rfPlacePhone.Add(phone);
                    }
                    else
                    {
                        if (phone.TestResult == TestResult.Fail)
                        {
                            phone.Procedure = RackProcedure.Retry;
                            rfRetryPhone.Add(phone);
                        }
                        else
                        {
                            //Phone to pick or it's gold.
                            if (phone.TestResult == TestResult.None)
                            {
                                phone.Procedure = RackProcedure.Pick;
                                rfPickPhone.Add(phone);
                            }
                            else
                            {
                                throw new Exception("Error 8496134989848");
                            }
                        }
                    }
                }
            }
            #endregion

            #region Has place, maybe pick, maybe retry.
            if (rfPlacePhone.Count > 0)
            {
                rfLuckyPhones.Add(rfPlacePhone.First());
                return rfLuckyPhones;
            }
            #endregion

            #region Has retry, maybe bin, maybe pick.
            if (rfRetryPhone.Count > 0)
            {
                #region Have bin retry and pick.
                if (rfBinPhone.Count > 0 && rfPickPhone.Count > 0)
                {
                    #region Try combining retry with place.
                    List<ShieldBox> boxesForRetryPhone = new List<ShieldBox>();
                    ShieldBox boxOfBin;
                    for (int i = 0; i < rfBinPhone.Count; i++)
                    {
                        boxOfBin = rfBinPhone.ElementAt(i).ShieldBox;
                        for (int j = 0; j < rfRetryPhone.Count; j++)
                        {
                            try
                            {
                                boxesForRetryPhone = GetBoxesForRetryPhone(
                                    rfRetryPhone.ElementAt(j), RfTestMode, ShieldBoxType.Rf);
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                            foreach (var box in boxesForRetryPhone)
                            {
                                if (box.Position.TeachPos == boxOfBin.Position.TeachPos)
                                {
                                    var pPhone = rfPickPhone.First();
                                    pPhone.NextTargetPosition = rfRetryPhone.ElementAt(j).CurrentTargetPosition;
                                    rfRetryPhone.ElementAt(j).NextTargetPosition =
                                        boxOfBin.Position;

                                    rfLuckyPhones.Add(pPhone);
                                    rfLuckyPhones.Add(rfRetryPhone.ElementAt(j));
                                    rfLuckyPhones.Add(rfBinPhone.ElementAt(i));
                                    return rfLuckyPhones;
                                }
                            }
                        }
                    }

                    //If retry and bin can't be combo, then bin first.
                    rfLuckyPhones.Clear();
                    rfLuckyPhones.Add(rfBinPhone.First());
                    return rfLuckyPhones;
                    #endregion
                }
                #endregion

                #region Has retry, maybe bin, maybe pick.
                else
                {
                    #region Has retry, has bin, maybe pick.                    
                    if (rfBinPhone.Count > 0)
                    {
                        #region Try combining retry with bin.
                        List<ShieldBox> boxesForRetryPhone = new List<ShieldBox>();
                        ShieldBox boxOfBin;
                        for (int i = 0; i < rfBinPhone.Count; i++)
                        {
                            boxOfBin = rfBinPhone.ElementAt(i).ShieldBox;
                            for (int j = 0; j < rfRetryPhone.Count; j++)
                            {
                                #region Try finding box for retry.
                                try
                                {
                                    boxesForRetryPhone = GetBoxesForRetryPhone(
                                        rfRetryPhone.ElementAt(j), RfTestMode, ShieldBoxType.Rf);
                                }
                                catch (Exception)
                                {
                                    continue;
                                } 
                                #endregion

                                foreach (var box in boxesForRetryPhone)
                                {
                                    if (box.Position.TeachPos == boxOfBin.Position.TeachPos)
                                    {
                                        rfRetryPhone.ElementAt(j).NextTargetPosition =
                                            boxOfBin.Position;
                                        rfLuckyPhones.Add(rfRetryPhone.ElementAt(j));
                                        rfLuckyPhones.Add(rfBinPhone.ElementAt(i));
                                        return rfLuckyPhones;
                                    }
                                }
                            }
                        }

                        //If retry and bin can't be combo, then bin first.
                        rfLuckyPhones.Clear();
                        rfLuckyPhones.Add(rfBinPhone.First());
                        return rfLuckyPhones;
                        #endregion
                    }
                    #endregion

                    #region Have retry but no bin.
                    else
                    {
                        #region More than one retry phones.
                        if (rfRetryPhone.Count > 1)
                        {
                            List<ShieldBox> boxesForPhone1 = new List<ShieldBox>();
                            List<ShieldBox> boxesForPhone2 = new List<ShieldBox>();

                            for (int i = 0; i < rfRetryPhone.Count - 1; i++)
                            {
                                #region Try finding box for a retry phone.
                                try
                                {
                                    boxesForPhone1 = GetBoxesForRetryPhone(
                                        rfRetryPhone.ElementAt(i), RfTestMode, ShieldBoxType.Rf);
                                }
                                catch (Exception)
                                {
                                    continue;
                                } 
                                #endregion

                                for (int j = i + 1; j < rfRetryPhone.Count; j++)
                                {
                                    #region Try finding box for another retry phone.
                                    try
                                    {
                                        boxesForPhone2 = GetBoxesForRetryPhone(
                                            rfRetryPhone.ElementAt(j), RfTestMode, ShieldBoxType.Rf);
                                    }
                                    catch (Exception)
                                    {
                                        continue;
                                    }
                                    #endregion

                                    #region See if they match.
                                    foreach (var box1 in boxesForPhone1)
                                    {
                                        foreach (var box2 in boxesForPhone2)
                                        {
                                            if (box1.Position.TeachPos == rfRetryPhone.ElementAt(j)
                                                    .ShieldBox
                                                    .Position.TeachPos &&
                                                box2.Position.TeachPos == rfRetryPhone.ElementAt(i)
                                                    .ShieldBox
                                                    .Position.TeachPos)
                                            {
                                                rfRetryPhone.ElementAt(i).NextTargetPosition =
                                                    rfRetryPhone.ElementAt(j).CurrentTargetPosition;
                                                rfRetryPhone.ElementAt(j).NextTargetPosition =
                                                    rfRetryPhone.ElementAt(i).CurrentTargetPosition;

                                                rfLuckyPhones.Add(rfRetryPhone.ElementAt(i));
                                                rfLuckyPhones.Add(rfRetryPhone.ElementAt(j));
                                                return rfLuckyPhones;
                                            }
                                        }
                                    } 
                                    #endregion
                                }
                            }

                            #region No combo for retry phone, go solo.
                            foreach (var rPhone in rfRetryPhone)
                            {
                                try
                                {
                                    List<ShieldBox> box = GetBoxesForRetryPhone(
                                        rPhone, RfTestMode, ShieldBoxType.Rf, true);
                                    rPhone.NextTargetPosition = box.First().Position;
                                    rfLuckyPhones.Add(rPhone);
                                    return rfLuckyPhones;
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                            }
                            #endregion

                            //Return empty list.
                            return rfLuckyPhones;
                        }
                        #endregion

                        #region Just one retry, maybe pick, no bin.
                        else
                        {                            
                            try
                            {
                                var rPhone = rfRetryPhone.First();
                                List<ShieldBox> box = GetBoxesForRetryPhone(
                                    rPhone, RfTestMode, ShieldBoxType.Rf, true);
                                rPhone.NextTargetPosition = box.First().Position;

                                if (rfPickPhone.Count > 0)
                                {
                                    Phone pPhone = rfPickPhone.First();
                                    pPhone.NextTargetPosition = rPhone.CurrentTargetPosition;
                                    rfLuckyPhones.Add(pPhone);
                                }
                                rfLuckyPhones.Add(rPhone);
                                return rfLuckyPhones;
                            }
                            catch (Exception)
                            {
                                return rfLuckyPhones;
                            }
                        } 
                        #endregion                       
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region No retry, maybe bin, maybe pick.
            else
            {
                #region No retry, has bin, maybe pick.
                if (rfBinPhone.Count > 0)
                {
                    #region No retry, Have bin and pick.
                    if (rfPickPhone.Count > 0)
                    {
                        var pPhone = rfPickPhone.First();
                        var bPhone = rfBinPhone.First();
                        pPhone.NextTargetPosition = bPhone.CurrentTargetPosition;
                        rfLuckyPhones.Add(pPhone);
                        rfLuckyPhones.Add(bPhone);
                        return rfLuckyPhones;
                    }
                    #endregion

                    #region No retry, no pick, has bin.
                    else
                    {
                        var phone = rfBinPhone.First();
                        rfLuckyPhones.Add(phone);
                        return rfLuckyPhones;
                    }
                    #endregion
                }

                #endregion

                #region No retry, no bin, has pick.
                else
                {
                    #region Pick Regular phone.
                    try
                    {
                        ShieldBox box = GetEmptyBox(ShieldBoxType.Rf);
                        var phone = rfPickPhone.First();
                        phone.NextTargetPosition = box.Position;
                        rfLuckyPhones.Add(phone);
                        return rfLuckyPhones;
                    }
                    catch (Exception)
                    {
                        return rfLuckyPhones;
                    }
                    #endregion
                }
                #endregion
            }
            #endregion
        }

        private void ComboUnloadAndLoad(Phone phoneIn, Phone phoneOut, out RackGripper gripper)
        {
            ShieldBox box = phoneOut.ShieldBox;
            gripper = GetAvailableGripper();
            UnloadAndLoad(box, gripper);
            Unlink(phoneOut);
            Link(phoneIn, box);
            CloseBoxAsync(box);
        }

        private void ComboUnload(Phone phone)
        {
            RackGripper gripper = GetAvailableGripper();
            Unload(gripper, phone);
            Unlink(phone);
        }

        private void ComboUnload(Phone phone, out RackGripper gripper)
        {
            gripper = GetAvailableGripper();
            Unload(gripper, phone);
            Unlink(phone);
        }

        /// <summary>
        /// For one phone situation.
        /// </summary>
        /// <param name="phone"></param>
        private void BinOrPlace(Phone phone)
        {
            RackGripper gripper = GetAvailableGripper();
            Unload(gripper, phone);
            Unlink(phone);
            switch (phone.Procedure)
            {
                case RackProcedure.Bin:
                    Bin(gripper);
                    break;
                case RackProcedure.Place:
                    Place(gripper);
                    break;
                default:
                    break;
            }           
        }

        private void MoveFromCurrentBoxToNext(Phone phone)
        {
            RackGripper gripper = GetAvailableGripper();
            ShieldBox nextBox = ConverterTeachPosToShieldBox(phone.NextTargetPosition.TeachPos);            
            Unload(gripper, phone);
            Unlink(phone);
            Load(gripper, nextBox);
            Link(phone, nextBox);
            CloseBoxAsync(nextBox);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// Pre-link box with phone, so conveyor can reload a new phone,
        ///  if error happen during deliver from conveyor to box, then unlink box with phone.
        private void ServeNewRfPhone(Phone phone)
        {
            ShieldBox box = ConverterTeachPosToShieldBox(phone.NextTargetPosition.TeachPos);
            try
            {
                RackGripper gripper = GetAvailableGripper();
                Pick(gripper);
                Link(phone, box);
                OkToReloadOnConveyor();
                Load(gripper, box);
                CloseBoxAsync(box);
            }
            catch (Exception)
            {
                Unlink(phone, box);
                throw;
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

        private void Unlink(Phone phone, ShieldBox box)
        {
            phone.ShieldBox = null;
            box.Available = true;
            box.Empty = true;
            box.Phone = null;
        }

        /// <summary>
        /// Unlink phone with its box.
        /// </summary>
        /// <param name="phone"></param>
        private void Unlink(Phone phone)
        {
            ShieldBox box = phone.ShieldBox;
            phone.ShieldBox = null;
            box.Available = true;
            box.Empty = true;
            box.Phone = null;
        }

        private void PrintStateOfBoxes()
        {
            foreach (var box in ShieldBoxs)
            {
                Console.Write("Box{0} Available:{1} Empty:{2} Checked:{3}", 
                    box.Id, box.Available, box.Empty, box.GoldPhoneChecked);
                if (box.Phone != null)
                {
                    Console.Write(" Phone:{0} Step:{1} Box:{2}", box.Phone.Id, box.Phone.Step, box.Phone.ShieldBox.Id);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
       
        private ShieldBox GetEmptyBox(ShieldBoxType type)
        {
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled && box.Empty && box.Type == type)
                {
                    return box;
                }
            }

            throw new ShieldBoxNotFoundException("GetEmptyBox " + type + " fail.");
        }      
        
        /// <summary>
        /// Base on test mode.
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="testMode"></param>
        /// <param name="type"></param>
        /// <param name="needEmpty"></param>
        /// <returns></returns>
        private List<ShieldBox> GetBoxesForRetryPhone(
            Phone phone, RackTestMode testMode, ShieldBoxType type, bool needEmpty=false)
        {
            switch (testMode)
            {
                case RackTestMode.AB:
                    #region AB mode
                    List<ShieldBox> retryBoxs = new List<ShieldBox>();
                    lock (_availableBoxLocker)
                    {
                        foreach (var box in ShieldBoxs)
                        {
                            var foundBox = true;
                            if (box.Enabled && box.Available && box.Type == type)
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

                            if (needEmpty && box.Empty==false)
                            {
                                foundBox = false;
                            }

                            if (foundBox)
                            {
                                retryBoxs.Add(box);
                            }
                        }
                    }

                    if (retryBoxs.Count == 0)
                    {
                        throw new ShieldBoxNotFoundException("GetBoxesForWifiRetryPhone failed.");
                    }

                    return retryBoxs; 
                    #endregion

                case RackTestMode.AAB:
                    throw new Exception("Error 49841634791.");
                case RackTestMode.ABC:
                    throw new Exception("Error 49841634791.");
                case RackTestMode.ABA:
                    throw new Exception("Error 49841634791.");
                default:
                    throw new Exception("Error 49841634791.");
            }
        }

        private bool HasNoPhoneToBeServed()
        {
            lock (_phoneToBeServedLocker)
            {
                return PhoneToBeServed.Count == 0;
            }
        }

        private void RecyclePhones(IEnumerable<Phone> phones)
        {
            lock (_phoneToBeServedLocker)
            {
                PhoneToBeServed.AddRange(phones);
            }
        }

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
                if (PhoneToBeServed.Count>0)
                {
                    foreach (var pho in PhoneToBeServed)
                    {
                        if (pho.Id == phone.Id)
                        {
                            return;
                        }
                    }
                }
                PhoneToBeServed.Add(phone);                
            }
        }

        private void Phone_TestComplete(object sender)
        {
            Phone phone = (Phone)sender;
            AddPhoneToBeServed(phone);
        }

        private void RemovePhoneToBeServed(Phone phone)
        {
            lock (_phoneToBeServedLocker)
            {
                PhoneToBeServed.Remove(phone);
            }
        }
        
    }
}
