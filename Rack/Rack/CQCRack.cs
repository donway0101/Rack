﻿using System.Collections.Generic;
using System.Threading;
using ACS.SPiiPlusNET;

namespace Rack
{
    public partial class CqcRack
    {
        #region Private
        private readonly string _ip;
        private readonly Api _ch = new Api();
        private const double YIsInBox = 200.0;
        private const double YIsNearHome = 10.0;
        private bool _eventEnabled;
        private string _newPhoneSerialNumber = string.Empty;
        private bool _newPhoneHasBeenServed = false;
        #endregion

        #region Robot

        public bool RobotInSimulateMode { get; set; } = false;
        private bool _serverInSimulateMode;

        public double HomeSpeed { get; set; } = 30;

        /// <summary>
        /// Gripper one is a reference. Gripper 2 is offset.
        /// </summary>
        public double PickXOffset { get; set; } = 1;

        /// <summary>
        /// Set after tester initialized. 
        /// </summary>
        public bool ServerInSimulateMode
        {
            get { return _serverInSimulateMode; }
            set
            {
                _serverInSimulateMode = value;
                foreach (var tester in Testers)
                {
                    tester.SimulateMode = value;
                }
            }
        }

        public bool ShieldBoxOnline { get; set; } = true;
        public bool TesterOnline { get; set; } = true;
        public bool ScannerOnline { get; set; } = true;
        public bool TestRun { get; set; } = true;
        public bool StepperOnline { get; set; } = true;
        public bool ConveyorOnline { get; set; } = true;
        public bool MotorsOnline { get; set; } = true;
        public bool EthercatOnline { get; set; } = true;
        
        public double DefaultRobotSpeed { get; set; } = 10;        

        public bool RobotHomeComplete { get; set; }

        public bool StepperHomeComplete { get; set; }
        public bool BoxChecked { get; set; }
        public bool SetupComplete { get; set; }

        public EthercatMotion Motion;
        public Stepper Steppers;
        public EthercatIo EcatIo;

        public bool SystemFault { get; set; }
        public bool ConveyorFault { get; set; }
        public bool ProductionFault { get; set; }

        public uint SlipInHeight { get; set; } = 12;
        #endregion

        #region ShieldBox
        private const bool CloseBoxAfterLoad = true;
        private const bool NotCloseBoxAfterLoad = false;

        public ShieldBox ShieldBox1 { get; set; }
        public ShieldBox ShieldBox2 { get; set; }
        public ShieldBox ShieldBox3 { get; set; }
        public ShieldBox ShieldBox4 { get; set; }
        public ShieldBox ShieldBox5 { get; set; }
        public ShieldBox ShieldBox6 { get; set; }

        private readonly object _shieldBoxsLocker = new object();

        public ShieldBox[] ShieldBoxs { get; set; }

        private bool _shieldBoxInstanced = false;

        /// <summary>
        /// Box3 can be danger for bining.
        /// </summary>
        public bool NoShieldBox3 { get; set; } = true;
        #endregion

        #region Tester
        public Tester Tester1 { get; set; }
        public Tester Tester2 { get; set; }
        public Tester Tester3 { get; set; }
        public Tester Tester4 { get; set; }
        public Tester Tester5 { get; set; }
        public Tester Tester6 { get; set; }

        public Tester[] Testers { get; set; }

        private bool _testerInstanced = false;
        #endregion
      
        #region Conveyor
        public Scanner Scanner { get; set; }

        public Conveyor Conveyor { get; set; }
        public bool ConveyorIsBusy { get; set; }

        private readonly ManualResetEvent _conveyorWorkingManualResetEvent = new ManualResetEvent(true);
        private readonly ManualResetEvent _conveyorPickReadyManualResetEvent = new ManualResetEvent(true);

        private Thread _conveyorManagerThread;
        #endregion

        #region PhoneServer

        /// <summary>
        /// Only RF test in a Rack.
        /// </summary>
        public bool OneTestInRack { get; set; } = false;

        public Phone GoldRf = new Phone()
        { Id = -1, SerialNumber="00000000001", GoldPhoneBusy=false,};
        public Phone GoldWifi = new Phone()
        { Id = -2, SerialNumber = "00000000001", GoldPhoneBusy = false };

        // Todo read xml?
        public long PhoneCount { get; set; } = 1;

        public long CurrentServedPhoneId { get; set; }

        /// <summary>
        /// Can be on conveyor or on gripper.
        /// </summary>
        public Phone LatestPhone { get; set; }

        private Thread _phoneServerThread;

        private readonly object _phoneToBeServedLocker = new object();

        /// <summary>
        /// Phones only which already has place to go can add to the list.
        /// </summary>
        public List<Phone> PhoneToBeServed = new List<Phone>();

        private readonly object _rfPhoneLocker = new object();

        public List<Phone> RfPhones = new List<Phone>();

        private readonly object _wifiPhoneLocker = new object();

        public List<Phone> WifiPhones = new List<Phone>();

        private readonly object _availableBoxLocker = new object();

        public List<ShieldBox> AvailableBox = new List<ShieldBox>();

        /// <summary>
        /// Set, to serve, if exception, has to set again to continue.
        /// </summary>
        public ManualResetEvent PhoneServerManualResetEvent = new ManualResetEvent(false);

        public RackTestMode WifiTestMode { get; set; } = RackTestMode.AB;

        public RackTestMode RfTestMode { get; set; } = RackTestMode.ABC;

        public RackTestMode BtTestMode { get; set; } = RackTestMode.AB;
        #endregion

        #region Events
        public delegate void ErrorOccuredEventHandler(object sender, int code, string description);

        public event ErrorOccuredEventHandler ErrorOccured;

        protected void OnErrorOccured(int code, string description)
        {
            ErrorOccured?.Invoke(this, code, description);
        }

        public delegate void WarningOccuredEventHandler(object sender, int code, string description);

        public event WarningOccuredEventHandler WarningOccured;

        protected void OnWarningOccured(int code, string description)
        {
            WarningOccured?.Invoke(this, code, description);
        }

        public delegate void InfoOccuredEventHandler(object sender, int code, string description);

        public event InfoOccuredEventHandler InfoOccured;

        protected void OnInfoOccured(int code, string description)
        {
            InfoOccured?.Invoke(this, code, description);
        }

        public delegate void ProductionCompleteEventHandler(object sender, bool pass, string sn, string footprint, string description);

        public event ProductionCompleteEventHandler ProductionComplete;

        protected void OnProductionComplete(bool pass, string sn, string footprint, string description)
        {
            ProductionComplete?.Invoke(this, pass, sn, footprint, description);
        }
        #endregion

    }
}
