using System.Collections.Generic;
using System.Security.AccessControl;

namespace Rack
{
    public class Phone
    {
        public long Id { get; set; }
        public PhoneType Type { get; set; } = PhoneType.Normal;
        public string SerialNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// Need smart algorithm for next target position decision
        public TargetPosition NextTargetPosition { get; set; }
        //Todo link to shield box.
        public List<TargetPosition> TargetPositionFootprint { get; set; }
        public TargetPosition CurrentTargetPosition { get; set; }
        public bool ReadyForNextProcedure { get; set; }
        public PhonePriority Priority { get; set; } = PhonePriority.Low;
        public RackGripper OnWhichGripper { get; set; }
        public RackProcedure Procedure { get; set; }
        public long TestCycleTime { get; set; }
        public ShieldBoxTestResult TestResult { get; set; } = ShieldBoxTestResult.None;

        /// <summary>
        /// 
        /// </summary>
        /// Todo link to shield box test result.
        public int FailCount { get; set; }

        public TestStep Step { get; set; }
    }
}
