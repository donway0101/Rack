using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TesterSimulator
{
    public class SocketClient
    {
        private Socket _clientSocket;
        private EndPoint _endPoint;
        private readonly int _port;
        private readonly Thread _messageReceiveThread;
        private readonly Thread _connectThread;
        private readonly ManualResetEvent _connectManualResetEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent _receiveManualResetEvent = new ManualResetEvent(false);
        private bool _connected = false;

        public SocketClient(int port)
        {
            _port = port;
            _connectThread = new Thread(Connect) { IsBackground = true };
            _messageReceiveThread = new Thread(ReceiveMessage) { IsBackground = true };
        }

        private void ReceiveMessage()
        {
            while (true)
            {
                _receiveManualResetEvent.WaitOne();
                try
                {
                    byte[] buffer = new byte[1024];
                    int rec = _clientSocket.Receive(buffer, 0, buffer.Length, 0);
                    if (rec <= 0)
                    {
                        throw new SocketException();
                    }
                    Array.Resize(ref buffer, rec);
                   string ClientReceivedMessage = Encoding.Default.GetString(buffer);
                }
                catch (Exception)
                {
                    _connected = false;
                    _receiveManualResetEvent.Reset();
                    _connectManualResetEvent.Set();
                }
            }
        }

        public void Start()
        {
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _endPoint = new IPEndPoint(IPAddress.Parse(GetLocalIpAddress()), _port);
            if (_connectThread.IsAlive == false)
            {
                _connectManualResetEvent.Set();
                _connectThread.Start();
            }
            if (_messageReceiveThread.IsAlive == false)
            {
                _messageReceiveThread.Start();
            }
        }

        public void SetFail()
        {
            Send("SetTestResult,1;");
        }
        public void SetPass()
        {
            Send("SetTestResult,0;");
        }

        private void Send(string cmd)
        {
            try
            {
                byte[] buffer = Encoding.Default.GetBytes(cmd);
                _clientSocket.Send(buffer, 0, buffer.Length, 0);
            }
            catch (Exception ex)
            {
                _connected = false;
                _receiveManualResetEvent.Reset();
                _connectManualResetEvent.Set();
            }
        }

        private void Connect()
        {
            while (true)
            {
                _connectManualResetEvent.WaitOne();
                Thread.Sleep(500);
                try
                {
                    if (_connected == false)
                    {
                        _clientSocket.Close();
                        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        _clientSocket.Connect(_endPoint);
                        _receiveManualResetEvent.Set();
                        _connectManualResetEvent.Reset();
                        _connected = true;
                    }
                }
                catch (Exception e)
                {
                }
            }
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

            if (ip == string.Empty)
            {
                throw new Exception("GetLocalIpAddress fail.");
            }
            return ip;
        }
    }
}
