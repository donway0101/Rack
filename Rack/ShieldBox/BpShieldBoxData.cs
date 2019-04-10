namespace Rack
{
    public enum Command
    {
        OPEN, //
        CLOSE,
        PASS,
        FAIL,
        TESTING,
        STATUS,
        SWITCHRF,
    }

    public enum State
    {
        Testing,
        Fail,
        Pass,
        Open,
        Close,
    }

    public static class Response
    {
        private const string ResponseEnding = "\r\n";
        public const string OpenSuccessful = ResponseEnding + "OK" + ResponseEnding;
        public const string CloseSuccessful = ResponseEnding + "READY" + ResponseEnding;
        public const string LightOnSuccessful = "OK" + ResponseEnding;
        public const string LightOnSuccessful2 = ResponseEnding + "OK" + ResponseEnding;
        public const string BoxIsOpened = "OPEN" + ResponseEnding;
        public const string BoxIsClosed = "CLOSE" + ResponseEnding;
    }

    public enum TestResult
    {
        Pass,
        Fail,
        Testing,
    }



}
