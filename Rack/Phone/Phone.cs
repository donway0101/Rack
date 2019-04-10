namespace Rack
{
    public class Phone
    {
        public PhoneId Id { get; set; } = PhoneId.Normal;
        public string SerialNumber { get; set; }
        public TargetPosition NextTargetPosition { get; set; }
        public bool ReadyForNextProcedure { get; set; }
        public PhonePriority Priority { get; set; } = PhonePriority.Low;
    }
}
