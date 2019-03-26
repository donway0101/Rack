using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public struct Files
    {
        public const string RackData = "RackData.xml";
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

    public enum TeachPos
    {
        Home,
        Pick,
        Bin,
        ConveyorLeft,
        ConveyorRight,

        Holder1,
        Holder2,
        Holder3,
        Holder4,
        Holder5,
        Holder6,

        Gold1,
        Gold2,
        Gold3,
        Gold4,
        Gold5,

    }
}
