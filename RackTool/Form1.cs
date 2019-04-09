using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Diagnostics;
using Rack;
using Motion;
using Tools;
using GripperStepper;
using System.Threading;
using ACS.SPiiPlusNET;
using Input = EcatIo.Input;
using Outout = EcatIo.Output;
namespace RackTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //Todo set tab draw mode to ownerdraw
            tabControl1.DrawItem += new DrawItemEventHandler(tabControl1_DrawItem);
        }

        private void tabControl1_DrawItem(Object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush;

            // Get the item from the collection.
            TabPage _tabPage = tabControl1.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            Rectangle _tabBounds = tabControl1.GetTabRect(e.Index);

            if (e.State == DrawItemState.Selected)
            {

                // Draw a different background color, and don't paint a focus rectangle.
                _textBrush = new SolidBrush(Color.Red);
                g.FillRectangle(Brushes.Gray, e.Bounds);
            }
            else
            {
                _textBrush = new System.Drawing.SolidBrush(e.ForeColor);
                e.DrawBackground();
            }

            // Use our own font.
            Font _tabFont = new Font("Arial", 10.0f, FontStyle.Bold, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Center;
            _stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));
        }

        private readonly CqcRack _rack = new CqcRack("192.168.8.18");
        private TeachPos _selectedTargetPosition;
        private StepperMotor _selectedGripper;
        private Thread _uiUpdateThread;

        private void button_Start_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Start();
                SetupForTeaching();

                if (_uiUpdateThread == null)
                {
                    _uiUpdateThread = new Thread(UiUpdate)
                    {
                        IsBackground = true
                    };
                }

                if (_uiUpdateThread.IsAlive == false)
                {
                    _uiUpdateThread.Start();
                }

                buttonHome.Enabled = true;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void UiUpdate()
        {
            while (true)
            {
                try
                {
                    labelPositionG1.Invoke((MethodInvoker) (() => { labelPositionG1.Text=_rack.Stepper.GetPosition(StepperMotor.One).ToString(); }));
                    labelPositionG2.Invoke((MethodInvoker)(() => { labelPositionG2.Text = _rack.Stepper.GetPosition(StepperMotor.Two).ToString(); }));
                    labelPositionX.Invoke((MethodInvoker)(() => { labelPositionX.Text = _rack.Motion.GetPositionX().ToString(); }));
                    labelPositionY.Invoke((MethodInvoker)(() => { labelPositionY.Text = _rack.Motion.GetPosition(_rack.Motion.MotorY).ToString(); }));
                    labelPositionZ.Invoke((MethodInvoker)(() => { labelPositionZ.Text = _rack.Motion.GetPosition(_rack.Motion.MotorZ).ToString(); }));
                    labelPositionR.Invoke((MethodInvoker)(() => { labelPositionR.Text = _rack.Motion.GetPosition(_rack.Motion.MotorR).ToString(); }));
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
            }
        }

        private void Invoke(Control control, MethodInvoker action)
        {
            control.Invoke(action);
        }

        private void SetupForTeaching()
        {

            comboBox_Gripper.Items.Clear();
            foreach (var pos in Enum.GetValues(typeof(StepperMotor)))
            {
                comboBox_Gripper.Items.Add(pos);
            }

            comboBoxMovePos.Items.Clear();
            foreach (var pos in Enum.GetValues(typeof(TeachPos)))
            {
                comboBoxMovePos.Items.Add(pos);
            }
        }

        private void button_Home_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {

                try
                {
                    _rack.HomeRobot();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        //Test
        private void button3_Click(object sender, EventArgs e)
        {
            //DialogResult result = MessageBox.Show("是否所有屏蔽箱门都打开了？", "!!!", MessageBoxButtons.YesNo);
            //if (result == DialogResult.No)
            //{
            //    return;
            //}
            _rack.SetSpeed(defaultTestSpeed);

            Task.Run(() =>
            {

                try
                {
                    _rack.Test();
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Stop();     
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void buttonCreateXml_Click(object sender, EventArgs e)
        {
            XmlReaderWriter.CreateStorageFile("RackData.xml");
        }

        bool testLoop = true;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            testLoop = checkBoxIsLoop.Enabled;
            buttonTest.Enabled = true;
        }
        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.SaveTeachPosition(_selectedTargetPosition);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }


        private void buttonSaveApproach_Click(object sender, EventArgs e)
        {
            try
            {
                //Todo is approach is lower than teach, then exception.
                _rack.SaveApproachHeight(_selectedTargetPosition);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        #region Manual control

        private void button8_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Jog(_rack.Motion.MotorX1, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button8_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Halt(_rack.Motion.MotorX1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void button9_MouseDown(object sender, MouseEventArgs e)
        {
            
            try
            {
                _rack.Motion.Jog(_rack.Motion.MotorX1, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button9_MouseUp(object sender, MouseEventArgs e)
        {

            try
            {
                _rack.Motion.Halt(_rack.Motion.MotorX1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button10_MouseDown(object sender, MouseEventArgs e)
        {
            
            try
            {
                _rack.Motion.Jog(_rack.Motion.MotorX2, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button11_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Jog(_rack.Motion.MotorX2, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void buttonNagetiveX2_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Halt(_rack.Motion.MotorX2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button15_MouseDown(object sender, MouseEventArgs e)
        {
            
            try
            {
                _rack.Motion.Jog(_rack.Motion.MotorY, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button13_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Jog(_rack.Motion.MotorZ, false);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button17_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Jog(_rack.Motion.MotorR, false);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Stepper.ToPoint(StepperMotor.One, Convert.ToDouble(textBoxDistanceG1.Text));
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Stepper.ToPoint(StepperMotor.Two, Convert.ToDouble(textBoxDistanceG2.Text));
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button14_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Jog(_rack.Motion.MotorY, true);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button12_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Jog(_rack.Motion.MotorZ, true);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button16_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Jog(_rack.Motion.MotorR, true);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button11_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Halt(_rack.Motion.MotorX2);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button14_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Halt(_rack.Motion.MotorY);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button12_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Halt(_rack.Motion.MotorZ);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button16_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Halt(_rack.Motion.MotorR);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button13_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Halt(_rack.Motion.MotorZ);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button17_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Halt(_rack.Motion.MotorR);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button15_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _rack.Motion.Halt(_rack.Motion.MotorY);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        #endregion
        private void buttonLoad_Click(object sender, EventArgs e)
        {
            _rack.SetSpeed(defaultTestSpeed);

            Task.Run((Action)(() =>
            {
                //Todo complete condition.
                TargetPosition target = _rack.Motion.HomePosition;
                switch (this._selectedTargetPosition)
                {
                    case TeachPos.Home:
                        break;
                    case TeachPos.Pick:
                        target = _rack.Motion.PickPosition;
                        break;
                    case TeachPos.Bin:
                        break;
                    case TeachPos.ConveyorLeft:
                        break;
                    case TeachPos.ConveyorRight:
                        break;
                    case TeachPos.Holder1:
                        target = _rack.Motion.Holder1;
                        break;
                    case TeachPos.Holder2:
                        target = _rack.Motion.Holder2;
                        break;
                    case TeachPos.Holder3:
                        target = _rack.Motion.Holder3;
                        break;
                    case TeachPos.Holder4:
                        target = _rack.Motion.Holder4;
                        break;
                    case TeachPos.Holder5:
                        target = _rack.Motion.Holder5;
                        break;
                    case TeachPos.Holder6:
                        target = _rack.Motion.Holder6;
                        break;
                    case TeachPos.Gold1:
                        break;
                    case TeachPos.Gold2:
                        break;
                    case TeachPos.Gold3:
                        break;
                    case TeachPos.Gold4:
                        break;
                    case TeachPos.Gold5:
                        break;
                }

                try
                {
                    _rack.Load(_selectedGripper, target);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }));

        }

        private void comboBox_Gripper_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedGripper = (StepperMotor)comboBox_Gripper.SelectedItem;
        }

        private void comboBoxMovePos_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedTargetPosition = (TeachPos)comboBoxMovePos.SelectedItem;
        }

        private double defaultTestSpeed = 5;

        private async void buttonPick_Click(object sender, EventArgs e)
        {
            _rack.SetSpeed(defaultTestSpeed);

            await Task.Run(() =>
            {
                try
                {
                    _rack.Pick(_selectedGripper);
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            });
        }

        private async void buttonPlace_Click(object sender, EventArgs e)
        {
            _rack.SetSpeed(defaultTestSpeed);

            await Task.Run(() =>
            {
                try
                {
                    _rack.Place(_selectedGripper);
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            });
        }
        private void buttonUnload_Click(object sender, EventArgs e)
        {
            _rack.SetSpeed(defaultTestSpeed);

            Task.Run((Action)(() =>
            {
                //Todo complete condition.
                TargetPosition target = _rack.Motion.HomePosition;
                switch (this._selectedTargetPosition)
                {
                    case TeachPos.Home:
                        break;
                    case TeachPos.Pick:
                        target = _rack.Motion.PickPosition;
                        break;
                    case TeachPos.Bin:
                        break;
                    case TeachPos.ConveyorLeft:
                        break;
                    case TeachPos.ConveyorRight:
                        break;
                    case TeachPos.Holder1:
                        target = _rack.Motion.Holder1;
                        break;
                    case TeachPos.Holder2:
                        target = _rack.Motion.Holder2;
                        break;
                    case TeachPos.Holder3:
                        target = _rack.Motion.Holder3;
                        break;
                    case TeachPos.Holder4:
                        target = _rack.Motion.Holder4;
                        break;
                    case TeachPos.Holder5:
                        target = _rack.Motion.Holder5;
                        break;
                    case TeachPos.Holder6:
                        target = _rack.Motion.Holder6;
                        break;
                    case TeachPos.Gold1:
                        break;
                    case TeachPos.Gold2:
                        break;
                    case TeachPos.Gold3:
                        break;
                    case TeachPos.Gold4:
                        break;
                    case TeachPos.Gold5:
                        break;
                }

                try
                {
                    _rack.Unload(_selectedGripper, target);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }));
        }

        private void button26_Click(object sender, EventArgs e)
        {
            _rack.ReadyThePhone();
        }

        private void checkBoxPickConveyorMoveForward_CheckedChanged(object sender, EventArgs e)
        {
            _rack.Conveyor.ConveyorMovingForward = checkBoxPickConveyorMoveForward.Checked;
        }

        private void button38_Click(object sender, EventArgs e)
        {
            //_rack._conveyor.UpBlockSeparate(false);
            //_rack._conveyor.InitialState();
            //_rack._conveyor.UpBlockSeparate(true);
            _rack.Conveyor.Start();
        }

        private void button39_Click(object sender, EventArgs e)
        {
            _rack.Conveyor.CommandInposForPicking = true;
        }

        private void button40_Click(object sender, EventArgs e)
        {
            _rack.Conveyor.CommandReadyForPicking = true;
        }

        private void button41_Click(object sender, EventArgs e)
        {
            _rack.Conveyor.InposForPicking = false;
        }

        

        private void trackBarSetSpeed_Scroll(object sender, EventArgs e)
        {
            labelSpeed2.Text = trackBarSetSpeed2.Value.ToString();
            _rack.SetSpeedImm(Convert.ToDouble(trackBarSetSpeed2.Value));
        }

        private void trackBarSetSpeed1_Scroll(object sender, EventArgs e)
        {
            labelSpeed1.Text = trackBarSetSpeed1.Value.ToString();
            _rack.SetSpeedImm(Convert.ToDouble(trackBarSetSpeed1.Value));
        }

        private void RefleshRobotUi()
        {
            try
            {
                foreach (var item in _rack.Motion.Motors)
                {
                    MotorStates State = _rack.Motion.GetRobotState(item);
                    switch (item.Id)
                    {
                        case Axis.ACSC_AXIS_0: buttonEableZ.Text = Convert.ToBoolean(State & MotorStates.ACSC_MST_ENABLE) ? "Disable" : "Enable"; break;
                        case Axis.ACSC_AXIS_1: buttonEableX1.Text = Convert.ToBoolean(State & MotorStates.ACSC_MST_ENABLE) ? "Disable" : "Enable"; break;
                        case Axis.ACSC_AXIS_2: buttonEableX2.Text = Convert.ToBoolean(State & MotorStates.ACSC_MST_ENABLE) ? "Disable" : "Enable"; break;
                        case Axis.ACSC_AXIS_3: buttonEableY.Text = Convert.ToBoolean(State & MotorStates.ACSC_MST_ENABLE) ? "Disable" : "Enable"; break;
                        case Axis.ACSC_AXIS_4: buttonEableR.Text = Convert.ToBoolean(State & MotorStates.ACSC_MST_ENABLE) ? "Disable" : "Enable"; break;
                        default:
                            break;
                    }
                }
                buttonG1TightOrLoose.Text = _rack.Io.GetInput(Input.Gripper01Tight) ? "Loose" : "Tight";
                buttonG2TightOrLoose.Text = _rack.Io.GetInput(Input.Gripper02Tight) ? "Loose" : "Tight";
                buttonEableG1.Text = _rack.Stepper.GetStatus(StepperMotor.One, StatusCode.Enabled) ? "Disable" : "Enable";
                buttonEableG2.Text = _rack.Stepper.GetStatus(StepperMotor.Two, StatusCode.Enabled) ? "Disable" : "Enable";
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
           
        }

        #region Enable
        private void button22_Click(object sender, EventArgs e)
        {
            if (_rack.Stepper.GetStatus(StepperMotor.One, StatusCode.Enabled))
            {
                _rack.Stepper.Disable(StepperMotor.One);
            }
            else
            {
                _rack.Stepper.Enable(StepperMotor.One);
            }
            RefleshRobotUi();
        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (_rack.Stepper.GetStatus(StepperMotor.Two, StatusCode.Enabled))
            {
                _rack.Stepper.Disable(StepperMotor.Two);
            }
            else
            {
                _rack.Stepper.Enable(StepperMotor.Two);
            }
            RefleshRobotUi();
        }

        private void buttonEableX1_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonEableX1.Text == "Enable")
                    _rack.Motion.Enable(_rack.Motion.MotorX1);

                else
                    _rack.Motion.Disable(_rack.Motion.MotorX1);
                RefleshRobotUi();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void buttonEableX2_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonEableX2.Text == "Enable")
                    _rack.Motion.Enable(_rack.Motion.MotorX2);

                else
                    _rack.Motion.Disable(_rack.Motion.MotorX2);
                RefleshRobotUi();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonEableY_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonEableY.Text == "Enable")
                    _rack.Motion.Enable(_rack.Motion.MotorY);

                else
                    _rack.Motion.Disable(_rack.Motion.MotorY);
                RefleshRobotUi();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonEableR_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonEableR.Text == "Enable")
                    _rack.Motion.Enable(_rack.Motion.MotorR);

                else
                    _rack.Motion.Disable(_rack.Motion.MotorR);
                RefleshRobotUi();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonEableZ_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonEableZ.Text == "Enable")
                    _rack.Motion.Enable(_rack.Motion.MotorZ);

                else
                    _rack.Motion.Disable(_rack.Motion.MotorZ);
                RefleshRobotUi();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        private void buttonG1TightOrLoose_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonG1TightOrLoose.Text == "G1Open")
                {
                   _rack.OpenGripper(StepperMotor.One);
                    buttonG1TightOrLoose.Text = "G1Close";
                }
                else
                {
                    _rack.CloseGripper(StepperMotor.One);
                    buttonG1TightOrLoose.Text = "G1Open";
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonG2TightOrLoose_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonG2TightOrLoose.Text == "G2Open")
                {
                    _rack.OpenGripper(StepperMotor.Two);
                    buttonG2TightOrLoose.Text = "G2Close";
                }
                else
                {
                    _rack.CloseGripper(StepperMotor.Two);
                    buttonG2TightOrLoose.Text = "G2Open";
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonHighSpeed_Click(object sender, EventArgs e)
        {

        }


    }
}
