﻿using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Rack
{
    public class ShieldBox
    {
        private SerialPort _serial = null;
        private readonly object _sendLock = new object();
        private const string CmdEnding = "\r";
        private string _response;
        /// <summary>
        /// Match position of box, no matter what kind of box it is.
        /// </summary>
        public long Id { get; set; }
        public string PortName = "COM3";
        public ShieldBoxState State { get; set; } = ShieldBoxState.Close;
        public int PassRate { get; set; }
        public int TestCount { get; set; }
        public int PassCount { get; set; }
        public bool Enabled { get; set; } = false;
        public bool Empty { get; set; }
        /// <summary>
        /// Box is enabled and test is finished.
        /// </summary>
        /// Todo make sure set phone properties before set it available.
        public bool Available { get; set; }
        //Todo need to retry gold? remember it's fail time?
        public bool GoldPhoneChecked { get; set; }
        public ShieldBoxType Type { get; set; } = ShieldBoxType.RF;
        public TargetPosition Position { get; set; }
        public Phone Phone { get; set; }

        public ShieldBox(int id)
        {
            Id = id;
        }

        public void Start(int serialBaudRate = 9600,
            Parity serialParity = Parity.None, int serialDataBit = 8,
            StopBits serialStopBits = StopBits.One)
        {
            Stop();
            
            _serial = new SerialPort(PortName, serialBaudRate, serialParity,
                    serialDataBit, serialStopBits)
                { ReadTimeout = 1000 };
            _serial.Open();
            _serial.DataReceived += _serial_DataReceived;
        }

        private void _serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _response += _serial.ReadExisting();
        }

        public void Stop()
        {
            if (_serial != null)
            {
                _serial.Close();
                _serial.Dispose();
                Delay(500);
            }
        }

        public void SendCmd(ShieldBoxCommand command)
        {
            lock (_sendLock)
            {
                string cmd = command + CmdEnding;
                _serial.Write(cmd);
            }            
        }

        private void Delay(int milliSecond)
        {
            Thread.Sleep(milliSecond);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout">Takes less than 3 sec to open</param>
        public void OpenBox(int timeout = 5000)
        {
            try
            {
                SendCommand(ShieldBoxCommand.OPEN, ShieldBoxResponse.OpenSuccessful, timeout);
                State = ShieldBoxState.Open;
            }
            catch (Exception e)
            {
                throw new Exception("OpenBox " + Id + " timeout");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"> Takes less than 3 sec close</param>
        public void CloseBox(int timeout = 5000)
        {
            try
            {
                SendCommand(ShieldBoxCommand.CLOSE, ShieldBoxResponse.CloseSuccessful, timeout);
                State = ShieldBoxState.Close;
            }
            catch (Exception e)
            {
                throw new Exception("CloseBox " + Id + " timeout");
            }            
        }

        public void GreenLight(int timeout = 1000)
        {
            try
            {
                SendCommand(ShieldBoxCommand.PASS, ShieldBoxResponse.LightOnSuccessful, timeout);
            }
            catch (Exception e)
            {
                throw new Exception("GreenLight " + Id + " timeout");
            }
        }

        public void RedLight(int timeout = 1000)
        {
            try
            {
                SendCommand(ShieldBoxCommand.FAIL, ShieldBoxResponse.LightOnSuccessful, timeout);
            }
            catch (Exception e)
            {
                throw new Exception("RedLight " + Id + " timeout");
            }
        }

        public void YellowLight(int timeout = 1000)
        {
            try
            {
                SendCommand(ShieldBoxCommand.TESTING, ShieldBoxResponse.LightOnSuccessful, timeout);
            }
            catch (Exception e)
            {
                throw new Exception("YellowLight " + Id + " timeout");
            }
        }

        private void SendCommand(ShieldBoxCommand command, string response, int timeout = 5000)
        {
            SendCmd(command);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (_response != response)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    _response = String.Empty;
                    throw new TimeoutException();
                }
                Delay(100);
            }
            //long sec = stopwatch.ElapsedMilliseconds; // For time consumption command testing
            _response = String.Empty;
        }

        public bool BoxIsClosed(int timeout = 1000)
        {
            SendCmd(ShieldBoxCommand.STATUS);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (_response != ShieldBoxResponse.BoxIsOpened & _response != ShieldBoxResponse.BoxIsClosed)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    _response = String.Empty;
                    throw new TimeoutException();
                }
                Delay(100);
            }

            if (_response != ShieldBoxResponse.BoxIsClosed)
            {
                _response = String.Empty;
                return false;
            }
            else
            {
                _response = String.Empty;
                return true;
            }           
        }

        public Task<bool> CloseBoxAsync()
        {
            return Task.Factory.StartNew(() => true);
        }

        public Task<bool> OpenBoxAsync()
        {
            return Task.Factory.StartNew(() => true);
        }
    }
}
