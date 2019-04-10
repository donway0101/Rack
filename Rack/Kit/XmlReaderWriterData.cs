namespace Rack
{
    public struct Files
    {
        public const string RackData = "RackData.xml";
        public const string BoxData = "BoxData.xml";
    }

    public enum PosItem
    {
        Teach,
        Pos,
        Name,
        XPos,
        YPos,
        ZPos,
        RPos,
        APos,
        ApproachHeight,
    }


    public enum ShieldBoxItem
    {
        ShieldBox,
        BoxId,
        State,
        COM,
        Type,
        Label,
    }

    public enum LabelType
    {
        A,
        B,
        C,
    }
    public enum ShieldBoxType
    {
        RF,
        WIFI,
        BT,
    }


}
