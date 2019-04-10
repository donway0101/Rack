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
        /// Information sent from the driver.
        /// </summary>
        private string Response;

        /// <summary>
        /// RS232 communication is slow, and it may not send all message in a time.
        /// </summary>
        private string PartialResponse;

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

        public bool StepperOneIsConnected { get; set; }

        public bool StepperTwoIsConnected { get; set; }

        public bool StepperIsConnected { get; set; }

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
            Close();
            Thread.Sleep(500);
            SerialPort = new SerialPort(PortName, 9600, Parity.None, 8, StopBits.One);
            SerialPort.Open();
            SerialPort.DataReceived += SerialPort_DataReceived;
            Thread.Sleep(300);

            Connect();
        }


        public void Connect()
        {
            string res = SendCommand(StepperMotor.One, "IFD");
            StepperOneIsConnected = MotorAcknowledged(StepperMotor.One, res);

            res = SendCommand(StepperMotor.Two, "IFD");
            StepperTwoIsConnected = MotorAcknowledged(StepperMotor.Two, res);

            StepperIsConnected = StepperOneIsConnected & StepperTwoIsConnected;

            if (StepperIsConnected == false)
            {
                throw new Exception("Not all stepper's connection is good.");
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
        public void SetFeedbackFormatDecimal(StepperMotor motor)
        {
            string res = SendCommand(motor, "IFD");
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }
        }

        /// <summary>
        /// Store response from driver.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            PartialResponse += SerialPort.ReadExisting();

            //A whole response from driver should end with carriage return.
            if (PartialResponse.Contains("\r"))
            {
                Console.WriteLine(PartialResponse);
                Response = PartialResponse;
                PartialResponse = string.Empty;
                DriverResponsed = true;
            }
        }

        /// <summary>
        /// Send command to motor through serial port.
        /// </summary>
        /// <param name="cmd"></param>
        public void SendCommand(string cmd)
        {
            lock (PortWriteLocker)
            {            
                try
                {
                    cmd += "\r"; //Add a carriage return.
                    byte[] buffer = Encoding.ASCII.GetBytes(cmd);
                    SerialPort.Write(buffer, 0, buffer.Length);
                    DriverResponsed = false;
                }
                catch (Exception)
                {
                    throw;
                }
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

                Thread.Sleep(5);
            }
            return Response.Replace("\r", "");
        }

        /// <summary>
        /// Convert motor enum to ID string.
        /// </summary>
        /// <param name="motor"></param>
        /// <returns></returns>
        private string GetMotorId(StepperMotor motor)
        {
            return Convert.ToString((int)motor);
        }

        public void Stop(StepperMotor motor)
        {
            string res = SendCommand(motor, "STD");

            if (MotorAcknowledged(motor, res) != true)
            {
                throw new Exception("Drive is NOT enabled");
            }
        }

        /// <summary>
        /// Send command to motor through serial port, and wait for response.
        /// </summary>
        /// <param name="cmd"></param>
        /// <seealso cref="SendCommand"/>
        public string SendCommand(StepperMotor motor,string cmd)
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
                        SerialPort.Write(buffer, 0, buffer.Length);
                        return GetResponse();
                    }
                    catch (Exception)
                    {
                        failCount++;
                        Thread.Sleep(50);
                        PartialResponse = string.Empty;
                        if (failCount>5)
                        {
                            throw new Exception(command + " get no response.");
                        }                  
                    }
                }
                
            }
        }

        /// <summary>
        /// Enalbe motor.
        /// </summary>
        /// <param name="motor"></param>
        public void Enable(StepperMotor motor)
        {
            if (GetStatus(motor, StatusCode.Enabled) == true)
            {
                return;
            }

            if (GetStatus(motor, StatusCode.Alarm)==true)
            {
                ResetAlarm(motor);
            }

            string res = SendCommand(motor, "ME");

            if (MotorAcknowledged(motor, res) != true) 
            {
                throw new Exception("Drive is NOT acknowledged");
            }

            if (GetStatus(motor, StatusCode.Enabled) == false)
            {
                throw new Exception("Motor can not be enabled");
            }
        }

        /// <summary>
        /// Check if motor receives command.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private bool MotorAcknowledged(StepperMotor motor, string response)
        {
            bool idMatch = response.Substring(0, 1) == GetMotorId(motor);
            bool endMatch = response.Substring(1, 1) == "%" | response.Substring(1, 1) == "*";
            return idMatch & endMatch;
        }

        /// <summary>
        /// Disable motor.
        /// </summary>
        /// <param name="motor"></param>
        public void Disable(StepperMotor motor)
        {
            string res = SendCommand(motor, "MD");

            if (MotorAcknowledged(motor, res) != true)
            {
                throw new Exception("Drive is NOT disabled");
            }

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
        public void HomeMotor(StepperMotor motor, double homeOffset, int defaultWorkingSpeed=10)
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

            string res = SendCommand(motor, "SH1H");
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged of home search command SH1H");
            }

            Thread.Sleep(50);
            WaitCondition(motor, StatusCode.Homing, false);
            WaitCondition(motor, StatusCode.Inpos, true);

            ToPointRelative(motor, homeOffset);

            res = SendCommand(motor, "EP0");
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }

            res = SendCommand(motor, "SP0");
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }


            SetSpeed(motor, defaultWorkingSpeed);
        }

        /// <summary>
        /// Move motor to specific angle.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="angle"></param>
        public void ToPoint(StepperMotor motor, double angle)
        {
            int target = Convert.ToInt32(angle * _countPerDegree);
            string res = SendCommand(motor, "FP" + target);
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }

            if (GetStatus(motor, StatusCode.Alarm))
            {
                throw new Exception("Drive alarm");
            }

            Thread.Sleep(50);
        }

        public void ToPointWaitTillEnd(StepperMotor motor, double angle)
        {
            int target = Convert.ToInt32(angle * _countPerDegree);
            string res = SendCommand(motor, "FP" + target);
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }

            Thread.Sleep(50);

            WaitTillEnd(motor, angle);
        }

        public async Task<bool> ToPointAsync(StepperMotor motor1, double angle1, StepperMotor motor2, double angle2,
            int timeout = 10)
        {
            return await Task.Run(() =>
            {
                try
                {
                    int target1 = Convert.ToInt32(angle1 * _countPerDegree);
                    string res1 = SendCommand(motor1, "FP" + target1);
                    if (MotorAcknowledged(motor1, res1) == false)
                    {
                        throw new Exception("Drive is NOT acknowledged");
                    }

                    int target2 = Convert.ToInt32(angle2 * _countPerDegree);
                    string res2 = SendCommand(motor2, "FP" + target2);
                    if (MotorAcknowledged(motor2, res2) == false)
                    {
                        throw new Exception("Drive is NOT acknowledged");
                    }

                    Thread.Sleep(50);
                    WaitTillEnd(motor1, angle1, timeout);
                    WaitTillEnd(motor2, angle2, timeout);

                    double currentPos1 = GetPosition(motor1);
                    if (Math.Abs(currentPos1 - angle1) > 0.5)
                    {
                        throw new Exception("Position error is bigger than 0.5 degree.");
                    }

                    double currentPos2 = GetPosition(motor2);
                    if (Math.Abs(currentPos2 - angle2) > 0.5)
                    {
                        throw new Exception("Position error is bigger than 0.5 degree.");
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Move motor to specific angle relative to current postion.
        ///  allow error: 0.5 degree.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="angle"></param>
        public void ToPointRelative(StepperMotor motor, double angle, int timeout = 10)
        {
            double lastPos = GetPosition(motor);

            int target = Convert.ToInt32(angle * _countPerDegree);
            string res = SendCommand(motor, "FL" + target);
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }

            Thread.Sleep(50);
            WaitTillEnd(motor, lastPos + angle, timeout);        
        }

        /// <summary>
        /// Wait motor get in position.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="targetAngle"></param>
        /// <param name="timeout"></param>
        public void WaitTillEnd(StepperMotor motor, double targetAngle, int timeout = 10)
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
            if (GetStatus(StepperMotor.One, StatusCode.Enabled) == false)
            {
                throw new Exception("Stepper motor One is disabled.");
            }

            if (GetStatus(StepperMotor.Two, StatusCode.Enabled) == false)
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
        private void WaitCondition(StepperMotor motor, StatusCode code, bool condition,  int timeout = 20)
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
        public void SetAcceleration(StepperMotor motor, int acceleration = 30)
        {
            // rev / sec / sec
            //double acc = GearRatio * acceleration / 360.0;
            //// AC - Acceleration Rate 0.167 to 5461.167 (resolution is 0.167 rps/s)
            //int times = Convert.ToInt16(acc / 0.167);
            //times++;
            //acc = Convert.ToDouble(times) * 0.167;

            //string accStr = acc.ToString("0.000");
            string res = SendCommand(motor, "AC" + acceleration);
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }

            res = SendCommand(motor, "DE" + acceleration);
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }
        }

        /// <summary>
        /// Set speed of motor.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="velocity">rps</param>
        public void SetVelocity(StepperMotor motor, int velocity = 30)
        {
            //double vel = GearRatio * velocity / 360.0;

            // ST-Q/Si, ST-S , STM, STAC5: 0.0042 - 80.0000 (resolution is 0.0042)
            //int times = Convert.ToInt16(vel / 0.0042);
            //times++;
            //vel = Convert.ToDouble(times) * 0.0042;
            //string velStr = vel.ToString("0.0000");

            string res = SendCommand(motor, "VE" + velocity);
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }
        }

        public void SetSpeed(StepperMotor motor, int speed = 30)
        {
            SetVelocity(motor, speed);
            SetAcceleration(motor, speed);
        }

        /// <summary>
        /// Reset all alarm of motor.
        /// </summary>
        /// <param name="motor"></param>
        public void ResetAlarm(StepperMotor motor)
        {
            string res = SendCommand(motor, "AR");
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }

            Thread.Sleep(20);

            if (GetStatus(motor, StatusCode.Alarm) == true)
            {
                throw new Exception("Drive's alarm can not be reset");
            }

            res = SendCommand(motor, "ME");
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }

            if (GetStatus(motor, StatusCode.Enabled) == false)
            {
                throw new Exception("Drive is can not be enabled.");
            }
        }

        /// <summary>
        /// Asks the drive to respond with what it’s doing
        /// </summary>
        /// <param name="motor"></param>
        public bool GetStatus(StepperMotor motor, StatusCode status)
        {
            string info = SendCommand(motor, "SC");
            string code = info.Substring(4, info.Length - 4);

            string binaryValue = Convert.ToString(Convert.ToInt32(code, 16), 2);

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
        public bool GetInput(StepperMotor motor, InputStepper input)
        {
            string res = SendCommand(motor, "IS");
            if (res.Length != 12)
            {
                throw new Exception("Input status response length error");
            }

            return res.Substring(12 - (int)input, 1) == "0";
        }

        /// <summary>
        /// Get current position of motor.
        /// </summary>
        /// <param name="motor"></param>
        /// <returns></returns>
        public double GetPosition(StepperMotor motor)
        {
            // Response of "IP" is always hexadecimal
            string res = SendCommand(motor, "IP");
            string posString = res.Substring(4, res.Length - 4);

            int pos = Convert.ToInt32(posString);

            return pos / _countPerDegree;
        }

    }
}
