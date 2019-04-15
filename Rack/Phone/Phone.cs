using System.Collections.Generic;
using System.Security.AccessControl;

namespace Rack
{
    public class Phone
    {
        public long Id { get; set; }
        public PhoneType Type { get; set; } = PhoneType.Normal;
        public string SerialNumber { get; set; }
        /// Need smart algorithm for next target position decision
        public TargetPosition NextTargetPosition { get; set; } = new TargetPosition(){TeachPos = TeachPos.None};
        //Todo link to shield box.
        public List<TargetPosition> TargetPositionFootprint { get; set; }
        public TargetPosition CurrentTargetPosition { get; set; }
        public bool ReadyForNextProcedure { get; set; }
        public PhonePriority Priority { get; set; } = PhonePriority.Low;
        public RackProcedure Procedure { get; set; }
        public long TestCycleTime { get; set; }
        /// <summary>
        /// After tester send back test result to shield box,
        ///  set TestResult of phone, and add add failcount if needed.
        /// </summary>
        public TestResult TestResult { get; set; } = TestResult.None;
        /// Todo link to shield box test result.
        public int FailCount { get; set; }
        //Todo initial phone with shield box type.
        public ShieldBoxType AtBoxType { get; set; } = ShieldBoxType.RF;
        //Todo will use in a unload and load movement.
        //Combine check with sensor.
        public RackGripper OnGripper { get; set; }
        public ShieldBox ShieldBox { get; set; }
    }
}
