using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rack;

namespace Rack
{
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

    public enum RackTestStep
    {
        Rf,
        Wifi,
        Bt,
    }

    public enum BoxCombination
    {
        Rf2Wifi,
    }

    public enum TeachPos
    {
        //if two enumItem = same value, when using it, it will cause problem.
        None = 99,
        ConveyorLeft= 98,
        ConveyorRight = 97,
        G1ToG2Offset = 96,
        PickOffset = 95,
        
        Home = 41,
        Pick = 42,
        Bin = 44,

        ShieldBox1 = 1,
        ShieldBox2 = 2,
        ShieldBox3 = 3,
        ShieldBox4 = 4,
        ShieldBox5 = 5,
        ShieldBox6 = 6,

        Gold1 = 11,
        Gold2 = 12,
        Gold3 = 13,
        Gold4 = 14,
        Gold5 = 15,
    }
}
