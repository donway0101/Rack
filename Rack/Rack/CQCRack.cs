using System.Collections.Generic;
using System.Threading;
using ACS.SPiiPlusNET;

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
        private bool _testRun = false;
        
        public EthercatMotion Motion;
        public Stepper Steppers;
        public EthercatIo EcatIo;
        public Conveyor Conveyor;
       
        public ShieldBox ShieldBox1 { get; set; }
        public ShieldBox ShieldBox2 { get; set; }
        public ShieldBox ShieldBox3 { get; set; }
        public ShieldBox ShieldBox4 { get; set; }
        public ShieldBox ShieldBox5 { get; set; }
        public ShieldBox ShieldBox6 { get; set; }        

        private readonly object _shieldBoxsLocker = new object();
        /// <summary>
        /// Boxes which is available.
        /// </summary>
        public List<ShieldBox> ShieldBoxs = new List<ShieldBox>();

        private Thread _phoneServerThread;
        private readonly object _phoneToBeServedLocker = new object();
        /// <summary>
        /// Phones on conveyor or in box or gold.
        /// </summary>
        public List<Phone> PhoneToBeServed = new List<Phone>();

        private readonly object _phoneInServiceLocker = new object();
        /// <summary>
        /// Phones which can be served by robot.
        /// </summary>
        public List<Phone> PhoneInService = new List<Phone>();

        public bool RobotHomeComplete { get; set; }
        public bool SetupComplete { get; set; }

        private readonly string _ip;

    }
}
