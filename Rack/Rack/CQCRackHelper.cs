using System;

namespace Rack
{
    public partial class CqcRack
    {
        public TargetPosition TeachPos2TargetConverter(TeachPos teachPos)
        {
            TargetPosition target = Motion.HomePosition;
            switch (teachPos)
            {
                case TeachPos.Pick:
                    target = Motion.PickPosition;
                    break;
                case TeachPos.Bin:
                    target = Motion.BinPosition;
                    break;
                case TeachPos.Holder1:
                    target = Motion.ShieldBox1;
                    break;
                case TeachPos.Holder2:
                    target = Motion.ShieldBox2;
                    break;
                case TeachPos.Holder3:
                    target = Motion.ShieldBox3;
                    break;
                case TeachPos.Holder4:
                    target = Motion.ShieldBox4;
                    break;
                case TeachPos.Holder5:
                    target = Motion.ShieldBox5;
                    break;
                case TeachPos.Holder6:
                    target = Motion.ShieldBox6;
                    break;
                case TeachPos.Gold1:
                    target = Motion.Gold1;
                    break;
                case TeachPos.Gold2:
                    target = Motion.Gold2;
                    break;
                case TeachPos.Gold3:
                    target = Motion.Gold3;
                    break;
                case TeachPos.Gold4:
                    target = Motion.Gold4;
                    break;
                case TeachPos.Gold5:
                    target = Motion.Gold5;
                    break;
                default:
                    break;
            }

            return target;
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
                Steppers.SetSpeed(StepperMotor.One, stepperSpeed);
                Steppers.SetSpeed(StepperMotor.Two, stepperSpeed);
            }
        }

        public void SetMotorSpeed(double speed)
        {
            Motion.SetSpeed(speed);
        }

        public void SetRobotSpeedImm(double speed)
        {
            Motion.SetSpeedImm(speed);
        }

    }
}
