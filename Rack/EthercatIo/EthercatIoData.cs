﻿namespace Rack
{
    public enum Output
    {
        //ModuleId = value/10, pin = value % 10, value zero based.

        //0
        RedLight = 00,
        YellowLight = 01,
        GreenLight = 02,
        Beep = 03,
        GripperTwo = 04,
        GripperOne = 05,
        Reserved06 = 06,
        Reserved07 = 07,

        //1
        UpBlockPickForward = 22,
        UpBlockPickBackward = 10,
        SideBlockPick = 11,
        SideBlockSeparateBackward = 12,
        SideBlockSeparateForward = 20,
        ClampPick = 13,
        BeltPick = 14,
        BeltNg = 15,
        BeltConveyorOne = 16,
        UpBlockSeparateBackward = 17,
        UpBlockSeparateForward = 21

        //2
        //GripperOne = 20,
        //GripperOne = 21,
        //GripperOne = 22,
        //GripperOne = 23,
        //GripperOne = 24,
        //GripperOne = 25,
        //GripperOne = 26,
        //GripperOne = 27,
    }

    /// (ModudleID,Bit)
    /// 输出
    /// (0,0)=电机
    /// (0,4)=推出气缸  (5,2)到位传感器
    /// (0,5)=阻挡气缸  (2,6)到位传感器
    /// (0,6)=夹紧气缸  (2,3)松开传感器 夹紧传感器(2,4)
    /// 输入
    /// (2，2)=检测手机到位传感器
    public enum Input
    {
        // 0
        Start = 00,
        Reset = 01,
        Estop = 02,
        SideBlockPick = 03,
        Reserved04 = 04,
        UpBlockPickBackward = 05,
        ClampTightPick = 06,
        ClampLoosePick = 07,

        // 1
        SideBlockSeparateForward = 33,
        SideBlockSeparateBackward = 10,
        Reserved11 = 11,
        ConveyorOneIn = 12,
        ConveyorOneOut = 13,
        PickBufferHasPhoneForward = 14,
        PickBufferHasPhoneBackward = 15,
        ConveyorPickInTwo = 15,
        PickHasPhone = 16,
        Reserved17 = 17,

        // 2
        Gripper01 = 20,
        Gripper01Tight = 21,
        Gripper01Loose = 22,
        Gripper02 = 23,
        Gripper02Tight = 24,
        Gripper02Loose = 25,
        ConveyorPickOut = 26,
        ConveyorBinIn = 27,

        // 3
        ConveyorBinInTwo = 30,
        UpBlockSeparateForward = 34,
        UpBlockSeparateBackward = 31,
        UpBlockPickForward = 32,
        Reservered32 = 33,
        Reservered34 = 34,
        Reservered35 = 35,
        Reservered36 = 36,
        Reservered37 = 37,

        // 4
        Gold1 = 40,
        Gold2 = 41,
        Gold3 = 42,
        Gold4 = 43,
        Gold5 = 44,
        Reservered45 = 45,
        Reservered46 = 46,
        Reservered47 = 47
    }
}