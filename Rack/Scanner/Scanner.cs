using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rack;

namespace Rack
{
    public class Scanner
    {
        private SerialPort _serial = null;
        private readonly object _sendLock = new object();
        private const string CmdEnding = "\r";
        private string _response;
        public string PortName { get; set; } = "COM12";
        public string SerialNumber { get; set; }
        public int SerialNumberLenght { get; set; } = 11;

        /// <summary>
        /// Set false after serial number is used.
        /// </summary>
        public bool ScanSuccessful { get; set; }

        public delegate void InfoOccuredEventHandler(object sender, int code, string description);

        public event InfoOccuredEventHandler InfoOccured;

        protected void OnInfoOccured(int code, string description)
        {
            InfoOccured?.Invoke(this, code, description);
        }

        public delegate void ErrorOccuredEventHandler(object sender, int code, string description);

        public event ErrorOccuredEventHandler ErrorOccured;

        protected void OnErrorOccured(int code, string description)
        {
            ErrorOccured?.Invoke(this, code, description);
        }

        public Scanner(string portName)
        {
            PortName = portName;
        }

        public void Start(int serialBaudRate = 9600,
            Parity serialParity = Parity.None, int serialDataBit = 8,
            StopBits serialStopBits = StopBits.One)
        {
            if (_serial == null)
            {
                _serial = new SerialPort(PortName, serialBaudRate, serialParity,
                        serialDataBit, serialStopBits)
                { ReadTimeout = 1000 };
            }

            if (_serial.IsOpen == false)
            {
                _serial.Open();
                _serial.DataReceived -= _serial_DataReceived;
                _serial.DataReceived += _serial_DataReceived;
            }
        }

        private void _serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _response = _serial.ReadExisting();

            if (_response.Length==SerialNumberLenght)
            {
                SerialNumber = _response;
                ScanSuccessful = true;
                OnInfoOccured(20029, "Scan successful, SN : " + _response);
            }
            else
            {
                ScanSuccessful = false;
                OnErrorOccured(40017, "Scan fail, SN : " + _response);
            }
        }

        //public void Stop()
        //{
        //    if (_serial != null)
        //    {
        //        _serial.Close();
        //        _serial.Dispose();
        //        Delay(500);
        //    }
        //}

        //public void SendCmd(ShieldBoxCommand command)
        //{
        //    lock (_sendLock)
        //    {
        //        Delay(50);
        //        _response = string.Empty;
        //        string cmd = command + CmdEnding;
        //        _serial.Write(cmd);
        //    }
        //}

        //private void Delay(int milliSecond)
        //{
        //    Thread.Sleep(milliSecond);
        //}

        //private void SendCommand(ShieldBoxCommand command, string response, int timeout = 5000)
        //{
        //    //bool retryCmd = false;
        //    SendCmd(command);
        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    while (_response != response && _response != ShieldBoxResponse.ResponseEnding + response)
        //    {
        //        if (stopwatch.ElapsedMilliseconds > timeout)
        //        {
        //            throw new TimeoutException();
        //        }

        //        Delay(100);
        //    }
        //}

    }
}
