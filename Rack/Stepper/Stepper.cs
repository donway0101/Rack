using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rack
{
    public class Stepper
    {
        #region Private member
        /// <summary>
        /// RS232 communicate with Moons driver.
        /// </summary>
        private SerialPort SerialPort;

        /// <summary>
        /// Serial port name, eg. "COM3"
        /// </summary>
        private string PortName;

        /// <summary>
        /// Only one command sent to driver in a time.
        /// </summary>
        private static readonly object PortWriteLocker = new object();

        /// <summary>
        /// RS232 communication is slow, and it may not send all message in a time.
        /// </summary>
        private string Response;

        /// <summary>
        /// Every single time Host send a command, it should check for driver response.
        /// </summary>
        private bool DriverResponsed = false;

        /// <summary>
        /// 3600 counts electric pulse = 1 round
        /// </summary>
        private double ElectronicGearing = 3600.0;

        /// <summary>
        /// Load is driven by gear and belt.
        /// </summary>
        private double GearRatio = 60.0/15.0;

        /// <summary>
        /// Motor counts per degree, including transimission factor.
        /// </summary>
        private readonly double _countPerDegree = 0.0;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="portName">Rs232 Com port number</param>
        public Stepper(string portName)
        {
            PortName = portName;
            _countPerDegree = ElectronicGearing * GearRatio / 360.0;
        }

        /// <summary>
        /// Open serial port.
        /// </summary>
        public void Setup()
        {
            try
            {
                Close();
                Thread.Sleep(500);
                SerialPort = new SerialPort(PortName, 38400, Parity.None, 8, StopBits.One);
                SerialPort.Open();
                SerialPort.DataReceived += SerialPort_DataReceived;
                Thread.Sleep(300);

                Connect();
            }
            catch (Exception ex)
            {
                throw new Exception("Stepper motor setup error: " + ex.Message);
            }
        }


        public void Connect()
        {
            try
            {
                SendCommand(RackGripper.One, "IFD");
                SendCommand(RackGripper.Two, "IFD");
            }
            catch (Exception)
            {
                throw new Exception("Not all stepper's connection is good.");
            }
        }

        private void SendCommand(RackGripper motor, string cmd)
        {
            string response;
            bool idMatch, endMatch, result;
            try
            {
                response = SendCmd(motor, cmd);
                idMatch = response.Substring(0, 1) == GetMotorId(motor);
                endMatch = response.Substring(1, 1) == "%" || response.Substring(1, 1) == "*";
                result = idMatch && endMatch;
                if (result == false)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                response = SendCmd(motor, cmd);
                idMatch = response.Substring(0, 1) == GetMotorId(motor);
                endMatch = response.Substring(1, 1) == "%" || response.Substring(1, 1) == "*";
                result = idMatch && endMatch;
                if (result == false)
                {
                    throw new Exception("Send command:" + cmd + " to motor " + motor + " failed.");
                }
            }
        }

        /// <summary>
        /// Close serial port.
        /// </summary>
        public void Close()
        {
            try
            {
                if (SerialPort != null)
                {
                    SerialPort.Close();
                    SerialPort.Dispose();
                }            
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Set driver feedback format to decimal.
        /// </summary>
        /// <param name="motor"></param>
        //public void SetFeedbackFormatDecimal(RackGripper motor)
        //{
        //    string res = SendCmd(motor, "IFD");
        //    if (MotorAcknowledged(motor, res) == false)
        //    {
        //        throw new Exception("Drive is NOT acknowledged");
        //    }
        //}

        /// <summary>
        /// Store response from driver.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Response += SerialPort.ReadExisting();

            //A whole response from driver should end with carriage return.
            if (Response.Contains("\r"))
            {
                //Console.WriteLine(PartialResponse);
                DriverResponsed = true;
            }
        }

        /// <summary>
        /// Check return value of RS232.
        /// </summary>
        /// <returns></returns>
        private string GetResponse()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (DriverResponsed == false)
            {
                if (stopwatch.ElapsedMilliseconds > 300) //Should response within 2 second.
                {                 
                    throw new Exception("Driver response timeout.");
                }
                Thread.Sleep(10);
            }

            return Response.Replace("\r", "");
        }

        /// <summary>
        /// Convert motor enum to ID string.
        /// </summary>
        /// <param name="motor"></param>
        /// <returns></returns>
        private string GetMotorId(RackGripper motor)
        {
            return Convert.ToString((int)motor);
        }

        public void Stop(RackGripper motor)
        {
            try
            {
                SendCommand(motor, "STD");
            }
            catch (Exception)
            {
                throw new Exception("Drive is can NOT stopped.");
            }
        }

        /// <summary>
        /// Send command to motor through serial port, and wait for response.
        /// </summary>
        /// <param name="cmd"></param>
        public string SendCmd(RackGripper motor,string cmd, int retryTimes = 5)
        {
            lock (PortWriteLocker)
            {
                int failCount = 0;               
                string command = GetMotorId(motor) + cmd + "\r"; //Add a carriage return.
                byte[] buffer = Encoding.ASCII.GetBytes(command);

                while (true)
                {
                    try
                    {                     
                        DriverResponsed = false;
                        Response = string.Empty;
                        SerialPort.Write(buffer, 0, buffer.Length);
                        return GetResponse();
                    }
                    catch (Exception)
                    {
                        failCount++;
                        Thread.Sleep(100);
                        if (failCount> retryTimes)
                        {
                            throw new Exception("Stepper command:" + command + " get no response.");
                        }                  
                    }
                }
                
            }
        }

        /// <summary>
        /// Enalbe motor.
        /// </summary>
        /// <param name="motor"></param>
        public void Enable(RackGripper motor)
        {
            try
            {
                if (GetStatus(motor, StatusCode.Enabled) == true)
                {
                    return;
                }

                if (GetStatus(motor, StatusCode.Alarm) == true)
                {
                    ResetAlarm(motor);
                }

                SendCommand(motor, "ME");

                if (GetStatus(motor, StatusCode.Enabled) == false)
                {
                    throw new Exception(motor + " can not be enabled");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Enable motor fail due to:" + ex.Message);
            }
        }

        /// <summary>
        /// Check if motor receives command.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private bool MotorAcknowledged(RackGripper motor, string response)
        {
            try
            {
                bool idMatch = response.Substring(0, 1) == GetMotorId(motor);
                bool endMatch = response.Substring(1, 1) == "%" || response.Substring(1, 1) == "*";
                return idMatch && endMatch;
            }
            catch (Exception)
            {
                return false;
            }                      
        }

        /// <summary>
        /// Disable motor.
        /// </summary>
        /// <param name="motor"></param>
        public void Disable(RackGripper motor)
        {
            SendCommand(motor, "MD");

            if (GetStatus(motor, StatusCode.Enabled) == true)
            {
                throw new Exception("Motor is not disabled");
            }
        }

        /// <summary>
        /// Find reference point of motor, AKA home position.
        /// Must has the right sensor for homing.
        /// </summary>
        /// <param name="motor"></param>
        public void HomeMotor(RackGripper motor, double homeOffset, int defaultWorkingSpeed=10)
        {
            SetSpeed(motor, 1); //Default homing speed.

            if (GetStatus(motor, StatusCode.Enabled) == false)
            {
                Enable(motor);
            }

            if (GetInput(motor, InputStepper.X1) == false)
            {
                //If home sensor has been triggered before homing.
                // move out first.
                ToPointRelative(motor, -60);           
            }

            SendCommand(motor, "SH1H");

            Thread.Sleep(50);
            WaitCondition(motor, StatusCode.Homing, false);
            WaitCondition(motor, StatusCode.Inpos, true);

            ToPointRelative(motor, homeOffset);

            SendCommand(motor, "EP0");
            SendCommand(motor, "SP0");
            SetSpeed(motor, defaultWorkingSpeed);
        }

        /// <summary>
        /// Move motor to specific angle.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="angle"></param>
        public void ToPoint(RackGripper motor, double angle)
        {
            int target = Convert.ToInt32(angle * _countPerDegree);
            SendCommand(motor, "FP" + target);

            if (GetStatus(motor, StatusCode.Alarm))
            {
                throw new Exception("Drive alarm");
            }

            Thread.Sleep(50);
        }

        public void ToPointWaitTillEnd(RackGripper motor, double angle)
        {
            int target = Convert.ToInt32(angle * _countPerDegree);
            SendCommand(motor, "FP" + target);

            Thread.Sleep(50);
            WaitTillEnd(motor, angle);
        }

        /// <summary>
        /// Move motor to specific angle relative to current postion.
        ///  allow error: 0.5 degree.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="angle"></param>
        public void ToPointRelative(RackGripper motor, double angle, int timeout = 10)
        {
            double lastPos = GetPosition(motor);

            int target = Convert.ToInt32(angle * _countPerDegree);
            SendCommand(motor, "FL" + target);

            Thread.Sleep(50);
            WaitTillEnd(motor, lastPos + angle, timeout);        
        }

        /// <summary>
        /// Wait motor get in position.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="targetAngle"></param>
        /// <param name="timeout"></param>
        public void WaitTillEnd(RackGripper motor, double targetAngle, int timeout = 10)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            bool inPos = false;
            while (inPos == false)
            {
                if (stopwatch.ElapsedMilliseconds > timeout * 1000) //Should response within 2 second.
                {
                    throw new Exception("Motor in position timeout.");
                }

                inPos = GetStatus(motor, StatusCode.Inpos);

                Thread.Sleep(20);
            }

            if (GetStatus(motor, StatusCode.Enabled) == false)
            {
                throw new Exception("Stepper motor " + motor + " is disabled.");
            }

            double currentPos = GetPosition(motor);
            if (Math.Abs(currentPos - targetAngle) > 0.5)
            {
                throw new Exception("Position error is bigger than 0.5 degree.");
            }
        }

        public void CheckEnabled()
        {
            if (GetStatus(RackGripper.One, StatusCode.Enabled) == false)
            {
                throw new Exception("Stepper motor One is disabled.");
            }

            if (GetStatus(RackGripper.Two, StatusCode.Enabled) == false)
            {
                throw new Exception("Stepper motor two is disabled.");
            }
        }

        /// <summary>
        /// Wait motor status to be true.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="code"></param>
        /// <param name="condition"></param>
        /// <param name="timeout"></param>
        private void WaitCondition(RackGripper motor, StatusCode code, bool condition,  int timeout = 20)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            bool status = false;
            while (status != condition)
            {
                if (stopwatch.ElapsedMilliseconds > timeout * 1000) //Should response within 2 second.
                {
                    throw new Exception("Condition " + code + " timeout.");
                }

                status = GetStatus(motor, code);

                Thread.Sleep(20);
            }
        }

        /// <summary>
        /// Set acceleration and deceleration of motor.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="acceleration"> degree / sec / sec </param>
        public void SetAcceleration(RackGripper motor, int acceleration = 30)
        {
            // rev / sec / sec
            //double acc = GearRatio * acceleration / 360.0;
            //// AC - Acceleration Rate 0.167 to 5461.167 (resolution is 0.167 rps/s)
            //int times = Convert.ToInt16(acc / 0.167);
            //times++;
            //acc = Convert.ToDouble(times) * 0.167;

            //string accStr = acc.ToString("0.000");
            SendCommand(motor, "AC" + acceleration);
            SendCommand(motor, "DE" + acceleration);
        }

        /// <summary>
        /// Set speed of motor.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="velocity">rps</param>
        public void SetVelocity(RackGripper motor, int velocity = 30)
        {
            //double vel = GearRatio * velocity / 360.0;

            // ST-Q/Si, ST-S , STM, STAC5: 0.0042 - 80.0000 (resolution is 0.0042)
            //int times = Convert.ToInt16(vel / 0.0042);
            //times++;
            //vel = Convert.ToDouble(times) * 0.0042;
            //string velStr = vel.ToString("0.0000");

            SendCommand(motor, "VE" + velocity);
        }

        public void SetSpeed(RackGripper motor, int speed = 30)
        {
            SetVelocity(motor, speed);
            SetAcceleration(motor, speed);
        }

        /// <summary>
        /// Reset all alarm of motor.
        /// </summary>
        /// <param name="motor"></param>
        public void ResetAlarm(RackGripper motor)
        {
            SendCommand(motor, "AR");

            Thread.Sleep(20);

            if (GetStatus(motor, StatusCode.Alarm) == true)
            {
                throw new Exception("Drive's alarm can not be reset");
            }

            SendCommand(motor, "ME");

            if (GetStatus(motor, StatusCode.Enabled) == false)
            {
                throw new Exception("Drive is can not be enabled.");
            }
        }

        /// <summary>
        /// Asks the drive to respond with what it’s doing
        /// </summary>
        /// <param name="motor"></param>
        public bool GetStatus(RackGripper motor, StatusCode status)
        {
            string info = string.Empty;
            string code;
            string binaryValue;

            try
            {
                info = SendCmd(motor, "SC");
                code = info.Substring(4, info.Length - 4);
                binaryValue = Convert.ToString(Convert.ToInt32(code, 16), 2);
            }
            catch (Exception)
            {
                try
                {
                    Thread.Sleep(100);
                    info = SendCmd(motor, "SC");
                    code = info.Substring(4, info.Length - 4);
                    binaryValue = Convert.ToString(Convert.ToInt32(code, 16), 2);
                }
                catch (Exception)
                {
                    try
                    {
                        Thread.Sleep(100);
                        info = SendCmd(motor, "SC");
                        code = info.Substring(4, info.Length - 4);
                        binaryValue = Convert.ToString(Convert.ToInt32(code, 16), 2);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("GetStatus fail due to:" + ex.Message + " response is: " + info);
                    }
                }
            }

            bool[] result = binaryValue.Select(c => c == '1').ToArray();
            Array.Reverse(result);

            try
            {
                return result[(int)status];
            }
            catch (Exception)
            {
                return false;
            }         
        }

        /// <summary>
        /// Get state of input pin of driver.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool GetInput(RackGripper motor, InputStepper input)
        {
            string res = SendCmd(motor, "IS");
            if (res.Length != 12)
            {
                res = SendCmd(motor, "IS");
                if (res.Length != 12)
                {
                    throw new Exception("Input status response length error");
                }
            }

            return res.Substring(12 - (int)input, 1) == "0";
        }

        /// <summary>
        /// Get current position of motor.
        /// </summary>
        /// <param name="motor"></param>
        /// <returns></returns>
        public double GetPosition(RackGripper motor)
        {
            // Response of "IP" is always hexadecimal
            string res;
            string posString;
            int pos = 0;
            int failCount = 0;

            while (true)
            {
                try
                {
                    res = SendCmd(motor, "IP");
                    posString = res.Substring(4, res.Length - 4);
                    pos = Convert.ToInt32(posString);
                    break;
                }
                catch (Exception e)
                {
                    failCount++;
                    if (failCount>3)
                    {
                        throw new Exception("GetPosition of " + motor + " failed." + e.Message);
                    }
                }
            }
               
            return pos / _countPerDegree;
        }

    }
}
