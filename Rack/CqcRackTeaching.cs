using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using GripperStepper;
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
                Motion.GetPosition(Motion.MotorR) > 0
                    ? Gripper.GetPosition(GripperStepper.Gripper.One).ToString(CultureInfo.CurrentCulture)
                    : Gripper.GetPosition(GripperStepper.Gripper.Two).ToString(CultureInfo.CurrentCulture));
        }

        public void SaveApproachHeight(TeachPos selectedTeachPos)
        {
            XmlReaderWriter.SetTeachAttribute(Files.RackData, selectedTeachPos, PosItem.ApproachHeight,
                Motion.GetPosition(Motion.MotorZ).ToString(CultureInfo.CurrentCulture));
        }
    }
}
