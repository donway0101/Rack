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
                OnGripper = RackGripper.None,
                Priority = PhonePriority.Low,
                Procedure = RackProcedure.Pick,
                ReadyForNextProcedure = true,
                SerialNumber = "",
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
                OnGripper = RackGripper.None,
                Priority = PhonePriority.Low,
                Procedure = RackProcedure.Pick,
                ReadyForNextProcedure = true,
                SerialNumber = "",
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
                NextTargetPosition = new TargetPosition() { TeachPos = TeachPos.None },
                OnGripper = RackGripper.None,
                Priority = PhonePriority.Low,
                Procedure = RackProcedure.Pick,
                ReadyForNextProcedure = true,
                SerialNumber = "",
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
                NextTargetPosition = new TargetPosition() { TeachPos = TeachPos.None },
                OnGripper = RackGripper.None,
                Priority = PhonePriority.Low,
                Procedure = RackProcedure.Pick,
                ReadyForNextProcedure = true,
                SerialNumber = "",
                TestResult = TestResult.Pass,
            });
        }


    }
}
