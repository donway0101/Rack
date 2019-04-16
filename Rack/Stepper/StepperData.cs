namespace Rack
{
    public enum RackGripper
    {
        None=0,
        One=1, //Must match Id of stepper motor in 422 communication.
        Two=2,
    }

    public enum InputStepper
    {
        X1=1,
        X2,
        X3,
        X4,
    }

    //public enum Output
    //{
    //    Y1=1,
    //    Y2,
    //}

    public enum StatusCode
    {
        Enabled=0,
        Fault=2,
        Inpos=3,
        Alarm=9,
        Homing=10,
    }

}
