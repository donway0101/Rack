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
        private SerialPort SerialPort;
        private string PortName;
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
        private double GearRatio = 3.75;

        private double DegreeToCountFactor = 0;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="portName">Rs232 Com port number</param>
        public Stepper(string portName)
        {
            PortName = portName;
            DegreeToCountFactor = ElectronicGearing * GearRatio / 360;
        }

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
        private string GetMotorId(Motor motor)
        {
            return Convert.ToString((int)motor);
        }

        /// <summary>
        /// Send command to motor through serial port, and wait of response.
        /// </summary>
        /// <param name="cmd"></param>
        /// <seealso cref="SendCommand"/>
        public string SendCommand(Motor motor,string cmd)
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
        public void Enable(Motor motor)
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
        private bool MotorAcknowledged(Motor motor, string response)
        {
            if (response.Substring(0, 1) == GetMotorId(motor) & response.Substring(1, 1) == "%")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Disable motor.
        /// </summary>
        /// <param name="motor"></param>
        public void Disable(Motor motor)
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
        public void HomeMotor(Motor motor)
        {
            if (GetStatus(motor).Contains("R") == false)
            {
                throw new Exception("Motor is not ready");
            }

            if (GetInput(motor, Input.X3) == true)
            {

            }

            string res = SendCommand(motor, "SH3L");
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
        public void ToPoint(Motor motor, double angle)
        {

        }

        /// <summary>
        /// Move motor to specific angle relative to current postion.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="angle"></param>
        public void ToPointRelative(Motor motor, double angle)
        {
            int target = Convert.ToInt32(angle * DegreeToCountFactor);
            string res = SendCommand(motor, "FL" + target);
            if (MotorAcknowledged(motor, res) == false)
            {
                throw new Exception("Drive is NOT acknowledged");
            }
        }

        /// <summary>
        /// Set acceleration and deceleration of motor.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="angle"></param>
        public void SetAcceleration(Motor motor, double angle)
        {

        }

        /// <summary>
        /// Set speed of motor.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="angle"></param>
        public void SetVelocity(Motor motor, double angle)
        {

        }

        /// <summary>
        /// Reset all alarm of motor.
        /// </summary>
        /// <param name="motor"></param>
        public void ResetAlarm(Motor motor)
        {

        }

        /// <summary>
        /// Asks the drive to respond with what it’s doing
        /// </summary>
        /// <param name="motor"></param>
        public string GetStatus(Motor motor)
        {
            return SendCommand(motor, "RS");
        }

        public bool GetInput(Motor motor, Input input)
        {
            string res = SendCommand(motor, "RS");
            if (res.Length != 12)
            {
                throw new Exception("Input status response length error");
            }

            if (res.Substring(12-(int)input, 1) == "0")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get current position of motor.
        /// </summary>
        /// <param name="motor"></param>
        /// <returns></returns>
        public double GetPosition(Motor motor)
        {
            return 0;
        }


    }
}
