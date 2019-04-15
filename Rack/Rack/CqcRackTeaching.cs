using System;
using System.Globalization;
using System.Threading;
using System.Diagnostics;

namespace Rack
{
    public partial class CqcRack
    {
        public void SaveTeachPosition(TeachPos selectedTeachPos)
        {
            XmlReaderWriter.SetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.XPos,
                Motion.GetPositionX().ToString(CultureInfo.CurrentCulture));
            XmlReaderWriter.SetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.YPos,
                Motion.GetPosition(Motion.MotorY).ToString(CultureInfo.CurrentCulture));
            XmlReaderWriter.SetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.ZPos,
                Motion.GetPosition(Motion.MotorZ).ToString(CultureInfo.CurrentCulture));
            XmlReaderWriter.SetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.RPos,
                Motion.GetPosition(Motion.MotorR).ToString(CultureInfo.CurrentCulture));

            XmlReaderWriter.SetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.APos,
                Steppers.GetPosition(StepperMotor.One).ToString(CultureInfo.CurrentCulture));
        }

        public void SaveApproachHeight(TeachPos selectedTeachPos)
        {
            XmlReaderWriter.SetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.ApproachHeight,
                Motion.GetPosition(Motion.MotorZ).ToString(CultureInfo.CurrentCulture));
        }

        public void CalculateG1ToG2Offset(TeachPos selectedTeachPos)
        {
            double XPos = Convert.ToDouble(
                XmlReaderWriter.GetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.XPos));
            double YPos = Convert.ToDouble(
                XmlReaderWriter.GetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.YPos));
            double APos = Convert.ToDouble(
                XmlReaderWriter.GetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.APos));

            double xOffset = Motion.GetPositionX() - XPos;
            double yOffset = Motion.GetPosition(Motion.MotorY) - YPos;
            double aOffset = Steppers.GetPosition(StepperMotor.Two) - APos;

            if (Math.Abs(xOffset)>5 | Math.Abs(yOffset) > 5 | Math.Abs(aOffset) > 5)
            {
                throw new Exception("CalculateG1ToG2Offset offset over 5.");
            }

            XmlReaderWriter.SetTeachAttribute(Files.RackData, TeachPos.G1ToG2Offset, PosItem.XPos,
                xOffset.ToString(CultureInfo.CurrentCulture));
            XmlReaderWriter.SetTeachAttribute(Files.RackData, TeachPos.G1ToG2Offset, PosItem.YPos,
                yOffset.ToString(CultureInfo.CurrentCulture));

            XmlReaderWriter.SetTeachAttribute(Files.RackData, TeachPos.G1ToG2Offset, PosItem.APos,
                aOffset.ToString(CultureInfo.CurrentCulture));
        }

        public void DisableMotorsForTeaching()
        {
            Motion.Disable(Motion.MotorX1);
            Motion.Disable(Motion.MotorX2);
            Motion.Disable(Motion.MotorY);
            Steppers.Disable(StepperMotor.One);
            Steppers.Disable(StepperMotor.Two);
        }

        public void EnableMotorsForTeaching()
        {
            Motion.Enable(Motion.MotorX1);
            Motion.Enable(Motion.MotorX2);
            Motion.Enable(Motion.MotorY);
            Steppers.Enable(StepperMotor.One);
            Steppers.Enable(StepperMotor.Two);
        }

        public void LoadForTeaching(StepperMotor gripper, TeachPos selectedTeachPos)
        {
            TargetPosition target = ConverterTeachPosToTargetPosition(selectedTeachPos);
            target.ZPos = target.ZPos + 30;
            MoveToTargetPosition(gripper, target);
            DisableMotorsForTeaching();
            Motion.SetSpeedImm(3);
        }

        public void ReadyThePhone(int timeout = 3000)
        {
            Conveyor.Clamp(false);
            Conveyor.UpBlockSeparate(false);
            Conveyor.UpBlockPick(false);
            Conveyor.SideBlockSeparate(false);
            Conveyor.ConveyorMovingForward = !Conveyor.ConveyorMovingForward;
            Conveyor.UpBlockSeparate(false);
            Conveyor.UpBlockPick(false);
            Conveyor.SideBlockSeparate(false);
            Conveyor.ConveyorMovingForward = !Conveyor.ConveyorMovingForward;
            EcatIo.SetOutput(Output.ClampPick, true);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!EcatIo.GetInput(Input.ClampTightPick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            EcatIo.SetOutput(Output.SideBlockPick, true);
            sw.Restart();
            while (EcatIo.GetInput(Input.SideBlockPick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            Thread.Sleep(500);

            EcatIo.SetOutput(Output.SideBlockPick, false);
            sw.Restart();
            while (!EcatIo.GetInput(Input.SideBlockPick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            EcatIo.SetOutput(Output.ClampPick, false);
            sw = new Stopwatch();
            sw.Restart();
            while (!EcatIo.GetInput(Input.ClampLoosePick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            Conveyor.ReadyForPicking = true;
        }
    }
}
