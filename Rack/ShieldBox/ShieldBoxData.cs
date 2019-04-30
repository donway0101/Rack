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
        //Open and close value for Rack api for our customer
        Open=1,
        Close=2,
        Testing,
        Fail,
        Pass,       
    }

    public static class ShieldBoxResponse
    {
        public const string ResponseEnding = "\r\n";
        public const string OpenSuccessful = ResponseEnding + "OK" + ResponseEnding;
        public const string CloseSuccessful = ResponseEnding + "READY" + ResponseEnding;
        public const string LightOnSuccessful = "OK" + ResponseEnding;
        public const string LightOnSuccessful2 = ResponseEnding + "OK" + ResponseEnding;
        public const string BoxIsOpened = "OPEN" + ResponseEnding;
        public const string BoxIsClosed = "CLOSE" + ResponseEnding;
    }

    public enum TestResult
    {
        None,
        Pass,
        Fail,
    }



}
