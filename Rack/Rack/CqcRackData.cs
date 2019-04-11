using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rack;

namespace Rack
{

    public enum RackGripper
    {
        None,
        One,
        Two,
    }

    public enum RackProcedure
    {
        Bin,
        Place,
        Retry,
        Pick,
    }

    public enum RackTestMode
    {
        AB,
        AAB,
        ABC,
        ABA,        
    }

    public enum TeachPos
    {
        None = 99,
        ConveyorLeft= 98,
        ConveyorRight = 98,
        G1ToG2Offset = 96,
        PickOffset = 96,
        
        Home = 41,
        Pick = 42,
        Bin = 44,

        Holder1 = 1,
        Holder2 = 2,
        Holder3 = 3,
        Holder4 = 4,
        Holder5 = 5,
        Holder6 = 6,

        Gold1 = 11,
        Gold2 = 12,
        Gold3 = 13,
        Gold4 = 14,
        Gold5 = 15,
    }
}
