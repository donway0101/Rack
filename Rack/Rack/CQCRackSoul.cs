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
                                //Step 1: pick the first phone.
                                ComboPickAPhone(firstPhone);
                                luckyPhones.Remove(firstPhone);

                                //Step 2: unload the second phone and load the first phone.
                                TurboComboRetryAPhone(firstPhone, secondPhone);
                                luckyPhones.Remove(secondPhone);

                                //Step 3: unload the third phone and load the second phone.
                                switch (thirdPhone.Procedure)
                                {
                                    case RackProcedure.Bin:
                                        TurboComboBinAPhone(secondPhone, thirdPhone);
                                        break;
                                    case RackProcedure.Place:
                                        TurboComboPlaceAPhone(secondPhone, thirdPhone);
                                        break;
                                    default:
                                        throw new Exception("Error 48862255792365");
                                }
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
                                            ComboRetryAPhone(firstPhone);
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
                                            ComboRetryAPhone(firstPhone, secondPhone);
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
                                        ServePassRfPhone(theOnlyPhone);
                                        break;
                                    case RackProcedure.Retry:
                                        RetryAPhone(theOnlyPhone);
                                        break;
                                    case RackProcedure.Pick:
                                        ServeNewRfPhone(theOnlyPhone);
                                        break;
                                    default:
                                        throw new Exception("Error 6417987");
                                }

                                luckyPhones.Remove(theOnlyPhone);
                            }
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

                            #region Three phones in a time.
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
                                    //Step 1: pick the first phone.
                                    ComboPickAPhone(firstPhone);
                                    luckyPhones.Remove(firstPhone);

                                    //Step 2: unload the second phone and load the first phone.
                                    TurboComboRetryAPhone(firstPhone, secondPhone);
                                    luckyPhones.Remove(secondPhone);

                                    //Step 3: unload the third phone and load the second phone.
                                    switch (thirdPhone.Procedure)
                                    {
                                        case RackProcedure.Bin:
                                            TurboComboBinAPhone(secondPhone, thirdPhone);
                                            break;
                                        case RackProcedure.Place:
                                            TurboComboPlaceAPhone(secondPhone, thirdPhone);
                                            break;
                                        default:
                                            throw new Exception("Error 48862255792365");
                                    }
                                    luckyPhones.Remove(thirdPhone);
                                }
                                else
                                {
                                    throw new Exception("Error 16229461984");
                                }
                            }

                            #endregion
                            else
                            {
                                #region Two phones in a time.
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
                                                ComboRetryAPhone(firstPhone);
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
                                                ComboRetryAPhone(firstPhone, secondPhone);
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

                                #region Only one phone in a time.
                                else
                                {
                                    var theOnlyPhone = luckyPhones.First();
                                    switch (theOnlyPhone.Procedure)
                                    {
                                        case RackProcedure.Bin:
                                            ServeBinWifiPhone(theOnlyPhone);
                                            break;
                                        case RackProcedure.Place:
                                            ServePassWifiPhone(theOnlyPhone);
                                            break;
                                        case RackProcedure.Retry:
                                            ServeRetryWifiPhone(theOnlyPhone);
                                            break;
                                        case RackProcedure.Pick:
                                            ServeNewWifiPhone(theOnlyPhone);
                                            break;
                                        default:
                                            throw new Exception("Error 846164946151679"); ;
                                    }

                                    luckyPhones.Remove(theOnlyPhone); 
                                    
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

            //Load to Rf first, Rf pass is new phone to wifi.
            if (RfPhones.Count > 0)
            {
                List<Phone> phones = ArrangeRfPhones();
                if (phones.Count > 0)
                    return phones;
            }

            return ArrangeWifiPhones();
        }

        /// <summary>
        /// Phones which Step is in Wifi.
        /// </summary>
        /// <returns></returns>
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
                #region Have place retry and pick.
                if (wifiBinOrPlacePhone.Count > 0 && wifiPickPhone.Count > 0)
                {
                    #region Try combining retry with place.
                    bool retryAndPlaceCombo = false;
                    List<ShieldBox> boxesForRetryPhone = new List<ShieldBox>();
                    ShieldBox boxOfBinOrPlace;

                    wifiLuckyPhones.Add(wifiPickPhone.First());
                    for (int i = 0; i < wifiBinOrPlacePhone.Count; i++)
                    {
                        boxOfBinOrPlace = wifiBinOrPlacePhone.ElementAt(i).ShieldBox;
                        for (int j = 0; j < wifiRetryPhone.Count; j++)
                        {
                            try
                            {
                                boxesForRetryPhone = GetBoxesForWifiRetryPhone(wifiRetryPhone.ElementAt(j));
                                foreach (var box in boxesForRetryPhone)
                                {
                                    if (box.Position.TeachPos == boxOfBinOrPlace.Position.TeachPos)
                                    {
                                        wifiRetryPhone.ElementAt(j).NextTargetPosition =
                                            boxOfBinOrPlace.Position;

                                        wifiLuckyPhones.Add(wifiRetryPhone.ElementAt(j));
                                        wifiLuckyPhones.Add(wifiBinOrPlacePhone.ElementAt(i));
                                        retryAndPlaceCombo = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                OnInfoOccured(222, ex.Message);
                            }
                        }
                    }

                    if (retryAndPlaceCombo)
                    {
                        return wifiLuckyPhones;
                    }
                    else
                    {
                        wifiLuckyPhones.Clear();
                        //If they can't be combo, then place first.
                        wifiLuckyPhones.Add(wifiBinOrPlacePhone.First());
                        return wifiLuckyPhones;
                    }
                    #endregion
                }
                #endregion

                #region Has retry, but not both place and pick.
                else
                {
                    #region Has retry and place.                    
                    if (wifiBinOrPlacePhone.Count > 0)
                    {
                        List<ShieldBox> boxesForRetryPhone = new List<ShieldBox>();
                        ShieldBox boxOfBinOrPlace;

                        for (int i = 0; i < wifiBinOrPlacePhone.Count; i++)
                        {
                            boxOfBinOrPlace = wifiBinOrPlacePhone.ElementAt(i).ShieldBox;
                            for (int j = 0; j < wifiRetryPhone.Count; j++)
                            {
                                try
                                {
                                    boxesForRetryPhone = GetBoxesForWifiRetryPhone(wifiRetryPhone.ElementAt(j));
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
                                catch (Exception ex)
                                {
                                    OnInfoOccured(222, ex.Message);
                                }
                            }
                        }

                        //If they can't be combo, then place first.
                        wifiLuckyPhones.Add(wifiBinOrPlacePhone.First());
                        return wifiLuckyPhones;
                    }
                    #endregion

                    #region Have retry but no place.
                    else
                    {
                        #region Has retry and pick, no place.
                        if (wifiPickPhone.Count > 0)
                        {
                            #region More than one retry phones, no pick then.
                            if (wifiRetryPhone.Count > 1)
                            {
                                List<ShieldBox> boxesForPhone1 = new List<ShieldBox>();
                                List<ShieldBox> boxesForPhone2 = new List<ShieldBox>();

                                for (int i = 0; i < wifiRetryPhone.Count - 1; i++)
                                {
                                    try
                                    {
                                        boxesForPhone1 = GetBoxesForWifiRetryPhone(wifiRetryPhone.ElementAt(i));
                                        for (int j = i + 1; j < wifiRetryPhone.Count; j++)
                                        {
                                            boxesForPhone2 = GetBoxesForWifiRetryPhone(wifiRetryPhone.ElementAt(j));
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
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        OnInfoOccured(222, ex.Message);
                                    }
                                }

                                //Return empty list.
                                return wifiLuckyPhones;
                            }
                            #endregion

                            #region Just one retry and has pick.
                            else
                            {
                                #region Try to combine pick with retry.
                                try
                                {
                                    var pPhone = wifiPickPhone.First();
                                    var rPhone = wifiRetryPhone.First();
                                    List<ShieldBox> box = GetBoxesForWifiRetryPhone(rPhone);
                                    rPhone.NextTargetPosition = box.First().Position;
                                    pPhone.NextTargetPosition = rPhone.CurrentTargetPosition;
                                    wifiLuckyPhones.Add(pPhone);
                                    wifiLuckyPhones.Add(rPhone);
                                    return wifiLuckyPhones;
                                }
                                catch (Exception ex)
                                {
                                    OnInfoOccured(222, ex.Message);
                                    return wifiLuckyPhones;
                                }
                                #endregion
                            } 
                            #endregion
                        }
                        #endregion

                        #region Has retry, no place, no pick.
                        else
                        {
                            #region More than one retry phones.
                            if (wifiRetryPhone.Count > 1)
                            {
                                List<ShieldBox> boxesForPhone1 = new List<ShieldBox>();
                                List<ShieldBox> boxesForPhone2 = new List<ShieldBox>();

                                try
                                {
                                    for (int i = 0; i < wifiRetryPhone.Count - 1; i++)
                                    {
                                        boxesForPhone1 = GetBoxesForWifiRetryPhone(wifiRetryPhone.ElementAt(i));
                                        for (int j = i + 1; j < wifiRetryPhone.Count; j++)
                                        {
                                            boxesForPhone2 = GetBoxesForWifiRetryPhone(wifiRetryPhone.ElementAt(j));
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
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    return wifiLuckyPhones;
                                }

                                //Return empty list.
                                return wifiLuckyPhones;
                            }
                            #endregion

                            #region Only one retry phone
                            var phone = wifiRetryPhone.First();
                            try
                            {
                                List<ShieldBox> box = GetBoxesForWifiRetryPhone(phone);
                                phone.NextTargetPosition = box.First().Position;
                                wifiLuckyPhones.Add(phone);
                                return wifiLuckyPhones;
                            }
                            catch (Exception)
                            {
                                return wifiLuckyPhones;
                            }
                            #endregion
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
                        ShieldBox box = GetEmptyBoxForWifiPhone();
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

        /// <summary>
        /// Phones which Step is in Wifi.
        /// </summary>
        /// <returns></returns>
        private List<Phone> ArrangeRfPhones()
        {
            int maxFailCount = 1;
            if (RfTestMode == RackTestMode.ABC )
            {
                maxFailCount = 3;
            }

            List<Phone> rfLuckyPhones = new List<Phone>();
            List<Phone> rfBinOrPlacePhone = new List<Phone>();
            List<Phone> rfRetryPhone = new List<Phone>();
            List<Phone> rfPickPhone = new List<Phone>();

            #region Classify phones by test result.
            foreach (var phone in RfPhones)
            {
                if (phone.FailCount >= maxFailCount)
                {
                    phone.NextTargetPosition = Motion.BinPosition;
                    phone.Procedure = RackProcedure.Bin;
                    rfBinOrPlacePhone.Add(phone);
                }
                else
                {
                    if (phone.TestResult == TestResult.Pass)
                    {
                        phone.NextTargetPosition = Motion.PickPosition;
                        phone.Procedure = RackProcedure.Place;
                        rfBinOrPlacePhone.Add(phone);
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

            #region Has retry, maybe place, maybe pick.
            if (rfRetryPhone.Count > 0)
            {
                #region Have place retry and pick.
                if (rfBinOrPlacePhone.Count > 0 && rfPickPhone.Count > 0)
                {
                    List<Phone> bOpPhoneCouple = new List<Phone>();
                    List<Phone> rPhoneCouple = new List<Phone>();

                    #region Try to find retry and place combo.
                    foreach (var rPhone in rfRetryPhone)
                    {
                        foreach (var bOpPhone in rfBinOrPlacePhone)
                        {
                            //Todo find box for them.
                            rPhone.NextTargetPosition = bOpPhone.CurrentTargetPosition;

                            rPhoneCouple.Add(rPhone);
                            bOpPhoneCouple.Add(bOpPhone); //Order matters.
                        }
                    }
                    #endregion

                    #region Found retry and place combo.
                    if (rPhoneCouple.Count > 0)
                    {
                        //Try to find the second unload and load.
                        foreach (var rPhone in rPhoneCouple)
                        {
                            foreach (var pPhone in rfPickPhone)
                            {
                                
                                    pPhone.NextTargetPosition = rPhone.CurrentTargetPosition;
                                    rPhone.NextTargetPosition = bOpPhoneCouple
                                        .ElementAt(rPhoneCouple.IndexOf(rPhone))
                                        .CurrentTargetPosition;

                                    rfLuckyPhones.Add(pPhone);
                                    rfLuckyPhones.Add(rPhone);
                                    rfLuckyPhones.Add(
                                        bOpPhoneCouple.ElementAt(
                                            rPhoneCouple.IndexOf(rPhone))); //Order matters.
                                    return rfLuckyPhones; //Find two combo movement.
                                
                            }
                        }

                        // Can't build unload and load with pick.
                        //So just leave pick alone.
                        rfLuckyPhones.Add(rPhoneCouple.First());
                        rfLuckyPhones.Add(bOpPhoneCouple.First());
                        return rfLuckyPhones; //Find two unload and load movement.
                    }
                    #endregion

                    #region Not find retry and place combo.
                    else
                    {
                        foreach (var rPhone in rfRetryPhone)
                        {
                            foreach (var pPhone in rfPickPhone)
                            {
                               
                                    pPhone.NextTargetPosition = rPhone.CurrentTargetPosition;

                                    rfLuckyPhones.Add(pPhone);
                                    rfLuckyPhones.Add(rPhone); //Order matters.
                                    return rfLuckyPhones;
                                
                            }
                        }

                        return rfLuckyPhones;
                    }
                    #endregion
                }
                #endregion

                #region Has retry, but not both place and pick.
                else
                {
                    #region Has retry and place.                    
                    if (rfBinOrPlacePhone.Count > 0)
                    {
                        //foreach (var rPhone in rfRetryPhone)
                        //{
                        //    foreach (var bOpPhone in rfBinOrPlacePhone)
                        //    {
                        //        rPhone.NextTargetPosition = bOpPhone.CurrentTargetPosition;

                        //        rfLuckyPhones.Add(rPhone);
                        //        rfLuckyPhones.Add(bOpPhone); //Order matters.
                        //        return rfLuckyPhones;
                        //    }
                        //}

                        //Todo, right now, just two rf boxes.
                        var bOpPhone = rfBinOrPlacePhone.First();
                        //SetNextTargetPosition(bOpPhone);

                        var rPhone = rfRetryPhone.First();
                        List<ShieldBox> box = GetBoxesForWifiRetryPhone(rPhone);
                        rPhone.NextTargetPosition = box.First().Position;
                        if (rPhone.NextTargetPosition.TeachPos == rfBinOrPlacePhone.First().ShieldBox.Position.TeachPos)
                        {
                            rPhone.NextTargetPosition = bOpPhone.CurrentTargetPosition;
                            rfLuckyPhones.Add(rPhone);
                        }

                        rfLuckyPhones.Add(bOpPhone);
                        return rfLuckyPhones;
                    }
                    #endregion

                    #region Have retry but no place.
                    else
                    {
                        #region Has retry and pick, no place.
                        if (rfPickPhone.Count > 0)
                        {
                            #region More than one retry phones. Better not pick new phone in.
                            if (rfRetryPhone.Count > 1)
                            {
                                List<ShieldBox> boxesForPhone1 = new List<ShieldBox>();
                                List<ShieldBox> boxesForPhone2 = new List<ShieldBox>();

                                for (int i = 0; i < rfRetryPhone.Count - 1; i++)
                                {
                                    boxesForPhone1 = GetBoxesForWifiRetryPhone(rfRetryPhone.ElementAt(i));
                                    for (int j = i + 1; j < rfRetryPhone.Count; j++)
                                    {
                                        boxesForPhone2 = GetBoxesForWifiRetryPhone(rfRetryPhone.ElementAt(j));
                                        foreach (var box1 in boxesForPhone1)
                                        {
                                            foreach (var box2 in boxesForPhone2)
                                            {
                                                //Two phone can exchange box.
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
                                    }
                                }

                                //Return empty list.
                                return rfLuckyPhones;
                            }
                            #endregion

                            #region Just one retry and has pick.
                            else
                            {
                                #region Try to combine pick with retry.
                                foreach (var rPhone in rfRetryPhone)
                                {
                                    try
                                    {
                                        List<ShieldBox> box = GetBoxesForWifiRetryPhone(rPhone);
                                        rPhone.NextTargetPosition = box.First().Position;
                                        //Todo try to combine pick with retry, pick gold or regular.                                      
                                        rfLuckyPhones.Add(rPhone);
                                        //rfLuckyPhones.Add(pPhone); 
                                        return rfLuckyPhones;
                                    }
                                    catch (Exception)
                                    {
                                        //Return empty.
                                        return rfLuckyPhones;
                                    }
                                }

                                //Todo, use gold position as phone buffer?
                                return rfLuckyPhones;
                                #endregion
                            }
                            #endregion
                        }
                        #endregion

                        #region Has retry, no place, no pick.
                        else
                        {
                            #region More than one retry phones.
                            if (rfRetryPhone.Count > 1)
                            {
                                List<ShieldBox> boxesForPhone1 = new List<ShieldBox>();
                                List<ShieldBox> boxesForPhone2 = new List<ShieldBox>();

                                try
                                {
                                    for (int i = 0; i < rfRetryPhone.Count - 1; i++)
                                    {
                                        boxesForPhone1 = GetBoxesForWifiRetryPhone(rfRetryPhone.ElementAt(i));
                                        for (int j = i + 1; j < rfRetryPhone.Count; j++)
                                        {
                                            boxesForPhone2 = GetBoxesForWifiRetryPhone(rfRetryPhone.ElementAt(j));
                                            foreach (var box1 in boxesForPhone1)
                                            {
                                                foreach (var box2 in boxesForPhone2)
                                                {
                                                    //Two phone can exchange box.
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
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    return rfLuckyPhones;
                                }

                                //Return empty list.
                                return rfLuckyPhones;
                            }
                            #endregion

                            #region Only one retry phone
                            var phone = rfRetryPhone.First();
                            try
                            {
                                List<ShieldBox> box = GetBoxesForWifiRetryPhone(phone);
                                phone.NextTargetPosition = box.First().Position;
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
                #endregion
            }
            #endregion

            #region No retry, maybe place, maybe pick.
            else
            {
                #region No retry, has place, maybe pick.
                if (rfBinOrPlacePhone.Count > 0)
                {
                    #region No retry, Have place and pick.
                    if (rfPickPhone.Count > 0)
                    {
                        var pPhone = rfPickPhone.First();
                        var bOpPhone = rfBinOrPlacePhone.First();
                        pPhone.NextTargetPosition = bOpPhone.CurrentTargetPosition;
                        rfLuckyPhones.Add(pPhone);
                        rfLuckyPhones.Add(bOpPhone);
                        return rfLuckyPhones;
                    }
                    #endregion

                    #region No retry, no pick, has place.
                    else
                    {
                        var phone = rfBinOrPlacePhone.First();
                        //A Rf pass phone will go into Wifi box, so Next position not decided. 
                        rfLuckyPhones.Add(phone);
                        return rfLuckyPhones;
                    }
                    #endregion
                }

                #endregion

                #region No retry, no place, has pick.
                else
                {
                    #region Pick Regular phone.
                    try
                    {
                        ShieldBox box = GetEmptyBoxForRfPhone();
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

                //Link(phone, newBox);
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

        private void ServeRetryWifiPhone(Phone phone)
        {
            MovePhoneToOtherBox(phone);
        }

        private void MovePhoneToOtherBox(Phone phone)
        {
            ShieldBox box = ConverterTeachPosToShieldBox(phone.NextTargetPosition.TeachPos);
            RackGripper gripper = GetAvailableGripper();
            Unload(gripper, phone);
            Unlink(phone);
            Load(gripper, phone.NextTargetPosition);
            Link(phone, box);
            CloseBoxAsync(box);
        }

        /// <summary>
        /// Pick a fail phone out of a box.
        /// </summary>
        /// <param name="phone"></param>
        private void ComboRetryAPhone(Phone phone)
        {
            ShieldBox box = phone.ShieldBox;

            if (phone.Type == PhoneType.Normal)
            {
                Unlink(phone, box);
                //Unload.
                Print("Has unload a fail phone.");
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

        private void ComboRetryAPhone(Phone phoneIn, Phone phoneOut)
        {
            ShieldBox box2 = phoneOut.ShieldBox;
            ShieldBox box1 = ConverterTeachPosToShieldBox(phoneOut.NextTargetPosition.TeachPos);
            //Unload and load
            Unlink(phoneOut, box2);
            //Link(phoneIn, box2);
            Print("Has load a retry phone for box." + box2.Id);

            if (phoneIn.Type == PhoneType.Normal)
            {
                //Load()
                //Link(phoneOut, box1);
                Print("Has load a retry phone for box." + box1.Id);
            }
            else //A gold phone.
            {
                TargetPosition toLoadPosition = ConvertGoldIdToTargetPosition(phoneIn.Id);
                //Load()
                Print("Has put back gold phone.");
                //box.GoldPhoneChecked = true;
            }
        }

        /// <summary>
        /// A phone in hand in the end.
        /// </summary>
        /// <param name="phoneIn"></param>
        /// <param name="phoneOut"></param>
        private void TurboComboRetryAPhone(Phone phoneIn, Phone phoneOut)
        {
            ShieldBox box1 = phoneOut.ShieldBox;
            //Unload and load
            Unlink(phoneOut, box1);
            //Link(phoneIn, box1);

            if (phoneIn.Type == PhoneType.Normal)
            {
            }
            else //A gold phone.
            {
                TargetPosition toLoadPosition = ConvertGoldIdToTargetPosition(phoneIn.Id);
                //Load()
                //box.GoldPhoneChecked = true;
            }
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
                Load(gripper, phone.NextTargetPosition);
                CloseBoxAsync(box);
            }
            catch (Exception e)
            {
                OnErrorOccured(555, "ServeNewRfPhone failed: " + e.Message);
                Unlink(phone, box);
                throw;
            }
        }

        private void ServeNewWifiPhone(Phone phone)
        {
            MovePhoneToOtherBox(phone);
        }

        private void ServePassRfPhone(Phone phone)
        {
            phone.Step = RackTestStep.Wifi;
            //Set result will trigger add phone to server.
            phone.TestResult = TestResult.None;
            phone.FailCount = 0;
        }

        private void ServePassWifiPhone(Phone phone)
        {
            RackGripper gripper = GetAvailableGripper();
            Unload(gripper, phone);
            Unlink(phone);
            Place(gripper);
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

        private void Unlink(Phone phone, ShieldBox box)
        {
            phone.ShieldBox = null;
            box.Available = true;
            box.Empty = true;
            box.Phone = null;
        }

        private void Unlink(Phone phone)
        {
            ShieldBox box = phone.ShieldBox;
            phone.ShieldBox = null;
            box.Available = true;
            box.Empty = true;
            box.Phone = null;
        }



        private void ComboPlaceAPhone(Phone phoneIn, Phone phoneOut)
        {
            ShieldBox box = phoneOut.ShieldBox;
            //Unload and load
            Unlink(phoneOut, box);
            //Link(phoneIn,box);
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

        private void TurboComboPlaceAPhone(Phone phoneIn, Phone phoneOut)
        {
            ShieldBox box = phoneOut.ShieldBox;
            //Unload and load
            Unlink(phoneOut, box);
            //Link(phoneIn, box);

            if (phoneIn.Type == PhoneType.Normal)
            {
                //Place()
            }
            else //A gold phone.
            {
                TargetPosition toLoadPosition = ConvertGoldIdToTargetPosition(phoneIn.Id);
                //Load()
            }
        }

        private void BinAPhone(Phone phone)
        {
            ShieldBox box = phone.ShieldBox;
            Unlink(phone, box);

            //Unload()
            //Todo need to check if door is open.
            //Bin()
            Print("Has bin a phone.");
        }

        private void ServeBinWifiPhone(Phone phone)
        {
            RackGripper gripper = GetAvailableGripper();
            Unload(gripper, phone);
            Unlink(phone);
            Bin(gripper);
        }

        private void ComboBinAPhone(Phone phoneIn, Phone phoneOut)
        {
            ShieldBox box = phoneOut.ShieldBox;
            //Unload and load
            Unlink(phoneOut, box);
            //Link(phoneIn, box);
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

        private void TurboComboBinAPhone(Phone phoneIn, Phone phoneOut)
        {
            ShieldBox box = phoneOut.ShieldBox;
            //Unload and load
            Unlink(phoneOut, box);
            //Link(phoneIn, box);

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
                    Console.Write(" Phone:{0} Step:{1} Box:{2}", box.Phone.Id, box.Phone.Step, box.Phone.ShieldBox.Id);
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

        private bool MoreThanTwoEmptyBoxes()
        {
            int count = 0; 
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled && box.Empty)
                {
                    count++;
                }
            }

            return count > 1;
        }

        /// <summary>
        /// Maybe it's not empty.
        /// </summary>
        private ShieldBox GetBoxForGoldPhone()
        {
            //If not found a empty box, try to find a available box.
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled && box.Available && box.GoldPhoneChecked==false)
                {
                    return box;
                }
            }

            throw new ShieldBoxNotFoundException("GetBoxForGoldPhone fail.");
        }

        private ShieldBox GetBoxForWifiGoldPhone()
        {
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled && box.Available && box.GoldPhoneChecked == false && box.Type == ShieldBoxType.Wifi)
                {
                    return box;
                }
            }

            throw new ShieldBoxNotFoundException("GetBoxForWifiGoldPhone fail.");
        }

        private ShieldBox GetEmptyBoxForWifiPhone()
        {
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled && box.Empty && box.Type == ShieldBoxType.Wifi)
                {
                    return box;
                }
            }

            throw new ShieldBoxNotFoundException("GetEmptyBoxForNewWifiPhone fail.");
        }

        private ShieldBox GetEmptyBoxForRfPhone()
        {
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled && box.Empty && box.Type == ShieldBoxType.Rf)
                {
                    return box;
                }
            }

            throw new ShieldBoxNotFoundException("GetEmptyBoxForNewWifiPhone fail.");
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
                    if (box.Enabled && box.Available)
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

        private List<ShieldBox> GetBoxesForRetryPhone(Phone phone)
        {
            lock (_availableBoxLocker)
            {
                List<ShieldBox> retryBoxs = new List<ShieldBox>();
                foreach (var box in ShieldBoxs)
                {
                    var foundBox = true;
                    if (box.Enabled && box.Available)
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
                        retryBoxs.Add(box);
                    }
                }

                return retryBoxs;
            }
        }

        private List<ShieldBox> GetBoxesForWifiRetryPhone(Phone phone)
        {
            if (WifiTestMode == RackTestMode.AB)
            {
                List<ShieldBox> retryBoxs = new List<ShieldBox>();
                lock (_availableBoxLocker)
                {
                    foreach (var box in ShieldBoxs)
                    {
                        var foundBox = true;
                        if (box.Enabled && box.Available & box.Type == ShieldBoxType.Wifi)
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
                            retryBoxs.Add(box);
                        }
                    }
                }

                if (retryBoxs.Count == 0)
                {
                    throw new Exception("GetBoxesForWifiRetryPhone failed.");
                }

                return retryBoxs;
            }
            else
            {
                throw new Exception("Error 49841634791.");
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

            PhoneServerManualResetEvent.Set();
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
