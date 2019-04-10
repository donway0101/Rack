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
        UnloadAndLoad,
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
        Home,
        Pick,
        Bin,
        ConveyorLeft,
        ConveyorRight,
        G1ToG2Offset,
        PickOffset,

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
