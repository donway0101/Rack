using System;
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
        public bool RobotBining { get; set; } = false;

        public string PortName = "COM3";
        public ShieldBoxState State { get; set; } = ShieldBoxState.Close;
        public bool ReadyForTesting { get; set; }
        public int PassRate { get; set; }
        public int TestCount { get; set; }
        public int PassCount { get; set; }
        public bool Enabled { get; set; } = false;
        //Todo need to remember it?
        public bool Empty { get; set; } = true;
        /// <summary>
        /// Box is enabled and test is finished.
        /// </summary>
        /// Todo make sure set phone properties before set it available.
        public bool Available { get; set; }
        //Todo need to retry gold? remember it's fail time?
        public bool GoldPhoneChecked { get; set; }
        public ShieldBoxType Type { get; set; } = ShieldBoxType.Rf;
        public TargetPosition Position { get; set; } = new TargetPosition(){XPos = 400, ZPos = 700, YPos = 0};

        //After test, shield box will send back result and put phone to serve list.
        public Phone Phone { get; set; }

        public ShieldBox(int id)
        {
            Id = id;
        }

        public void Start(int serialBaudRate = 9600,
            Parity serialParity = Parity.None, int serialDataBit = 8,
            StopBits serialStopBits = StopBits.One)
        {
            //Stop();

            if (_serial==null)
            {
                _serial = new SerialPort(PortName, serialBaudRate, serialParity,
                        serialDataBit, serialStopBits)
                    { ReadTimeout = 1000 };
            }

            if (_serial.IsOpen==false)
            {
                _serial.Open();
                _serial.DataReceived -= _serial_DataReceived;
                _serial.DataReceived += _serial_DataReceived;
            }          
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
                Delay(50);
                _response = string.Empty;
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
        public void OpenBox(int timeout = 5000, bool setAvailable = true)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (RobotBining)
            {
                Delay(100);
                if (stopwatch.ElapsedMilliseconds > 120000)
                {
                    throw new Exception("OpenBox " + Id + " failed due to RobotBining.");
                }
            }

            try
            {
                SendCommand(ShieldBoxCommand.OPEN, ShieldBoxResponse.OpenSuccessful, timeout);
                State = ShieldBoxState.Open;
                ReadyForTesting = false;
                if (setAvailable)
                {
                    Available = true;
                }
            }
            catch (Exception e)
            {
                throw new BoxException("OpenBox " + Id + " timeout");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"> Takes less than 3 sec close</param>
        public void CloseBox(int timeout = 5000, bool changeState = true)
        {
            try
            {
                SendCommand(ShieldBoxCommand.CLOSE, ShieldBoxResponse.CloseSuccessful, timeout);
                State = ShieldBoxState.Close;
                if (changeState)
                {
                    ReadyForTesting = true;
                }
            }
            catch (Exception e)
            {
                throw new BoxException("CloseBox " + Id + " timeout");
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
                throw new BoxException("GreenLight " + Id + " timeout");
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
                throw new BoxException("RedLight " + Id + " timeout");
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
                throw new BoxException("YellowLight " + Id + " timeout");
            }
        }

        private void SendCommand(ShieldBoxCommand command, string response, int timeout = 5000)
        {
            //bool retryCmd = false;
            SendCmd(command);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (_response != response && _response != ShieldBoxResponse.ResponseEnding + response)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    throw new TimeoutException();
                }

                //if (stopwatch.ElapsedMilliseconds > timeout*0.8 && retryCmd == false)
                //{
                //    SendCmd(command);
                //    stopwatch.Restart();
                //    retryCmd = true;
                //}
                Delay(100);
            }
        }

        public bool IsClosed(int timeout = 1000)
        {
            SendCmd(ShieldBoxCommand.STATUS);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (_response != ShieldBoxResponse.BoxIsOpened && _response != ShieldBoxResponse.BoxIsClosed)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    return false;
                }
                Delay(100);
            }

            if (_response != ShieldBoxResponse.BoxIsClosed)
            {
                return false;
            }
            else
            {
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
