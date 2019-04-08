namespace ShieldBox
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

    public static class Response
    {
        private const string ResponseEnding = "\r\n";
        public const string OpenSuccessful = ResponseEnding + "OK" + ResponseEnding;
        public const string CloseSuccessful = ResponseEnding + "READY" + ResponseEnding;
    }

    public enum TestResult
    {
        Pass,
        Fail,
        Testing,
    }



}
