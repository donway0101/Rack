using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripperStepper
{
    public enum Gripper
    {
        One=1,
        Two=2,
    }

    public enum Input
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
