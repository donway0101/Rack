namespace Rack
{
    public class Phone
    {
        public long Id { get; set; }
        public PhoneType Type { get; set; } = PhoneType.Normal;
        public string SerialNumber { get; set; }
        public TargetPosition NextTargetPosition { get; set; }
        public TargetPosition CurrentTargetPosition { get; set; }
        public bool ReadyForNextProcedure { get; set; }
        public PhonePriority Priority { get; set; } = PhonePriority.Low;
        public RackGripper OnWhichGripper { get; set; }
        public RackProcedure Procedure { get; set; }
        public long TestCycleTime { get; set; }
    }
}
