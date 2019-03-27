using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion
{
    public class DataStructure
    {
    }

    public class TargetPosition
    {
        public Location Id { get; set; } = Location.NoWhere;
        public double XPos { get; set; }
        public double YPos { get; set; }
        public double ZPos { get; set; }
        public double RPos { get; set; }
        public double APos { get; set; }
        public double ApproachHeight { get; set; }
    }

    public enum Location
    {
        NoWhere = 99,
        Home=41,
        Pick=42,
        Bin=44,
        
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
        Gold6 = 16,
    }
}
