namespace Rack
{
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
        Ip,
        Port,
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

    public struct Files
    {
        public const string RackData = "RackData.xml";
        public const string BoxData = "BoxData.xml";
        public const string LoginData = "LoginData.xml";
        public const string RackSetting = "RackSetting.xml";
    }
    public enum LoginType
    {
        LogicType,
        Accout,

        Administrator,
        Technician,
        Operator,
    }
    public enum Power
    {
        None,
        Administrator,
        Technician,
        Operator
    }
    public enum LogicInformation
    {
        LoginName,
        LoginPassWord,
    }

    public enum RackSetting
    {
        ConveyorSetting,
        ConveyorMovingForward,
    }
}
