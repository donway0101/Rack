using System;
using System.Diagnostics;
using System.Threading;
using ACS.SPiiPlusNET;

namespace Rack
{
    public class EthercatMotion
    {

        #region Private members
        private readonly Api _ch;
        private readonly int _axisNum;
        private readonly string _newLine = Environment.NewLine;
        private const string EcatActPosName = "EthercatAbsolutePosition";
        private const string GlobalDefine = "GLOBAL INT "; 
        #endregion

        #region Motors
        public Motor MotorX1 { get; set; }// = new Motor(Axis.ACSC_AXIS_1);
        public Motor MotorX2 { get; set; }// = new Motor(Axis.ACSC_AXIS_2);
        public Motor MotorZ { get; set; }// = new Motor(Axis.ACSC_AXIS_0);
        public Motor MotorY { get; set; }
        public Motor MotorR { get; set; }

        public Motor[] Motors { get; set; } 
        #endregion

        #region Targets
        public TargetPosition HomePosition { get; set; }
        public TargetPosition PickPosition { get; set; }
        public TargetPosition BinPosition { get; set; }
        public TargetPosition ConveyorLeftPosition { get; set; }
        public TargetPosition ConveyorRightPosition { get; set; }
        public TargetPosition ShieldBox1 { get; set; }
        public TargetPosition ShieldBox2 { get; set; }
        public TargetPosition ShieldBox3 { get; set; }
        public TargetPosition ShieldBox4 { get; set; }
        public TargetPosition ShieldBox5 { get; set; }
        public TargetPosition ShieldBox6 { get; set; }

        public TargetPosition Gold1 { get; set; }
        public TargetPosition Gold2 { get; set; }
        public TargetPosition Gold3 { get; set; }
        public TargetPosition Gold4 { get; set; }
        public TargetPosition Gold5 { get; set; }

        public TargetPosition G1ToG2Offset { get; set; }

        public TargetPosition PickOffset { get; set; }

        public TargetPosition[] Locations { get; set; } 
        #endregion

        public bool MotorSetupComplete { get; set; }

        public EthercatMotion(Api Controller, int axisNum)
        {
            _ch = Controller;
            _axisNum = axisNum;          
        }

        public void Setup()
        {
            MotorSetupComplete = false;

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
            EnableAll();

            MotorSetupComplete = true;
            //Todo script inside acs to stop all motor is any error occur.          
        }

        public void LoadPositions()
        {
            HomePosition = LoadPosition(TeachPos.Home, TeachPos.Home);
            PickPosition = LoadPosition(TeachPos.Pick, TeachPos.Pick);
            BinPosition = LoadPosition(TeachPos.Bin, TeachPos.Bin);
            ConveyorLeftPosition = LoadPosition(TeachPos.ConveyorLeft, TeachPos.NoWhere);
            ConveyorRightPosition = LoadPosition(TeachPos.ConveyorRight, TeachPos.NoWhere);

            ShieldBox1 = LoadPosition(TeachPos.Holder1, TeachPos.Holder1);
            ShieldBox2 = LoadPosition(TeachPos.Holder2, TeachPos.Holder2);
            ShieldBox3 = LoadPosition(TeachPos.Holder3, TeachPos.Holder3);
            ShieldBox4 = LoadPosition(TeachPos.Holder4, TeachPos.Holder4);
            ShieldBox5 = LoadPosition(TeachPos.Holder5, TeachPos.Holder5);
            ShieldBox6 = LoadPosition(TeachPos.Holder6, TeachPos.Holder6);

            Gold1 = LoadPosition(TeachPos.Gold1, TeachPos.Gold1);
            Gold2 = LoadPosition(TeachPos.Gold2, TeachPos.Gold2);
            Gold3 = LoadPosition(TeachPos.Gold3, TeachPos.Gold3);
            Gold4 = LoadPosition(TeachPos.Gold4, TeachPos.Gold4);
            Gold5 = LoadPosition(TeachPos.Gold5, TeachPos.Gold5);

            G1ToG2Offset = LoadPosition(TeachPos.G1ToG2Offset, TeachPos.NoWhere);
            PickOffset = LoadPosition(TeachPos.PickOffset, TeachPos.NoWhere);

            Locations = new TargetPosition[14]
            {
                ShieldBox1, ShieldBox2, ShieldBox3, ShieldBox4, ShieldBox5, ShieldBox6,
                HomePosition, PickPosition, BinPosition, Gold1, Gold2, Gold3, Gold4, Gold5
            };
        }

        private TargetPosition LoadPosition(TeachPos pos, TeachPos id)
        {
            TargetPosition target = new TargetPosition
            {
                Id = id,
                XPos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, pos, PosItem.XPos)),
                YPos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, pos, PosItem.YPos)),
                ZPos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, pos, PosItem.ZPos)),
                RPos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, pos, PosItem.RPos)),
                APos = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, pos, PosItem.APos)),
                ApproachHeight = Convert.ToDouble(
                    XmlReaderWriter.GetTeachAttribute(Files.RackData, pos, PosItem.ApproachHeight))
            };
            return target;
        }

        /// <summary>
        /// Set poweron feedback position.
        /// </summary>
        /// <param name="motor"></param>
        private void SetFPosition(Motor motor)
        {            
            motor.EncoderFactor = motor.BallScrewLead / motor.EncCtsPerR;
            WriteVariable(motor, "EFAC", motor.EncoderFactor);
            motor.PowerOnPos = Convert.ToDouble( _ch.ReadVariable(GetEcatActPosName(motor)));
            _ch.SetFPosition(motor.Id, motor.PowerOnPos * motor.EncoderFactor + motor.HomeOffset);
        }

        private void SetSafety(Motor motor)
        {
            _ch.WriteVariable(motor.CriticalErrAcc, "CERRA", ProgramBuffer.ACSC_NONE,
                (int)motor.Id, (int)motor.Id, -1, -1);
            _ch.WriteVariable(motor.CriticalErrVel, "CERRV", ProgramBuffer.ACSC_NONE,
                (int)motor.Id, (int)motor.Id, -1, -1);
            _ch.WriteVariable(motor.CriticalErrIdle, "CERRI", ProgramBuffer.ACSC_NONE,
                (int)motor.Id, (int)motor.Id, -1, -1);
            _ch.WriteVariable(motor.SoftLimitNagtive, "SLLIMIT", ProgramBuffer.ACSC_NONE,
                (int)motor.Id, (int)motor.Id, -1, -1);
            _ch.WriteVariable(motor.SoftLimitPositive, "SRLIMIT", ProgramBuffer.ACSC_NONE,
                (int)motor.Id, (int)motor.Id, -1, -1);
        }

        private void WriteVariable(Motor motor, string acsVariable, object value)
        {
            _ch.WriteVariable(value, acsVariable, ProgramBuffer.ACSC_NONE,
               (int)motor.Id, (int)motor.Id, -1, -1);
        }

        private void MapEtherCat()
        {
            foreach (var motor in Motors)
            {
                _ch.MapEtherCATInput(MotionFlags.ACSC_NONE, motor.EcatPosActValAddr, GetEcatActPosName(motor));
            }
        }

        private string GetEcatActPosName(Motor motor)
        {
            return EcatActPosName + motor.EcatPosActValAddr;
        }

        private void DeclareVariableInDBuffer()
        {
            string Warning = "! Warning, generated by Host for Motor Abs Position, do not change." + _newLine;

            string EcatActPosDefine = string.Empty;
            foreach (var motor in Motors)
            {
                EcatActPosDefine += GlobalDefine + GetEcatActPosName(motor) + _newLine;
            }

            string program = Warning + EcatActPosDefine;
            ProgramBuffer DBuffer = (ProgramBuffer)(Convert.ToInt16(_ch.GetDBufferIndex()));

            string originProgram = _ch.UploadBuffer(DBuffer);
            if (originProgram != null)
            {
                if (originProgram.Contains(EcatActPosName) == false)
                {
                    _ch.AppendBuffer(DBuffer, program);

                    for (int i = 0; i < 9; i++)
                    {
                        _ch.ClearBuffer((ProgramBuffer)i, 1, 2000);
                    }
                    _ch.CompileBuffer(DBuffer);
                }
            }
            else
            {
                _ch.AppendBuffer(DBuffer, program);

                for (int i = 0; i < 9; i++)
                {
                    _ch.ClearBuffer((ProgramBuffer)i, 1, 2000);
                }
                _ch.CompileBuffer(DBuffer);
            }

            MapEtherCat();
        }

        public void SetPosition(Motor motor, double position) { }

        public void SetVelocity(Motor motor, double velocity)
        {
            _ch.SetVelocity(motor.Id, velocity);
        }

        public void SetSpeed(double velocity)
        {
            foreach (var mtr in Motors)
            {
                _ch.SetVelocity(mtr.Id, velocity*mtr.SpeedFactor);
                _ch.SetAcceleration(mtr.Id, velocity * 10);
                _ch.SetDeceleration(mtr.Id, velocity * 10);
                _ch.SetKillDeceleration(mtr.Id, velocity * 100);
                _ch.SetJerk(mtr.Id, velocity * mtr.JerkFactor);
            }
        }

        public void SetSpeedImm(double velocity)
        {
            foreach (var mtr in Motors)
            {
                _ch.SetVelocityImm(mtr.Id, velocity * mtr.SpeedFactor);
                if (velocity<=1.0)
                {
                    continue;
                }
                _ch.SetAccelerationImm(mtr.Id, velocity * 10);
                _ch.SetDecelerationImm(mtr.Id, velocity * 10);
                _ch.SetKillDecelerationImm(mtr.Id, velocity * 100);
                _ch.SetJerkImm(mtr.Id, velocity * mtr.JerkFactor);
            }
        }

        public void SetAcceleration(Motor motor, double acceleration) { }
        public void SetDeceleration(Motor motor, double deceleration) { }
        public void SetJerk(Motor motor, double jerk) { }
        public double GetPosition(Motor motor)
        {
            return _ch.GetFPosition(motor.Id)*motor.Direction;
        }

        public double GetPositionX()
        {
            return _ch.GetFPosition(MotorX1.Id) + _ch.GetFPosition(MotorX2.Id);
        }

        public double GetVelocity(Motor motor)
        {
            return _ch.GetVelocity(motor.Id);
        }
        public double GetAcceleration(Motor motor) { return 0; }
        public double GetDeceleration(Motor motor) { return 0; }
        public double GetJerk(Motor motor) { return 0; }

        public void Enable(Motor motor, int timeout=2000)
        {
            _ch.Enable(motor.Id);
            //Wait till enabled
            _ch.WaitMotorEnabled(motor.Id, 1, timeout);
        }

        public void EnableAll(int timeout = 2000)
        {
            foreach (var mtr in Motors)
            {
                _ch.Enable(mtr.Id);
                //Wait till enabled
                _ch.WaitMotorEnabled(mtr.Id, 1, timeout);
            }
        }

        public void Disable(Motor motor, int timeout = 2000)
        {
            _ch.Disable(motor.Id);
            //Wait till enabled
            _ch.WaitMotorEnabled(motor.Id, 0, timeout);
        }

        public void DisableAll(int timeout = 2000)
        {
            foreach (var mtr in Motors)
            {
                _ch.Disable(mtr.Id);
                //Wait till enabled
                _ch.WaitMotorEnabled(mtr.Id, 0, timeout);
            }
        }
        public void Jog(Motor motor, double velocity) { }

        public void KillAll()
        {
            _ch.KillAll();
        }

        public void ToPoint(Motor motor, double point)
        {
            point *= motor.Direction;
            _ch.ToPoint(MotionFlags.ACSC_AMF_MAXIMUM, motor.Id, point);
        }

        public void BreakToPoint(Motor motor, double point)
        {
            Break(motor);
            point *= motor.Direction;
            _ch.ToPoint(MotionFlags.ACSC_AMF_MAXIMUM, motor.Id, point);
        }

        public void BreakToPointWaitTillEnd(Motor motor, double point, int timeout = 60000)
        {
            Break(motor);
            point *= motor.Direction;
            _ch.ToPoint(MotionFlags.ACSC_AMF_MAXIMUM, motor.Id, point);
            _ch.WaitMotionEnd(motor.Id, timeout);
        }

        public void Break(Motor motor)
        {
            _ch.Break(motor.Id);
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
            _ch.ToPoint(MotionFlags.ACSC_AMF_MAXIMUM, motor.Id, point);
            _ch.WaitMotionEnd(motor.Id, timeout);
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

            _ch.ToPoint(MotionFlags.ACSC_AMF_MAXIMUM, MotorX1.Id, x1Target);
            _ch.ToPoint(MotionFlags.ACSC_AMF_MAXIMUM, MotorX2.Id, x2Target);
        }

        public void Jog(Motor motor, bool positiveDirection)
        {
            double vel = GetVelocity(motor);
            if (positiveDirection == false)
            {
                vel = -vel;
            }

            _ch.Jog(MotionFlags.ACSC_AMF_VELOCITY, motor.Id, vel);
        }

        public void Halt(Motor motor)
        {
            _ch.Halt(motor.Id);
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
            _ch.WaitMotionEnd(MotorX1.Id, timeout);
            _ch.WaitMotionEnd(MotorX2.Id, timeout);
        }

        public void WaitTillEnd(Motor motor, int timeout = 60000)
        {
            _ch.WaitMotionEnd(motor.Id, timeout);
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
            _ch.WaitMotionEnd(MotorX1.Id, timeout);
            _ch.WaitMotionEnd(MotorX2.Id, timeout);
        }

        public void ToPointM(Motor[] motors, double[] points)
        {
            Axis[] axes = new Axis[motors.Length+1];
            for (int i = 0; i < motors.Length; i++)
            {
                axes[i] = motors[i].Id;
            }
            axes[motors.Length] = Axis.ACSC_NONE;

            _ch.ToPointM(MotionFlags.ACSC_AMF_MAXIMUM, axes, points);
        }

        public void Test()
        {
            //Enable(MotorX1);

            //ToPoint(MotorX1, 260);

            //Console.WriteLine("doing");

            //Ch.WaitMotionEnd(Axis.ACSC_AXIS_1, 60000);

            //Console.WriteLine("Finish");

            //Enable(MotorX1);
            //Enable(MotorX2);

            //Motor[] motors = new Motor[2] { MotorX1, MotorX2 };
            //double[] pos = new double[2] { 260, 260 };

            //ToPointM(motors, pos);

            //Console.WriteLine("doing");

            //_ch.WaitMotionEnd(Axis.ACSC_AXIS_1, 60000);
            //Console.WriteLine("finis1");
            //_ch.WaitMotionEnd(Axis.ACSC_AXIS_2, 60000);

            //Console.WriteLine("Finish2");
        }

        public void Estop()
        {

        }

        public MotorStates GetRobotState(Motor motor)
        {
            return _ch.GetMotorState(motor.Id);
        }

    }
}
