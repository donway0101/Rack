using System.Collections.Generic;
using System.Security.AccessControl;

namespace Rack
{
    public class Phone
    {
        /// <summary>
        /// Corresponding to target position of phone.
        /// </summary>
        public long Id { get; set; }
        public PhoneType Type { get; set; } = PhoneType.Normal;
        public string SerialNumber { get; set; }     
        public TargetPosition CurrentTargetPosition { get; set; } = new TargetPosition() { TeachPos = TeachPos.None };
        public TargetPosition NextTargetPosition { get; set; } = new TargetPosition() { TeachPos = TeachPos.None };
        public List<TargetPosition> TargetPositionFootprint { get; set; } = new List<TargetPosition>();
        public PhonePriority Priority { get; set; } = PhonePriority.Low;
        public RackProcedure Procedure { get; set; }
        public long TestCycleTime { get; set; }

        private TestResult _testResult = TestResult.None;
        /// <summary>
        /// After tester send back test result to shield box,
        ///  set TestResult of phone, and add add failcount if needed.
        /// </summary>
        public TestResult TestResult
        {
            get => _testResult;

            set
            {
                if (_testResult!=value)
                {
                    _testResult = value;
                    OnTestResultChanged();
                }
            }
        }
        /// Todo link to shield box test result.
        public int FailCount { get; set; }
        //Todo initial phone with shield box type.
        public ShieldBoxType AtBoxType { get; set; } = ShieldBoxType.RF;
        //Todo will use in a unload and load movement.
        //Combine check with sensor.
        public RackGripper OnGripper { get; set; }
        public ShieldBox ShieldBox { get; set; }

        public delegate void TestResultChangedEventHandler(object sender);

        public event TestResultChangedEventHandler TestResultChanged;

        protected void OnTestResultChanged()
        {
            TestResultChanged?.Invoke(this);
        }

    }
}
