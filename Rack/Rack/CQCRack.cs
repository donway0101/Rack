using System;
using System.Threading;
using System.Threading.Tasks;
using ACS.SPiiPlusNET;
using Motion;
using GripperStepper;
using EcatIo;
using Conveyor;
using ShieldBox;
using Tools;

namespace Rack
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>If estop button is on, then ethercat bus error occur, notify user, use reboot method</remarks>
    /// Power up, Ethercat error occur, wire problem? 
    public partial class CqcRack
    {
        private readonly Api _ch = new Api();
        private const double YIsInBox = 200;
        private const double YIsNearHome = 10;
        private bool _gripperIsOnline = true;
        private readonly string _ip;
        private bool _testRun = false;
        private Thread _shieldBoxServerThread;

        public EthercatMotion Motion;
        public Stepper Stepper;
        public EthercatIo EcatIo;
        public PickAndPlaceConveyor Conveyor;
      
        public BpShieldBox ShieldBox1 { get; set; }
        public BpShieldBox[] ShieldBoxs { get; set; }

        public bool RobotHomeComplete { get; set; }
        public bool SetupComplete { get; set; }

        public CqcRack(string controllerIp)
        {
            _ip = controllerIp;
        }

    }
}
