using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rack;
using System.Drawing;
namespace RackTool
{
    public partial class Main
    {
        private void RefleshConveyorUI()
        {
            try
            {
                #region LabelReflesh
                labelConveyorBinInTwo.ForeColor = _rack.EcatIo.GetInput(Input.ConveyorBinInTwo) ? Color.Green : Color.Gray;
                labelConveyorBinIn.ForeColor = _rack.EcatIo.GetInput(Input.ConveyorBinIn) ? Color.Green : Color.Gray;
                labelConveyorOneIn.ForeColor = _rack.EcatIo.GetInput(Input.ConveyorOneIn) ? Color.Green : Color.Gray;
                labelConveyorOneOut.ForeColor = _rack.EcatIo.GetInput(Input.ConveyorOneOut) ? Color.Green : Color.Gray;
                labelPickBufferHasPhoneForward.ForeColor = _rack.EcatIo.GetInput(Input.PickBufferHasPhoneForward) ? Color.Green : Color.Gray;
                labelPickHasPhone.ForeColor = _rack.EcatIo.GetInput(Input.PickHasPhone) ? Color.Green : Color.Gray;
                labelPickBufferHasPhoneBackward.ForeColor = _rack.EcatIo.GetInput(Input.PickBufferHasPhoneBackward) ? Color.Green : Color.Gray;

                labelGold1.ForeColor = _rack.EcatIo.GetInput(Input.Gold1) ? Color.Gray : Color.Green;
                labelGold2.ForeColor = _rack.EcatIo.GetInput(Input.Gold2) ? Color.Gray : Color.Green;
                labelGold3.ForeColor = _rack.EcatIo.GetInput(Input.Gold3) ? Color.Gray : Color.Green;
                labelGold4.ForeColor = _rack.EcatIo.GetInput(Input.Gold4) ? Color.Gray : Color.Green;
                labelGold5.ForeColor = _rack.EcatIo.GetInput(Input.Gold5) ? Color.Gray : Color.Green;
                #endregion

                #region ButtonReflesh
                buttonUpBlockSeparateForward.Text = _rack.EcatIo.GetInput(Input.UpBlockSeparateForward)? "Down" : "Up";
                buttonUpBlockPickBackward.Text = _rack.EcatIo.GetInput(Input.UpBlockPickBackward) ? "Down" : "Up";
                buttonUpBlockPickForward.Text = _rack.EcatIo.GetInput(Input.UpBlockPickForward) ? "Down" : "Up";
                buttonUpBlockSeparateBackward.Text = _rack.EcatIo.GetInput(Input.UpBlockSeparateBackward) ? "Down" : "Up";

                buttonOpenOrClose.Text = _rack.EcatIo.GetInput(Input.ClampTightPick) ? "OPen" : "Close";
                buttonSideBlockSeparateForward.Text = _rack.EcatIo.GetInput(Input.SideBlockSeparateForward) ? "Stretch" : "Retract";
                buttonSideBlockPick.Text = _rack.EcatIo.GetInput(Input.SideBlockPick) ? "Stretch" : "Retract";
                buttonSideBlockSeparateBackward.Text = _rack.EcatIo.GetInput(Input.SideBlockSeparateBackward) ? "Stretch" : "Retract";
                #endregion
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
