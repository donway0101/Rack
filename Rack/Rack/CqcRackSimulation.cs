namespace Rack
{
    public partial class CqcRack
    {
        //Todo 
        public void AddNewPhone()
        {
            Phone phone = new Phone()
            {
                AtBoxType = ShieldBoxType.RF,
                CurrentTargetPosition = new TargetPosition() { TeachPos = TeachPos.Pick },
                FailCount = 0,
                Id = PhoneCount++,
                NextTargetPosition = new TargetPosition() {TeachPos = TeachPos.None},
                Procedure = RackProcedure.Pick,
                TestResult = TestResult.None,
            };

            AddPhoneToBeServed(phone);
        }

        public void AddFailedPhone()
        {
            AddPhoneToBeServed(new Phone()
            {
                AtBoxType = ShieldBoxType.RF,
                CurrentTargetPosition = new TargetPosition() { TeachPos = TeachPos.Holder1 },
                FailCount = 3,
                Id = PhoneCount++,
                NextTargetPosition = new TargetPosition() { TeachPos = TeachPos.None },         
                TestResult = TestResult.Fail,
            });
        }

        public void AddRetryPhone()
        {
            AddPhoneToBeServed(new Phone()
            {
                AtBoxType = ShieldBoxType.RF,
                CurrentTargetPosition = new TargetPosition() { TeachPos = TeachPos.Holder2 },
                FailCount = 2,
                Id = PhoneCount++,
                TestResult = TestResult.Fail,
            });
        }

        public void AddPassPhone()
        {
            AddPhoneToBeServed(new Phone()
            {
                AtBoxType = ShieldBoxType.RF,
                CurrentTargetPosition = new TargetPosition() { TeachPos = TeachPos.Holder3 },
                FailCount = 0,
                Id = PhoneCount++,
                //NextTargetPosition = new TargetPosition() { TeachPos = TeachPos.None },
                TestResult = TestResult.Pass,
            });
        }

        public void AddGoldPhone()
        {
            AddPhoneToBeServed(new Phone()
            {
                AtBoxType = ShieldBoxType.RF,
                CurrentTargetPosition = new TargetPosition() { TeachPos = TeachPos.Gold1 },
                FailCount = 0,
                Id = -1,
                //NextTargetPosition = new TargetPosition() { TeachPos = TeachPos.None },
                TestResult = TestResult.None,
                Procedure = RackProcedure.Pick,
                Type = PhoneType.Golden,
            });
        }

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
    }
}
