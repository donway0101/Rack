using System;
using System.Globalization;
using System.Threading;
using Tools;
using EcatIo;
using System.Diagnostics;
using System.Reflection;
using GripperStepper;
using Motion;

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
                Stepper.GetPosition(Gripper.One).ToString(CultureInfo.CurrentCulture));
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
            double aOffset = Stepper.GetPosition(Gripper.Two) - APos;

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
            Stepper.Disable(Gripper.One);
            Stepper.Disable(Gripper.Two);
        }

        public void EnableMotorsForTeaching()
        {
            Motion.Enable(Motion.MotorX1);
            Motion.Enable(Motion.MotorX2);
            Motion.Enable(Motion.MotorY);
            Stepper.Enable(Gripper.One);
            Stepper.Enable(Gripper.Two);
        }

        public void LoadForTeaching(Gripper gripper, TeachPos selectedTeachPos)
        {
            TargetPosition target = TeachPos2TargetConverter(selectedTeachPos);
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
            Io.SetOutput(Output.ClampPick, true);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!Io.GetInput(Input.ClampTightPick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            Io.SetOutput(Output.SideBlockPick, true);
            sw.Restart();
            while (Io.GetInput(Input.SideBlockPick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            Thread.Sleep(500);

            Io.SetOutput(Output.SideBlockPick, false);
            sw.Restart();
            while (!Io.GetInput(Input.SideBlockPick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            Io.SetOutput(Output.ClampPick, false);
            sw = new Stopwatch();
            sw.Restart();
            while (!Io.GetInput(Input.ClampLoosePick))
            {
                if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException();
                Thread.Sleep(10);
            }

            Conveyor.ReadyForPicking = true;
        }
    }
}
