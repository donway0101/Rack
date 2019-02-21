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
        //ModuleId = value/10, pin = value % 10
        YellowLight = 01,
        GreenLight = 11,
        RedLight = 17,
    }

    public enum Input
    {

    }
}
