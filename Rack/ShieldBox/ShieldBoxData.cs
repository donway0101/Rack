namespace Rack
{
    public enum ShieldBoxCommand
    {
        OPEN, //
        CLOSE,
        PASS,
        FAIL,
        TESTING,
        STATUS,
        SWITCHRF,
    }

    public enum ShieldBoxState
    {
        Testing,
        Fail,
        Pass,
        Open,
        Close,
    }

    public static class ShieldBoxResponse
    {
        private const string ResponseEnding = "\r\n";
        public const string OpenSuccessful = ResponseEnding + "OK" + ResponseEnding;
        public const string CloseSuccessful = ResponseEnding + "READY" + ResponseEnding;
        public const string LightOnSuccessful = "OK" + ResponseEnding;
        public const string LightOnSuccessful2 = ResponseEnding + "OK" + ResponseEnding;
        public const string BoxIsOpened = "OPEN" + ResponseEnding;
        public const string BoxIsClosed = "CLOSE" + ResponseEnding;
    }

    public enum ShieldBoxTestResult
    {
        None,
        Pass,
        Fail,
        Testing,
    }



}
