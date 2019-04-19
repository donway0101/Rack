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

        public string Ip { get; set; }
        public int Id { get; set; }
        public ShieldBox ShieldBox { get; set; }
        public int RobotState { get; set; } 

        public delegate void MessageReceivedEventHandler(object sender, string message);

        public event MessageReceivedEventHandler MessageReceived;

        protected void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }

        public delegate void InfoOccuredEventHandler(object sender, int code, string description);

        public event InfoOccuredEventHandler InfoOccured;

        protected void OnInfoOccured(int code, string description)
        {
            InfoOccured?.Invoke(this, code, description);
        }

        public Tester(string ip, int portNum)
        {
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
                //Todo need to new a thread?
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
                    OnInfoOccured(4, "Client may disconnect" + e.Message);
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
                        OnInfoOccured(4, "Client may disconnect" + e.Message);
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
                    OnInfoOccured(4, "Receive unknown message " + message);
                    return;
                }

                int index = message.IndexOf(";", StringComparison.Ordinal);
                if (index+1 != message.Length)
                {
                    OnInfoOccured(4, "Receive unknown message " + message);
                    return;
                }

                message = message.Replace(";", string.Empty);

                string[] subMessage = message.Split(',');

                //Todo, is message is not continue, could lost some information.               
                Enum.TryParse(subMessage[0], out TesterCommand command);
                switch (command)
                {
                    case TesterCommand.GetRobotState:
                        //Todo update state
                        SendMessage(TesterCommand.GetRobotState + "," + RobotState + ";");
                        break;
                    case TesterCommand.GetShieldedBoxState:
                        //Todo send state.
                        int state = (int) ShieldBox.State;
                        SendMessage(TesterCommand.GetShieldedBoxState + ","+ state + ";");
                        break;
                    case TesterCommand.SetTestResult:
                        //Todo set result
                        try
                        {
                            ShieldBox.Phone.TestResult = TestResult.Fail;
                            ShieldBox.Phone.FailCount++;

                            SendMessage(TesterCommand.SetTestResult + ",OK;");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                                               
                        break;
                    default:
                        OnInfoOccured(4,"Receive unknown message " + message);
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
