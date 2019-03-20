using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rack
{
    public enum ShiledBoxData
    {
        RackData,

        ShiledBox,
        Box,
        Id,
        CarrierHeight,
        DoorHeight,
    }

    public enum ConveyorData
    {
        Conveyor,
        PickConveyorHeight,
        BinConveyorHeight,
        XMinPos,
        XMaxPos,
    }

    public enum TeachData
    {
        Teach,
        Pos,
        Name,
        XPos,
        YPos,
        ZPos,
    }
}
