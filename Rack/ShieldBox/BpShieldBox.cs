﻿using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Rack
{
    public class BpShieldBox
    {
        private readonly SerialPort _serial = null;
        private readonly object _sendLock = new object();
        private const string CmdEnding = "\r";
        private string _response;
        public readonly int Id;

        public State State { get; set; } = State.Close;
        public int PassRate { get; set; }
        public int TestCount { get; set; }
        public int PassCount { get; set; }
        public bool Enabled { get; set; }
        public bool Empty { get; set; }
        public bool Available { get; set; }
        /// <summary>
        /// Checked by golden phone.
        /// </summary>
        public bool Golded { get; set; }

        public BpShieldBox(int id, string serialPortName, int serialBaudRate = 9600,
            Parity serialParity = Parity.None, int serialDataBit = 8,
            StopBits serialStopBits = StopBits.One)
        {
            _serial = new SerialPort(serialPortName, serialBaudRate, serialParity,
                    serialDataBit, serialStopBits)
                { ReadTimeout = 1000};
            Id = id;
        }

        public void Start()
        {
            _serial.Open();
            _serial.DataReceived += _serial_DataReceived;
        }

        private void _serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _response += _serial.ReadExisting();
        }

        public void Stop()
        {
            _serial.Close();
            _serial.Dispose();
        }

        public void SendCmd(Command command)
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
                SendCommand(Command.OPEN, Response.OpenSuccessful, timeout);
                State = State.Open;
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
                SendCommand(Command.CLOSE, Response.CloseSuccessful, timeout);
                State = State.Close;
            }
            catch (Exception e)
            {
                throw new Exception("CloseBox " + Id + " timeout");
            }            
        }

        public void GreenLight(int timeout = 5000)
        {
            try
            {
                SendCommand(Command.PASS, Response.LightOnSuccessful, timeout);
            }
            catch (Exception e)
            {
                throw new Exception("GreenLight " + Id + " timeout");
            }
        }

        private void SendCommand(Command command, string response, int timeout = 5000)
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
            SendCmd(Command.STATUS);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (_response != Response.BoxIsOpened & _response != Response.BoxIsClosed)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    _response = String.Empty;
                    throw new TimeoutException();
                }
                Delay(100);
            }

            if (_response != Response.BoxIsClosed)
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
