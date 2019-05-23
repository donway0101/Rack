using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rack
{
    public class Tester
    {
        private readonly Socket _listeningSocket;
        private Socket _acceptSocket;
        private readonly int _portNum;
        private bool _stop;
        private readonly Thread _messageReceiveThread;
        private string _receivedMessage;
        private readonly object _sendLocker = new object();
        private bool _acceptingConnection;
        private readonly ManualResetEvent _socketManualResetEvent = new ManualResetEvent(false);
        private bool _connected;

        public string Ip { get; set; }
        public int Id { get; set; }
        public ShieldBox ShieldBox { get; set; }
        public int RobotState { get; set; } = 2; //Ready.
        public bool SimulateMode { get; set; }
        public string SimulateSerialNumber { get; set; } = "DefaultSerial";

    public bool Connected
        {
            get { return _connected; }
            set {
                _connected = value;
                if (ShieldBox!=null)
                {
                    ShieldBox.TesterComputerConnected = value;
                }
            }
        }

        public delegate void MessageReceivedEventHandler(object sender, string message);

        public event MessageReceivedEventHandler MessageReceived;

        protected void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }

        public delegate void ErrorOccuredEventHandler(object sender, int code, string description);

        public event ErrorOccuredEventHandler ErrorOccured;

        protected void OnErrorOccured(int code, string description)
        {
            ErrorOccured?.Invoke(this, code, description);
        }

        public delegate void InfoOccuredEventHandler(object sender, int code, string description);

        public event InfoOccuredEventHandler InfoOccured;

        protected void OnInfoOccured(int code, string description)
        {
            InfoOccured?.Invoke(this, code, description);
        }

        public Tester(int id, string ip, int portNum)
        {
            Id = id;
            _portNum = portNum;
            _messageReceiveThread = new Thread(ReceiveMessage){IsBackground = true};

            _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _acceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _listeningSocket.Bind(new IPEndPoint(IPAddress.Parse(GetLocalIpAddress()), _portNum));
            _listeningSocket.Listen(0);
        }

        public void Start()
        {
            Task.Run(() => { AcceptClient(); });
           
            _stop = false;
            if (_messageReceiveThread.IsAlive == false)
            {
                _messageReceiveThread.Start();
            }
        }

        public void Stop()
        {
            _stop = true;
        }

        private void AcceptClient()
        {
            if (_acceptingConnection)
            {
                return;
            }

            _acceptingConnection = true;

            if (_listeningSocket.Connected)
            {
                _listeningSocket.Close();
            }

            if (_acceptSocket.Connected)
            {
                _acceptSocket.Close();
            }

            _socketManualResetEvent.Reset();
            _acceptSocket = _listeningSocket.Accept();
            Connected = true;
            _socketManualResetEvent.Set();

            _acceptingConnection = false;
        }

        private void ReceiveMessage()
        {
            string message = string.Empty;
            while (!_stop)
            {
                _socketManualResetEvent.WaitOne();
                try
                {
                    byte[] buffer = new byte[1024];
                    int rec = _acceptSocket.Receive(buffer, 0, buffer.Length, 0);
                    if (rec <= 0)
                        throw new SocketException();
                    Array.Resize(ref buffer, rec);
                    message = Encoding.Default.GetString(buffer);
                    MessageParser(message);                  
                    OnMessageReceived(message);
                }
                catch (Exception e)
                {
                    OnErrorOccured(40005, "Client may disconnect" + e.Message);
                    Connected = false;
                    AcceptClient();
                }
            }
        }

        private void SendMessage(string message)
        {
            Task.Run(() =>
            {
                lock (_sendLocker)
                {
                    try
                    {
                        byte[] msg = Encoding.ASCII.GetBytes(message);
                        _acceptSocket.Send(msg, msg.Length, 0);
                    }
                    catch (Exception e)
                    {
                        OnErrorOccured(40004, "Send message to Tester "+ Id + " fail." + e.Message);
                        AcceptClient();
                    }
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// If message is not complete use: _receivedMessage += message;
        ///  if message is too long and wrong, empty _receivedMessage
        /// <param name="message"></param>
        private void MessageParser(string message)
        {
            Task.Run(() =>
            {
                if (message.Contains(";") == false)
                {
                    OnErrorOccured(40003, "Receive unknown message " + message);
                    return;
                }

                int index = message.IndexOf(";", StringComparison.Ordinal);
                if (index+1 != message.Length)
                {
                    OnErrorOccured(40003, "Receive unknown message " + message);
                    return;
                }

                message = message.Replace(";", string.Empty);

                string[] subMessage = message.Split(',');

                //Todo, is message is not continue, could lost some information.               
                Enum.TryParse(subMessage[0], out TesterCommand command);
                switch (command)
                {
                    case TesterCommand.GetRobotState:
                        OnInfoOccured(20001, "Tester " + Id + " requesting Robot state.");
                        SendMessage(TesterCommand.GetRobotState + "," + RobotState + ";");
                        OnInfoOccured(20001, "Rack response Robot state to Tester " + Id +".");
                        break;
                    case TesterCommand.GetShieldedBoxState:
                        OnInfoOccured(20002,"Tester " + Id + " requesting shieldedBox State.");
                        int state = 0;
                        if (ShieldBox.ReadyForTesting)
                        {
                            state = (int)ShieldBoxState.Close;
                        }
                        else
                        {
                            state = (int)ShieldBoxState.Open;
                        }

                        if (SimulateMode)
                        {
                            SendMessage(TesterCommand.GetShieldedBoxState + ",2;");
                        }
                        else
                        {
                            SendMessage(TesterCommand.GetShieldedBoxState + "," + state + ";");
                        }
                        
                        OnInfoOccured(20002, "Rack response shieldedBox state to Tester " + Id + ".");
                        break;
                    case TesterCommand.SetTestResult:
                        OnInfoOccured(20003, "Tester " + Id + " send test result: " + subMessage[1] + ".");
                        if (ShieldBox.Phone != null)
                        {
                            ShieldBox.Phone.TestCycleTimeStopWatch.Stop();
                        }
                        try
                        {
                            SendMessage(TesterCommand.SetTestResult + ",OK;");
                            OnInfoOccured(20003, "Rack response send test result to Tester " + Id + ".");

                            if (ShieldBox.Phone.AutoOpenBox)
                            {
                                ShieldBox.OpenBox();
                            }

                            switch (subMessage[1])
                            {
                                case "0":                                   
                                    ShieldBox.Phone.TestResult = TestResult.Pass;                                    
                                    break;

                                case "1":
                                    ShieldBox.Phone.TestResult = TestResult.Fail;
                                    ShieldBox.Phone.FailCount++;
                                    break;

                                case "2":
                                    //Battery low. NG.
                                    ShieldBox.Phone.TestResult = TestResult.Fail;
                                    ShieldBox.Phone.FailCount = 3;
                                    break;

                                case "3":
                                    //Procedure not match.
                                    ShieldBox.Phone.TestResult = TestResult.Fail;
                                    ShieldBox.Phone.FailCount = 3;
                                    break;

                                case "4":
                                    //Timeout match.
                                    ShieldBox.Phone.TestResult = TestResult.Fail;
                                    ShieldBox.Phone.FailCount = 3;
                                    break;

                                default:
                                    break;
                            }                          
                        }
                        catch (BoxException)
                        {
                            OnErrorOccured((int)Error.OpenBoxFail, "Open box failed.");
                        }
                        catch (Exception ex)
                        {
                            OnErrorOccured(40002, "SetTestResult error: " + ex.Message);
                        }
                        break;

                    case TesterCommand.GetSerialNumber:
                        OnInfoOccured(20028, "Tester " + Id + " requesting Serial Number.");
                        if (SimulateMode)
                        {                          
                            SendMessage(TesterCommand.GetSerialNumber + "," + SimulateSerialNumber + ";");
                        }
                        else
                        {
                            SendMessage(TesterCommand.GetSerialNumber + "," + ShieldBox.Phone.SerialNumber + ";");
                        }
                        OnInfoOccured(20028, "Rack response Get Serial Number to Tester " + Id + ".");
                        break;

                    default:
                        OnErrorOccured(40003,"Receive unknown message " + message);
                        break;
                }

            });
        }

        private string GetLocalIpAddress()
        {
            string ip = string.Empty;
            foreach (IPAddress ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ipAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    ip = ipAddress.ToString();
                }
            }

            if (ip==string.Empty)
            {
                throw new Exception("GetLocalIpAddress fail.");
            }
            return ip;
        }
    }
}
