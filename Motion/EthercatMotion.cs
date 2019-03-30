using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

        public TargetPosition HomePosition { get; set; } = new TargetPosition() { Id = Location.Home };
        public TargetPosition PickPosition { get; set; } = new TargetPosition() { Id = Location.Pick };
        public TargetPosition BinPosition { get; set; } = new TargetPosition() { Id = Location.Bin };
        public TargetPosition ConveyorLeftPosition { get; set; } = new TargetPosition() { Id = Location.NoWhere };
        public TargetPosition ConveyorRightPosition { get; set; } = new TargetPosition() { Id = Location.NoWhere };
        public TargetPosition Holder1 { get; set; } = new TargetPosition() { Id = Location.Holder1 };
        public TargetPosition Holder2 { get; set; } = new TargetPosition() { Id = Location.Holder2 };
        public TargetPosition Holder3 { get; set; } = new TargetPosition() { Id = Location.Holder3 };
        public TargetPosition Holder4 { get; set; } = new TargetPosition() { Id = Location.Holder4 };
        public TargetPosition Holder5 { get; set; } = new TargetPosition() { Id = Location.Holder5 };
        public TargetPosition Holder6 { get; set; } = new TargetPosition() { Id = Location.Holder6 };

        public TargetPosition Gold1 { get; set; } = new TargetPosition() { Id = Location.Gold1 };
        public TargetPosition Gold2 { get; set; } = new TargetPosition() { Id = Location.Gold2 };
        public TargetPosition Gold3 { get; set; } = new TargetPosition() { Id = Location.Gold3 };
        public TargetPosition Gold4 { get; set; } = new TargetPosition() { Id = Location.Gold4 };
        public TargetPosition Gold5 { get; set; } = new TargetPosition() { Id = Location.Gold5 };

        public TargetPosition[] Locations { get; set; }

        public EthercatMotion(Api Controller, int axisNum)
        {
            Ch = Controller;
            AxisNum = axisNum;          
        }

        public void Setup()
        {
            #region Motor parameters setup

            MotorZ = new Motor(Axis.ACSC_AXIS_0)
            {
                EcatPosActValAddr = 145,
                EncCtsPerR = 131072,
                BallScrewLead = 32,
                HomeOffset = 365,
                CriticalErrAcc = 100,
                CriticalErrVel = 100,
                CriticalErrIdle = 5,
                SoftLimitNagtive = -10,
                SoftLimitPositive = 730,
                SpeedFactor = 0.9,
                JerkFactor = 16
            };
            //For Sanyo, axisAddrOffset = 18

            MotorX1 = new Motor(Axis.ACSC_AXIS_1)
            {
                EcatPosActValAddr = MotorZ.EcatPosActValAddr + 18,
                EncCtsPerR = 131072,
                BallScrewLead = 16,
                HomeOffset = 793.5,
                CriticalErrAcc = 100,
                CriticalErrVel = 100,
                CriticalErrIdle = 5,
                SoftLimitNagtive = -5,
                SoftLimitPositive = 590,
                SpeedFactor = 0.9,
                JerkFactor = 16
            };
            //For Sanyo, axisAddrOffset = 18

            MotorX1.MaxTravel = MotorX1.SoftLimitPositive - 10;

            MotorX2 = new Motor(Axis.ACSC_AXIS_2)
            {
                EcatPosActValAddr = MotorX1.EcatPosActValAddr + 18,
                EncCtsPerR = 131072,
                BallScrewLead = 16,
                HomeOffset = 12.7,
                CriticalErrAcc = 100,
                CriticalErrVel = 100,
                CriticalErrIdle = 5,
                SoftLimitNagtive = -5,
                SoftLimitPositive = 760,
                SpeedFactor = 1.0,
                JerkFactor = 20
            };
            //For Sanyo, axisAddrOffset = 18

            MotorX2.MaxTravel = MotorX2.SoftLimitPositive - 10;

            MotorY = new Motor(Axis.ACSC_AXIS_3)
            {
                EcatPosActValAddr = MotorX2.EcatPosActValAddr + 18,
                EncCtsPerR = 131072,
                BallScrewLead = 16,
                HomeOffset = 12.4,
                CriticalErrAcc = 100,
                CriticalErrVel = 100,
                CriticalErrIdle = 5,
                SoftLimitNagtive = -330,
                SoftLimitPositive = 4,
                SpeedFactor = 0.8,
                JerkFactor = 20,
                Direction = -1
            };
            //For Sanyo, axisAddrOffset = 18


            //Todo , is two gripper is 60,    MotorR.EncCtsPerR  error?
            MotorR = new Motor(Axis.ACSC_AXIS_4)
            {
                EcatPosActValAddr = 113,
                EncCtsPerR = 10000,
                BallScrewLead = 360.0 * 1.0 / 100.0,
                HomeOffset = -1.9,
                CriticalErrAcc = 100,
                CriticalErrVel = 100,
                CriticalErrIdle = 5,
                SoftLimitNagtive = -40,
                SoftLimitPositive = 40,
                SpeedFactor = 0.18,
                JerkFactor = 3.6
            };
            //Warning: double calculation, need to add .0 to each number.

            #endregion

            Motors = new Motor[5] { MotorZ, MotorX1, MotorX2, MotorY, MotorR };
            DeclareVariableInDBuffer();

            DisableAll();
            foreach (var mtr in Motors)
            {
                //Todo need disable motors?
                SetFPosition(mtr);
                SetSafety(mtr);
            }

            LoadPosition(HomePosition, TeachPos.Home);
            LoadPosition(PickPosition, TeachPos.Pick);
            LoadPosition(BinPosition, TeachPos.Bin);
            LoadPosition(ConveyorLeftPosition, TeachPos.ConveyorLeft);
            LoadPosition(ConveyorRightPosition, TeachPos.ConveyorRight);

            LoadPosition(Holder1, TeachPos.Holder1);
            LoadPosition(Holder2, TeachPos.Holder2);
            LoadPosition(Holder3, TeachPos.Holder3);
            LoadPosition(Holder4, TeachPos.Holder4);
            LoadPosition(Holder5, TeachPos.Holder5);
            LoadPosition(Holder6, TeachPos.Holder6);           

            LoadPosition(Gold1, TeachPos.Gold1);
            LoadPosition(Gold2, TeachPos.Gold2);
            LoadPosition(Gold3, TeachPos.Gold3);
            LoadPosition(Gold4, TeachPos.Gold4);
            LoadPosition(Gold5, TeachPos.Gold5);

            Locations = new TargetPosition[14] { Holder1, Holder2, Holder3, Holder4, Holder5, Holder6,
            HomePosition,PickPosition, BinPosition, Gold1, Gold2, Gold3, Gold4, Gold5};
        }

        private void LoadPosition(TargetPosition target, TeachPos pos)
        {
            target.XPos = Convert.ToDouble(
                                XmlReaderWriter.GetTeachAttribute(Files.RackData, pos, PosItem.XPos));
            target.YPos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, pos, PosItem.YPos));
            target.ZPos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, pos, PosItem.ZPos));
            target.RPos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, pos, PosItem.RPos));
            target.APos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, pos, PosItem.APos));
            target.ApproachHeight = Convert.ToDouble(
                   XmlReaderWriter.GetTeachAttribute(Files.RackData, pos, PosItem.ApproachHeight));
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
            Ch.WriteVariable(motor.SoftLimitNagtive, "SLLIMIT", ProgramBuffer.ACSC_NONE,
                (int)motor.Id, (int)motor.Id, -1, -1);
            Ch.WriteVariable(motor.SoftLimitPositive, "SRLIMIT", ProgramBuffer.ACSC_NONE,
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
                Ch.SetAcceleration(mtr.Id, velocity * 10);
                Ch.SetDeceleration(mtr.Id, velocity * 10);
                Ch.SetKillDeceleration(mtr.Id, velocity * 100);
                Ch.SetJerk(mtr.Id, velocity * mtr.JerkFactor);
            }
        }

        public void SetSpeedImm(double velocity)
        {
            foreach (var mtr in Motors)
            {
                Ch.SetVelocityImm(mtr.Id, velocity * mtr.SpeedFactor);
                Ch.SetAccelerationImm(mtr.Id, velocity * 10);
                Ch.SetDecelerationImm(mtr.Id, velocity * 10);
                Ch.SetKillDecelerationImm(mtr.Id, velocity * 100);
                Ch.SetJerkImm(mtr.Id, velocity * mtr.JerkFactor);
            }
        }

        public void SetAcceleration(Motor motor, double acceleration) { }
        public void SetDeceleration(Motor motor, double deceleration) { }
        public void SetJerk(Motor motor, double jerk) { }
        public double GetPosition(Motor motor)
        {
            return Ch.GetFPosition(motor.Id)*motor.Direction;
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

        public void EnableAll(int timeout = 2000)
        {
            foreach (var mtr in Motors)
            {
                Ch.Enable(mtr.Id);
                //Wait till enabled
                Ch.WaitMotorEnabled(mtr.Id, 1, timeout);
            }
        }

        public void Disable(Motor motor, int timeout = 2000)
        {
            Ch.Disable(motor.Id);
            //Wait till enabled
            Ch.WaitMotorEnabled(motor.Id, 0, timeout);
        }

        public void DisableAll(int timeout = 2000)
        {
            foreach (var mtr in Motors)
            {
                Ch.Disable(mtr.Id);
                //Wait till enabled
                Ch.WaitMotorEnabled(mtr.Id, 0, timeout);
            }
        }
        public void Jog(Motor motor, double velocity) { }
        public void Kill(Motor motor) { }

        public void ToPoint(Motor motor, double point)
        {
            point *= motor.Direction;
            Ch.ToPoint(MotionFlags.ACSC_AMF_MAXIMUM, motor.Id, point);
        }

        public void BreakToPoint(Motor motor, double point)
        {
            Break(motor);
            point *= motor.Direction;
            Ch.ToPoint(MotionFlags.ACSC_AMF_MAXIMUM, motor.Id, point);
        }

        public void BreakToPointWaitTillEnd(Motor motor, double point, int timeout = 60000)
        {
            Break(motor);
            point *= motor.Direction;
            Ch.ToPoint(MotionFlags.ACSC_AMF_MAXIMUM, motor.Id, point);
            Ch.WaitMotionEnd(motor.Id, timeout);
        }

        public void Break(Motor motor)
        {
            Ch.Break(motor.Id);
        }

        /// <summary>
        /// Catch exception and Send kill command to motor if needed.
        /// </summary>
        /// <param name="motor"></param>
        /// <param name="point"></param>
        /// <param name="timeout"></param>
        public void ToPointWaitTillEnd(Motor motor, double point, int timeout=60000)
        {
            point *= motor.Direction;
            Ch.ToPoint(MotionFlags.ACSC_AMF_MAXIMUM, motor.Id, point);
            Ch.WaitMotionEnd(motor.Id, timeout);
        }

        public void BreakToPointX(double point)
        {
            Break(MotorX1);
            Break(MotorX2);
            ToPointX(point);
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
                    x1Target = 0; //Move x1Pos
                    x2Target = x2Pos + x1Pos + distance;
                }
                else
                {
                    if (x2Pos < Math.Abs(halfDistance))
                    {
                        x2Target = 0;
                        x1Target = x2Pos + x1Pos + distance;
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

        public void BreakToPointXWaitTillEnd(double point, int timeout = 60000)
        {
            Break(MotorX1);
            Break(MotorX2);
            ToPointXWaitTillEnd(point);
        }

        public void ToPointXWaitTillEnd(double point, int timeout = 60000)
        {
            ToPointX(point);
            Ch.WaitMotionEnd(MotorX1.Id, timeout);
            Ch.WaitMotionEnd(MotorX2.Id, timeout);
        }

        public void WaitTillEnd(Motor motor, int timeout = 60000)
        {
            Ch.WaitMotionEnd(motor.Id, timeout);
        }

        public void WaitTillXBiggerThan(double point, int timeout = 60000)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (GetPositionX() < point)
            {
                if (stopwatch.ElapsedMilliseconds>timeout)
                {
                    throw new Exception("WaitTillXBigger timeout");
                }
                Thread.Sleep(50);
            }
        }

        public void WaitTillZBiggerThan(double point, int timeout = 60000)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (GetPosition(MotorZ) < point)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    throw new Exception("WaitTillXBigger timeout");
                }
                Thread.Sleep(50);
            }
        }

        public void WaitTillXSmallerThan(double point, int timeout = 60000)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (GetPositionX() > point)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    throw new Exception("WaitTillXBigger timeout");
                }
                Thread.Sleep(50);
            }
        }

        public void WaitTillEndX(int timeout = 60000)
        {
            Ch.WaitMotionEnd(MotorX1.Id, timeout);
            Ch.WaitMotionEnd(MotorX2.Id, timeout);
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
