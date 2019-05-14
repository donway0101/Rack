namespace Rack
{
    /// <summary>
    /// 4 kinds of gold phone max.
    /// </summary>
    public partial class CqcRack
    {
        public void AddNewPhone()
        {
            Phone phone = new Phone()
            {
                Step = RackTestStep.Rf,
                FailCount = 0,
                Id = PhoneCount++,
                CurrentTargetPosition = Motion.PickPosition,               
            };

            if (ScannerOnline)
            {
                phone.SerialNumber = Scanner.SerialNumber;
            }

            phone.TestComplete -= Phone_TestComplete;
            phone.TestComplete += Phone_TestComplete;

            AddPhoneToBeServed(phone);

            LatestPhone = phone;
        }       

        //public void AddGoldPhone()
        //{
        //    AddPhoneToBeServed(new Phone()
        //    {
        //        AtBoxType = ShieldBoxType.Rf,
        //        Id = -1,
        //        Type = PhoneType.Golden,
        //    });
        //}

        public void ShieldBoxSetupForSimulation()
        {
            ShieldBox1 = new ShieldBox(1);
            ShieldBox2 = new ShieldBox(2);
            ShieldBox3 = new ShieldBox(3);
            ShieldBox4 = new ShieldBox(4);
            ShieldBox5 = new ShieldBox(5);
            ShieldBox6 = new ShieldBox(6);

            ShieldBoxs = new ShieldBox[6] { ShieldBox1, ShieldBox2, ShieldBox3, ShieldBox4, ShieldBox5, ShieldBox6 };

            foreach (var box in ShieldBoxs)
            {
                box.PortName = XmlReaderWriter.GetBoxAttribute(Files.BoxData, box.Id, ShieldBoxItem.COM);
                box.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, box.Id, ShieldBoxItem.State) == "Enable";
                box.Empty = true;
                box.Available = true;
                box.Position = ConvertBoxIdToTargetPosition(box.Id);
            }
        }

        public void TesterSetupForSimulation()
        {
            //TesterSetup();
        }
    }
}
