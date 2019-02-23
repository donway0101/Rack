using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GripperStepper
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
        private double ElectronicGearing = 3600;

        /// <summary>
        /// Load is driven by gear and belt.
        /// </summary>
        private double GearRatio = 1.0;//3.75;

        /// <summary>
        /// Motor counts per degree, including transimission factor.
        /// </summary>
        private double CountPerDegree = 0;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="portName">Rs232 Com port number</param>
        public Stepper(string portName)
        {
            PortName = portName;

            CountPerDegree = ElectronicGearing * GearRatio / 360;
        }

        /// <summary>
        /// Open serial port.
        /// </summary>
        public void Initialization()
        {           
            try
            {
                SerialPort = new SerialPort(PortName, 9600, Parity.None, 8, StopBits.One);
                SerialPort.Open();
                SerialPort.DataReceived += SerialPort_DataReceived;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Close serial port.
        /// </summary>
        public void Close()
        {
            try
            {
                SerialPort.Close();
                SerialPort.Dispose();
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
        public void SetFeedbackFormatDecimal(GripperMotor motor)
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
                if (stopwatch.ElapsedMilliseconds > 2*1000) //Should response within 2 second.
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
        private string GetMotorId(GripperMotor motor)
        {
            return Convert.ToString((int)motor);
        }

        /// <summary>
        /// Send command to motor through serial port, and wait for response.
        /// </summary>
        /// <param name="cmd"></param>
        /// <seealso cref="SendCommand"/>
        public string SendCommand(GripperMotor motor,string cmd)
        {
            lock (PortWriteLocker)
            {
                try
                {
                    string command = GetMotorId(motor) + cmd + "\r"; //Add a carriage return.
                    byte[] buffer = Encoding.ASCII.GetBytes(command);
                    SerialPort.Write(buffer, 0, buffer.Length);
                    DriverResponsed = false;
                    return GetResponse();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Enalbe motor.
        /// </summary>
        /// <param name="motor"></param>
        public void Enable(GripperMotor motor)
        {
            string res = SendCommand(motor, "ME");

            if (MotorAcknowledged(motor, res) != true) 
            {
                throw new Exception("Drive is NOT enabled");
            }
        }

        /// <summary>
        /// Check if motor receives command.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private bool MotorAcknowledged(GripperMotor motor, string response)
        {
            return response.Substring(0, 1) == GetMotorId(motor) & response.Substring(1, 1) == "%";
        }

        /// <summary>
        /// Disable motor.
        /// </summary>
        /// <param name="motor"></param>
        public void Disable(GripperMotor motor)
        {
            string res = SendCommand(motor, "MD");

            if (MotorAcknowledged(motor, res) != true)
            {
                throw new Exception("Drive is NOT disabled");
            }
        }

        /// <summary>
        /// Find reference point of motor, AKA home position.
        /// Must has the right sensor for homing.
        /// </summary>
        /// <param name="motor"></param>
        public void HomeMotor(GripperMotor motor, double homeOffset)
        {
            if (GetStatus(motor, StatusCode.Enabled) == false)
            {
                throw new Exception("Motor is not ready");
            }

            if (GetInput(motor, Input.X3) == true)
            {
                //If home sensor has been triggered before homing.
                // move out first.
                ToPointRelative(motor, 10);           
            }

            string res = SendCommand(motor, "SH3L");
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
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
        }

        /// <summary>
        /// Move motor to specific angle.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="angle"></param>
        public void ToPoint(GripperMotor motor, double angle, int timeout = 10)
        {
            double lastPos = GetPosition(motor);

            int target = Convert.ToInt32(angle * CountPerDegree);
            string res = SendCommand(motor, "FP" + target);
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }

            Thread.Sleep(50);
            WaitMotionEnd(motor, timeout);

            double currentPos = GetPosition(motor);
            if (Math.Abs(currentPos - (lastPos + angle)) > 0.5)
            {
                throw new Exception("Position error is bigger than 0.5 degree.");
            }
        }

        /// <summary>
        /// Move motor to specific angle relative to current postion.
        ///  allow error: 0.5 degree.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="angle"></param>
        public void ToPointRelative(GripperMotor motor, double angle, int timeout = 10)
        {
            double lastPos = GetPosition(motor);

            int target = Convert.ToInt32(angle * CountPerDegree);
            string res = SendCommand(motor, "FL" + target);
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }

            Thread.Sleep(50);
            WaitMotionEnd(motor, timeout);

            double currentPos = GetPosition(motor);
            if (Math.Abs( currentPos - (lastPos+angle) ) > 0.5)
            {
                throw new Exception("Position error is bigger than 0.5 degree.");
            }           
        }

        /// <summary>
        /// Wait motor get in position.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="timeout"></param>
        private void WaitMotionEnd(GripperMotor motor, int timeout = 10)
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
        }

        /// <summary>
        /// Wait motor status to be true.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="code"></param>
        /// <param name="condition"></param>
        /// <param name="timeout"></param>
        private void WaitCondition(GripperMotor motor, StatusCode code, bool condition,  int timeout = 20)
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
        public void SetAcceleration(GripperMotor motor, double acceleration)
        {
            // rev / sec / sec
            double acc = acceleration / 360 / GearRatio;
            string accStr = acc.ToString("0.000");
            string res = SendCommand(motor, "AC" + accStr);
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }

            res = SendCommand(motor, "DE" + accStr);
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }
        }

        /// <summary>
        /// Set speed of motor.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="velocity">degree / sec</param>
        public void SetVelocity(GripperMotor motor, double velocity)
        {
            double vel = velocity / 360 / GearRatio;
            string velStr = vel.ToString("0.0000");
            string res = SendCommand(motor, "VE" + velStr);
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }
        }

        /// <summary>
        /// Reset all alarm of motor.
        /// </summary>
        /// <param name="motor"></param>
        public void ResetAlarm(GripperMotor motor)
        {
            string res = SendCommand(motor, "AR");
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }

            Thread.Sleep(20);

            if (GetStatus(motor, StatusCode.Alarm) == true)
            {
                throw new Exception("Drive is still alarmed");
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
        public bool GetStatus(GripperMotor motor, StatusCode status)
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
        public bool GetInput(GripperMotor motor, Input input)
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
        public double GetPosition(GripperMotor motor)
        {
            // Response of "IP" is always hexadecimal
            string res = SendCommand(motor, "IP");
            string posString = res.Substring(4, res.Length - 4);

            int pos = Convert.ToInt32(posString);

            return pos / CountPerDegree;
        }


    }
}
