using System;

namespace Rack
{
    public partial class CqcRack
    {
        public ShieldBox ConvertIdToShieldBox(int id)
        {
            switch (id)
            {
                case 1:
                    return ShieldBox1;
                case 2:
                    return ShieldBox2;
                case 3:
                    return ShieldBox3;
                case 4:
                    return ShieldBox4;
                case 5:
                    return ShieldBox5;
                case 6:
                    return ShieldBox6;
                default:
                    throw new Exception("Shield box Id out of range exception.");
            }
        }

        private TargetPosition ConvertShieldBoxToTargetPosition(ShieldBox shieldBox)
        {
            switch (shieldBox.Id)
            {
                case 1:
                    return Motion.ShieldBox1;
                case 2:
                    return Motion.ShieldBox2;
                case 3:
                    return Motion.ShieldBox3;
                case 4:
                    return Motion.ShieldBox4;
                case 5:
                    return Motion.ShieldBox5;
                case 6:
                    return Motion.ShieldBox6;
                default:
                    throw new Exception("Shield box Id out of range exception.");
            }
        }

        public TargetPosition ConverterTeachPosToTargetPosition(TeachPos teachPos)
        {
            switch (teachPos)
            {
                case TeachPos.Pick:
                    return Motion.PickPosition;
                case TeachPos.Bin:
                    return Motion.BinPosition;
                case TeachPos.ShieldBox1:
                    return Motion.ShieldBox1;
                case TeachPos.ShieldBox2:
                    return Motion.ShieldBox2;
                case TeachPos.ShieldBox3:
                    return Motion.ShieldBox3;
                case TeachPos.ShieldBox4:
                    return Motion.ShieldBox4;
                case TeachPos.ShieldBox5:
                    return Motion.ShieldBox5;
                case TeachPos.ShieldBox6:
                    return Motion.ShieldBox6;
                case TeachPos.Gold1:
                    return Motion.Gold1;
                case TeachPos.Gold2:
                    return Motion.Gold2;
                case TeachPos.Gold3:
                    return Motion.Gold3;
                case TeachPos.Gold4:
                    return Motion.Gold4;
                case TeachPos.Gold5:
                    return Motion.Gold5;
                default:
                    throw new Exception("Shield box Id out of range exception.");
            }
        }

        public ShieldBox ConverterTeachPosToShieldBox(TeachPos teachPos)
        {
            switch (teachPos)
            {
                case TeachPos.ShieldBox1:
                    return ShieldBox1;
                case TeachPos.ShieldBox2:
                    return ShieldBox2;
                case TeachPos.ShieldBox3:
                    return ShieldBox3;
                case TeachPos.ShieldBox4:
                    return ShieldBox4;
                case TeachPos.ShieldBox5:
                    return ShieldBox5;
                case TeachPos.ShieldBox6:
                    return ShieldBox6;
                default:
                    return null;
            }
        }

        private TargetPosition ConvertBoxIdToTargetPosition(long id)
        {
            switch (id)
            {
                case 1:
                    return Motion.ShieldBox1;
                case 2:
                    return Motion.ShieldBox2;
                case 3:
                    return Motion.ShieldBox3;
                case 4:
                    return Motion.ShieldBox4;
                case 5:
                    return Motion.ShieldBox5;
                case 6:
                    return Motion.ShieldBox6;
                    default:
                        throw new Exception("Shield box Id out of range exception.");
            }
        }

        private TargetPosition ConvertGoldIdToTargetPosition(long id)
        {
            switch (id)
            {
                case -1:
                    return Motion.Gold1;
                case -2:
                    return Motion.Gold2;
                case -3:
                    return Motion.Gold3;
                case -4:
                    return Motion.Gold4;
                case -5:
                    return Motion.Gold5;
                default:
                    throw new Exception("Gold Id out of range exception.");
            }
        }

        private TargetPosition GetRobotCurrentPose()
        {
            TargetPosition currentPosition = new TargetPosition
            {
                XPos = Motion.GetPositionX(),
                YPos = Motion.GetPosition(Motion.MotorY),
                ZPos = Motion.GetPosition(Motion.MotorZ),
                RPos = Motion.GetPosition(Motion.MotorR)
            };
            return currentPosition;
        }

        public void SetRobotSpeed(double speed)
        {
            Motion.SetSpeed(speed);           
        }

        public void SetStepperSpeed(double speed)
        {
            if (StepperOnline)
            {
                int stepperSpeed = Convert.ToInt16(speed / 5.0);
                stepperSpeed++;
                if (stepperSpeed > 30)
                {
                    stepperSpeed = 30;
                }
                Steppers.SetSpeed(RackGripper.One, stepperSpeed);
                Steppers.SetSpeed(RackGripper.Two, stepperSpeed);
            }
        }

        public void SetMotorSpeed(double speed)
        {
            Motion.SetSpeed(speed);
        }

        private void SetRobotSpeedImm(double speed)
        {
            //Motion.SetSpeedImm(speed);
        }

    }
}
