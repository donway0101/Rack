using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACS.SPiiPlusNET;
using Tools;

namespace Motion
{
    public class EthercatMotion
    {
        /// <summary>
        /// The ACS controller.
        /// </summary>
        private Api Ch;

        private int AxisNum;

        private string NewLine = Environment.NewLine;
        private const string EcatActPosName = "EthercatAbsolutePosition";
        private const string GlobalDefine = "GLOBAL INT ";

        public Motor MotorX1 { get; set; }// = new Motor(Axis.ACSC_AXIS_1);
        public Motor MotorX2 { get; set; }// = new Motor(Axis.ACSC_AXIS_2);
        public Motor MotorZ { get; set; }// = new Motor(Axis.ACSC_AXIS_0);
        public Motor MotorY { get; set; }
        public Motor MotorR { get; set; }

        public Motor[] Motors { get; set; }

        public TargetPosition HomePosition { get; set; } = new TargetPosition();

        public EthercatMotion(Api Controller, int axisNum)
        {
            Ch = Controller;
            AxisNum = axisNum;          
        }

        public void Setup()
        {
            MotorZ = new Motor(Axis.ACSC_AXIS_0);
            MotorZ.EcatPosActValAddr = 144; //For Sanyo, axisAddrOffset = 18
            MotorZ.EncCtsPerR = 131072;
            MotorZ.BallScrewLead = 32;
            MotorZ.HomeOffset = 800.0;
            MotorZ.CriticalErrAcc = 100;
            MotorZ.CriticalErrVel = 100;
            MotorZ.CriticalErrIdle = 5;

            MotorX1 = new Motor(Axis.ACSC_AXIS_1);
            MotorX1.EcatPosActValAddr = 162; //For Sanyo, axisAddrOffset = 18
            MotorX1.EncCtsPerR = 131072;
            MotorX1.BallScrewLead = 16;          
            MotorX1.HomeOffset = 5.0;
            MotorX1.CriticalErrAcc = 100;
            MotorX1.CriticalErrVel = 100;
            MotorX1.CriticalErrIdle = 5;

            MotorX2 = new Motor(Axis.ACSC_AXIS_2);
            MotorX2.EcatPosActValAddr = 180; //For Sanyo, axisAddrOffset = 18
            MotorX2.EncCtsPerR = 131072;
            MotorX2.BallScrewLead = 16;
            MotorX2.HomeOffset = 5.0;
            MotorX2.CriticalErrAcc = 100;
            MotorX2.CriticalErrVel = 100;
            MotorX2.CriticalErrIdle = 5;

            MotorY.FPositionDirection = -1;

            //Motors = new Motor[5] { MotorZ, MotorX1, MotorX2, MotorY, MotorR };
            Motors = new Motor[3] { MotorZ, MotorX1, MotorX2 };
            DeclareVariableInDBuffer();

            foreach (var mtr in Motors)
            {
                //Todo need disable motors?
                SetFPosition(mtr);
                SetSafety(mtr);
            }

            LoadPosition();
        }

        private void LoadPosition()
        {
            HomePosition.XPos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, TeachData.Home, TeachData.XPos));
            HomePosition.YPos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, TeachData.Home, TeachData.YPos));
            HomePosition.ZPos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, TeachData.Home, TeachData.ZPos));
            HomePosition.RPos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, TeachData.Home, TeachData.RPos));
            HomePosition.APos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, TeachData.Home, TeachData.APos));
        }

        /// <summary>
        /// Set poweron feedback position.
        /// </summary>
        /// <param name="motor"></param>
        private void SetFPosition(Motor motor)
        {
            motor.EncoderFactor = motor.BallScrewLead / motor.EncCtsPerR;
            WriteVariable(motor, "EFAC", motor.EncoderFactor);
            motor.PowerOnPos = Convert.ToDouble( Ch.ReadVariable(GetEcatActPosName(motor)));
            Ch.SetFPosition(motor.Id, motor.PowerOnPos * motor.EncoderFactor + motor.HomeOffset);
        }

        private void SetSafety(Motor motor)
        {
            Ch.WriteVariable(motor.CriticalErrAcc, "CERRA", ProgramBuffer.ACSC_NONE,
                (int)motor.Id, (int)motor.Id, -1, -1);
            Ch.WriteVariable(motor.CriticalErrVel, "CERRV", ProgramBuffer.ACSC_NONE,
                (int)motor.Id, (int)motor.Id, -1, -1);
            Ch.WriteVariable(motor.CriticalErrIdle, "CERRI", ProgramBuffer.ACSC_NONE,
                (int)motor.Id, (int)motor.Id, -1, -1);
        }

        private void WriteVariable(Motor motor, string acsVariable, object value)
        {
            Ch.WriteVariable(value, acsVariable, ProgramBuffer.ACSC_NONE,
               (int)motor.Id, (int)motor.Id, -1, -1);
        }

        private void MapEtherCAT()
        {
            foreach (var motor in Motors)
            {
                Ch.MapEtherCATInput(MotionFlags.ACSC_NONE, motor.EcatPosActValAddr, GetEcatActPosName(motor));
            }
        }

        private string GetEcatActPosName(Motor motor)
        {
            return EcatActPosName + motor.EcatPosActValAddr;
        }

        private void DeclareVariableInDBuffer()
        {
            string Warning = "! Warning, generated by Host for Motor Abs Position, do not change." + NewLine;

            string EcatActPosDefine = string.Empty;
            foreach (var motor in Motors)
            {
                EcatActPosDefine += GlobalDefine + GetEcatActPosName(motor) + NewLine;
            }

            string program = Warning + EcatActPosDefine;
            ProgramBuffer DBuffer = (ProgramBuffer)(Convert.ToInt16(Ch.GetDBufferIndex()));

            string originProgram = Ch.UploadBuffer(DBuffer);
            if (originProgram != null)
            {
                if (originProgram.Contains(EcatActPosName) == false)
                {
                    Ch.AppendBuffer(DBuffer, program);

                    for (int i = 0; i < 9; i++)
                    {
                        Ch.ClearBuffer((ProgramBuffer)i, 1, 2000);
                    }
                    Ch.CompileBuffer(DBuffer);
                }
            }
            else
            {
                Ch.AppendBuffer(DBuffer, program);

                for (int i = 0; i < 9; i++)
                {
                    Ch.ClearBuffer((ProgramBuffer)i, 1, 2000);
                }
                Ch.CompileBuffer(DBuffer);
            }

            MapEtherCAT();
        }

        public void SetPosition(Motor motor, double position) { }

        public void SetVelocity(Motor motor, double velocity)
        {
            Ch.SetVelocity(motor.Id, velocity);
        }

        public void SetSpeed(double velocity)
        {
            foreach (var mtr in Motors)
            {
                Ch.SetVelocity(mtr.Id, velocity*mtr.SpeedFactor);
                Ch.SetJerk(mtr.Id, velocity * mtr.JerkFactor);
            }
        }

        public void SetSpeedImm(double velocity)
        {
            foreach (var mtr in Motors)
            {
                Ch.SetVelocityImm(mtr.Id, velocity * mtr.SpeedFactor);
                Ch.SetJerkImm(mtr.Id, velocity * mtr.JerkFactor);
            }
        }

        public void SetAcceleration(Motor motor, double acceleration) { }
        public void SetDeceleration(Motor motor, double deceleration) { }
        public void SetJerk(Motor motor, double jerk) { }
        public double GetPosition(Motor motor)
        {
            return Ch.GetFPosition(motor.Id)*motor.FPositionDirection;
        }

        public double GetPositionX()
        {
            return Ch.GetFPosition(MotorX1.Id) + Ch.GetFPosition(MotorX2.Id);
        }
        public double GetVelocity(Motor motor) { return 0; }
        public double GetAcceleration(Motor motor) { return 0; }
        public double GetDeceleration(Motor motor) { return 0; }
        public double GetJerk(Motor motor) { return 0; }

        public void Enable(Motor motor, int timeout=2000)
        {
            Ch.Enable(motor.Id);
            //Wait till enabled
            Ch.WaitMotorEnabled(motor.Id, 1, timeout);
        }
        public void Disable(Motor motor, int timeout = 2000)
        {
            Ch.Disable(motor.Id);
            //Wait till enabled
            Ch.WaitMotorEnabled(motor.Id, 0, timeout);
        }
        public void Jog(Motor motor, double velocity) { }
        public void Kill(Motor motor) { }

        public void ToPoint(Motor motor, double point)
        {
            Ch.ToPoint(MotionFlags.ACSC_AMF_MAXIMUM, motor.Id, point);
        }

        public void ToPointX(double point)
        {
            double x1Pos = GetPosition(MotorX1);
            double x2Pos = GetPosition(MotorX2);
            double currentPos = x1Pos + x2Pos;
            double distance = point - currentPos;
            double halfDistance = distance / 2;
            double x1Target, x2Target;

            if (distance>0)
            {
                if (MotorX1.MaxTravel - x1Pos <= halfDistance)
                {
                    x1Target = MotorX1.MaxTravel;
                    x2Target = distance - (MotorX1.MaxTravel - x1Pos) + x2Pos;
                }
                else
                {
                    if (MotorX2.MaxTravel-x2Pos<=halfDistance)
                    {
                        x2Target = MotorX2.MaxTravel;
                        x1Target = distance - (MotorX2.MaxTravel - x2Pos) + x1Pos;
                    }
                    else
                    {
                        x1Target = halfDistance + x1Pos;
                        x2Target = halfDistance + x2Pos;
                    }
                }
            }
            else //Move to left.
            {
                if (x1Pos<Math.Abs(halfDistance))
                {
                    x1Target = 0;
                    x2Target = distance;
                }
                else
                {
                    if (x2Pos < Math.Abs(halfDistance))
                    {
                        x2Target = 0;
                        x1Target = distance;
                    }
                    else
                    {
                        x1Target = halfDistance + x1Pos;
                        x2Target = halfDistance + x2Pos;
                    }
                }
            }

            Ch.ToPoint(MotionFlags.ACSC_AMF_MAXIMUM, MotorX1.Id, x1Target);
            Ch.ToPoint(MotionFlags.ACSC_AMF_MAXIMUM, MotorX2.Id, x2Target);
        }

        public void ToPointM(Motor[] motors, double[] points)
        {
            Axis[] axes = new Axis[motors.Length+1];
            for (int i = 0; i < motors.Length; i++)
            {
                axes[i] = motors[i].Id;
            }
            axes[motors.Length] = Axis.ACSC_NONE;

            Ch.ToPointM(MotionFlags.ACSC_AMF_MAXIMUM, axes, points);
        }

        public void Test()
        {
            //Enable(MotorX1);

            //ToPoint(MotorX1, 260);

            //Console.WriteLine("doing");

            //Ch.WaitMotionEnd(Axis.ACSC_AXIS_1, 60000);

            //Console.WriteLine("Finish");

            Enable(MotorX1);
            Enable(MotorX2);

            Motor[] motors = new Motor[2] { MotorX1, MotorX2 };
            double[] pos = new double[2] { 260, 260 };

            ToPointM(motors, pos);

            Console.WriteLine("doing");

            Ch.WaitMotionEnd(Axis.ACSC_AXIS_1, 60000);
            Console.WriteLine("finis1");
            Ch.WaitMotionEnd(Axis.ACSC_AXIS_2, 60000);

            Console.WriteLine("Finish2");
        }

        public void Estop()
        {

        }

      
    }
}
