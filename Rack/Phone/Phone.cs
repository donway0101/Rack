using System.Collections.Generic;
using System.Diagnostics;
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
        public bool GoldPhoneBusy { get; set; }
        public string SerialNumber { get; set; }
        public string FailDetail { get; set; }
        public bool AutoOpenBox { get; set; } = true;
        public TargetPosition CurrentTargetPosition { get; set; } = new TargetPosition() { TeachPos = TeachPos.None };
        public TargetPosition NextTargetPosition { get; set; } = new TargetPosition() { TeachPos = TeachPos.None };
        public List<TargetPosition> TargetPositionFootprint { get; set; } = new List<TargetPosition>();
        //public PhonePriority Priority { get; set; } = PhonePriority.Low;
        public RackProcedure Procedure { get; set; }
        //public long TestCycleTime { get; set; }

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
                _testResult = value;
                OnTestComplete();
            }
        }

        public Stopwatch TestCycleTimeStopWatch { get; set; } = new Stopwatch();

        public int MaxTestCycleTimeSec { get; set; } = 180;
        public int FailCount { get; set; }
        public RackGripper OnGripper { get; set; }
        public ShieldBox ShieldBox { get; set; }
        public RackTestStep Step { get; set; } = RackTestStep.Rf;

        public delegate void TestCompleteEventHandler(object sender);

        public event TestCompleteEventHandler TestComplete;

        protected void OnTestComplete()
        {
            TestComplete?.Invoke(this);
        }

    }
}
