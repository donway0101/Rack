using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO
{
    public class DataStructure
    {

    }

    public enum ModuleBaseAddress
    {
        First=72,
        Second=144,
    }

    public enum Output
    {
        //ModuleId = value/10, pin = value % 10, value zero based.
        
        YellowLight = 01,
        GreenLight = 11,
        RedLight = 17,

        Belt = 00,
        Push = 04,
        BlockPick=05,
        ClampPick=06,
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
        SomeInput=01,
        PushIn=52,
        BlockPickUp = 26,
        ClampTight=24,
        ClampLoose=23,
        PhoneInPick=22,
    }
}
