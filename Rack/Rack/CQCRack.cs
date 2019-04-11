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
        private readonly string _ip;
        private readonly Api _ch = new Api();
        private const double YIsInBox = 200;
        private const double YIsNearHome = 10;

        private bool _gripperIsOnline = true;     
        private bool _testRun = false;

        public bool RobotHomeComplete { get; set; }
        public bool SetupComplete { get; set; }

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
        /// Todo: current there is only one type of shiled box.
        ///  if more than one type, create another list,
        ///     change next target position strategy.
        /// <seealso cref="ArrangeNextTargetPosition"/>
        public List<ShieldBox> ShieldBoxs = new List<ShieldBox>();

        private Thread _phoneServerThread;
        private readonly object _phoneToBeServedLocker = new object();

        /// <summary>
        /// Phones only which already has place to go can add to the list.
        /// </summary>
        public List<Phone> PhoneToBeServed = new List<Phone>();

        private readonly object _availableBoxLocker = new object();
        public List<ShieldBox> AvailableBox = new List<ShieldBox>();

        /// <summary>
        /// Set, to serve, if exception, has to set again to continue.
        /// </summary>
        public ManualResetEvent PhoneServerManualResetEvent = new ManualResetEvent(false);

        public RackTestMode TestMode { get; set; } = RackTestMode.AB;
        
        public long PhoneCount { get; set; }

        #region Events
        public delegate void ErrorOccuredEventHandler(object sender, string description);

        public event ErrorOccuredEventHandler ErrorOccured;

        protected void OnErrorOccured(string description)
        {
            ErrorOccured?.Invoke(this, description);
        }

        public delegate void WarningOccuredEventHandler(object sender, string description);

        public event WarningOccuredEventHandler WarningOccured;

        protected void OnWarningOccured(string description)
        {
            WarningOccured?.Invoke(this, description);
        }

        public delegate void InfoOccuredEventHandler(object sender, string description);

        public event InfoOccuredEventHandler InfoOccured;

        protected void OnInfoOccured(string description)
        {
            InfoOccured?.Invoke(this, description);
        }

        #endregion

    }
}
