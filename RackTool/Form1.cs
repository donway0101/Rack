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
using Rack;
using Rack;
using Rack;
using System.Threading;
using ACS.SPiiPlusNET;
using Input = Rack.Input;
using Outout = Rack.Output;
using System.IO;
using System.IO.Ports;
using System.Collections;
using Rack;

namespace RackTool
{
    public partial class Form1 : Form
    {
        private readonly CqcRack _rack = new CqcRack("192.168.8.18");
        private TeachPos _selectedTargetPosition;
        private StepperMotor _selectedGripper;
        private Thread _uiUpdateThread;
        private ArrayList _portName;

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

        private async void button_Start_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;
            await Task.Run(() =>
            {
                try
                {
                    _rack.Start();
                    
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
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            });

            SetupForTeaching();

            buttonHome.Enabled = true;
            buttonStart.Enabled = true;
        }
        private void UiUpdate()
        {
            while (true)
            {
                try
                {
                    labelPositionG1.Invoke((MethodInvoker) (() => { labelPositionG1.Text=_rack.Steppers.GetPosition(StepperMotor.One).ToString(); }));
                    labelPositionG2.Invoke((MethodInvoker)(() => { labelPositionG2.Text = _rack.Steppers.GetPosition(StepperMotor.Two).ToString(); }));
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

        private async void button_Home_Click(object sender, EventArgs e)
        {
            buttonHome.Enabled = false;
            await Task.Run(() =>
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
            buttonHome.Enabled = true;
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
            try
            {
                XmlReaderWriter.CreateStorageFile("RackData.xml");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
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
                _rack.Steppers.ToPoint(StepperMotor.One, Convert.ToDouble(textBoxDistanceG1.Text));
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
                _rack.Steppers.ToPoint(StepperMotor.Two, Convert.ToDouble(textBoxDistanceG2.Text));
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
        private async void buttonLoad_Click(object sender, EventArgs e)
        {
            _rack.SetSpeed(defaultTestSpeed);

            await Task.Run((Action)(() =>
            {
                //Todo complete condition.
                TargetPosition target = _rack.TeachPos2TargetConverter(_selectedTargetPosition);

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
        private async void buttonUnload_Click(object sender, EventArgs e)
        {
            _rack.SetSpeed(defaultTestSpeed);

            await Task.Run((Action)(() =>
            {
                //Todo complete condition.
                TargetPosition target = _rack.TeachPos2TargetConverter(_selectedTargetPosition);

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
                buttonG1TightOrLoose.Text = _rack.EcatIo.GetInput(Input.Gripper01Tight) ? "G1Open" : "G1Close";
                buttonG2TightOrLoose.Text = _rack.EcatIo.GetInput(Input.Gripper02Tight) ? "G2Open" : "G2Close";
                buttonEableG1.Text = _rack.Steppers.GetStatus(StepperMotor.One, StatusCode.Enabled) ? "Disable" : "Enable";
                buttonEableG2.Text = _rack.Steppers.GetStatus(StepperMotor.Two, StatusCode.Enabled) ? "Disable" : "Enable";
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
           
        }

        #region Enable
        private void button22_Click(object sender, EventArgs e)
        {
            if (_rack.Steppers.GetStatus(StepperMotor.One, StatusCode.Enabled))
            {
                _rack.Steppers.Disable(StepperMotor.One);
            }
            else
            {
                _rack.Steppers.Enable(StepperMotor.One);
            }
            RefleshRobotUi();
        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (_rack.Steppers.GetStatus(StepperMotor.Two, StatusCode.Enabled))
            {
                _rack.Steppers.Disable(StepperMotor.Two);
            }
            else
            {
                _rack.Steppers.Enable(StepperMotor.Two);
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

        private async void buttonBin_Click(object sender, EventArgs e)
        {
            _rack.SetSpeed(defaultTestSpeed);

            await Task.Run(() =>
            {
                try
                {
                    //Todo check Box closed.
                    _rack.Bin(_selectedGripper);
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            });
        }

        private async void buttonUnloadAndLoad_Click(object sender, EventArgs e)
        {
            _rack.SetSpeed(defaultTestSpeed);

            await Task.Run(() =>
            {
                try
                {
                    TargetPosition target = _rack.TeachPos2TargetConverter(_selectedTargetPosition);
                    //Todo check Box closed.
                    _rack.UnloadAndLoad(target, _selectedGripper);
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void buttonReadyForPick_Click(object sender, EventArgs e)
        {            
            try
            {
                _rack.ReadyThePhone();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private async void buttonLoadForTeach_Click(object sender, EventArgs e)
        {
            _rack.SetSpeed(defaultTestSpeed);

            await Task.Run(() =>
            {
                try
                {
                    _rack.LoadForTeaching(_selectedGripper, _selectedTargetPosition);
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void buttonCalOffset_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.CalculateG1ToG2Offset( _selectedTargetPosition);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonLowSpeed_Click(object sender, EventArgs e)
        {
            trackBarSetSpeed2.Value = 5;
        }

        private void buttonMiddleSpeed_Click(object sender, EventArgs e)
        {
            trackBarSetSpeed2.Value = trackBarSetSpeed2.Maximum / 2;
        }
        private void buttonHighSpeed_Click(object sender, EventArgs e)
        {
            trackBarSetSpeed2.Value = trackBarSetSpeed2.Maximum;
        }

        private void 启用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int BoxId = 0;
            if (listViewBox.SelectedItems.Count != 0)
            {
                listViewBox.SelectedItems[0].ImageIndex = 0;
                switch (listViewBox.SelectedItems[0].Text)
                {
                    case "Box1": BoxId = 1;  break;
                    case "Box2": BoxId = 2;  break;
                    case "Box3": BoxId = 3; break;
                    case "Box4": BoxId = 4;  break;
                    case "Box5": BoxId = 5; break;
                    case "Box6": BoxId = 6;  break;
                    default:
                        break;
                }
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, BoxId, ShieldBoxItem.State, "Enable");
                buttonBoxLoad_Click(null,null);
            }
            else
            {
                MessageBox.Show("Please Select Box!");
            }
        }

        private void 禁用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewBox.SelectedItems.Count != 0)
            {
                int BoxId = 0;
                string Com = "None";
                if (listViewBox.SelectedItems.Count != 0)
                {
                    listViewBox.SelectedItems[0].ImageIndex = 1;
                    switch (listViewBox.SelectedItems[0].Text)
                    {
                        case "Box1": BoxId = 1; Com = comboBox1.Text; comboBox1.Text = "None";  break;
                        case "Box2": BoxId = 2; Com = comboBox2.Text; comboBox2.Text = "None";  break;
                        case "Box3": BoxId = 3; Com = comboBox3.Text; comboBox3.Text = "None";  break;
                        case "Box4": BoxId = 4; Com = comboBox4.Text; comboBox4.Text = "None";  break;
                        case "Box5": BoxId = 5; Com = comboBox5.Text; comboBox5.Text = "None";  break;
                        case "Box6": BoxId = 6; Com = comboBox6.Text; comboBox6.Text = "None";  break;
                        default:
                            break;
                    }
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, BoxId, ShieldBoxItem.State, "Disable");
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, BoxId, ShieldBoxItem.COM, "None");
                    if (_portName.Contains(Com) == false)
                        _portName.Add(Com);
                    buttonBoxLoad_Click(null, null);
                }
                else
                {
                    MessageBox.Show("Please Select Box!");
                }
            }
        }
        private void ClearItem()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();
            comboBox4.Items.Clear();
            comboBox5.Items.Clear();
            comboBox6.Items.Clear();
        }
        private void buttonBoxLoad_Click(object sender, EventArgs e)
        {
            string[] _PortName = SerialPort.GetPortNames();
            if (_portName == null)
                _portName = new ArrayList(_PortName);
            ClearItem();
            foreach (var item in _portName)
            {
                comboBox1.Items.Add(item);
                comboBox2.Items.Add(item);
                comboBox3.Items.Add(item);
                comboBox4.Items.Add(item);
                comboBox5.Items.Add(item);
                comboBox6.Items.Add(item);
            }
            comboBox1.Text = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.COM);
            comboBox2.Text = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.COM);
            comboBox3.Text = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.COM);
            comboBox4.Text = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.COM);
            comboBox5.Text = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.COM);
            comboBox6.Text = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.COM);
           
            listViewBox.Items[0].ImageIndex = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.State) == "Enable" ? 0 : 1;
            listViewBox.Items[1].ImageIndex = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.State) == "Enable" ? 0 : 1;
            listViewBox.Items[2].ImageIndex = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.State) == "Enable" ? 0 : 1;
            listViewBox.Items[3].ImageIndex = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.State) == "Enable" ? 0 : 1;
            listViewBox.Items[4].ImageIndex = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.State) == "Enable" ? 0 : 1;
            listViewBox.Items[5].ImageIndex = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.State) == "Enable" ? 0 : 1;

            comboBox1.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.State) == "Enable" ? true : false;
            buttonOK1.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.State) == "Enable" ? true : false;
            comboBox2.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.State) == "Enable" ? true : false;
            buttonOK2.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.State) == "Enable" ? true : false;
            comboBox3.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.State) == "Enable" ? true : false;
            buttonOK3.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.State) == "Enable" ? true : false;
            comboBox4.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.State) == "Enable" ? true : false;
            buttonOK4.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.State) == "Enable" ? true : false;
            comboBox5.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.State) == "Enable" ? true : false;
            buttonOK5.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.State) == "Enable" ? true : false;
            comboBox6.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.State) == "Enable" ? true : false;
            buttonOK6.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.State) == "Enable" ? true : false;
        }

        private void buttonBoxSave_Click(object sender, EventArgs e)
        {
            XmlReaderWriter.CreateBoxDataFile(Files.BoxData);
        }

        private void listViewBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        #region ButtonOk
        private void buttonOK1_Click(object sender, EventArgs e)
        {
            try
            {
                string Com = comboBox1.SelectedItem.ToString();
                _portName.Remove(Com);
                ClearItem();
                if (comboBox1.Items.Contains(Com) == false)
                    comboBox1.Items.Add(Com);
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1,ShieldBoxItem.COM,Com);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonOK2_Click(object sender, EventArgs e)
        {
            try
            {
                string Com = comboBox1.SelectedItem.ToString();
                _portName.Remove(Com);
                ClearItem();
                if (comboBox2.Items.Contains(Com) == false)
                    comboBox2.Items.Add(Com);
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.COM, Com);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonOK3_Click(object sender, EventArgs e)
        {
            try
            {
                string Com = comboBox3.SelectedItem.ToString();
                _portName.Remove(Com);
                ClearItem();
                if (comboBox3.Items.Contains(Com) == false)
                    comboBox3.Items.Add(Com);
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.COM, Com);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonOK4_Click(object sender, EventArgs e)
        {
            try
            {
                string Com = comboBox4.SelectedItem.ToString();
                _portName.Remove(Com);
                ClearItem();
                if (comboBox4.Items.Contains(Com) == false)
                    comboBox4.Items.Add(Com);
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.COM, Com);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonOK5_Click(object sender, EventArgs e)
        {
            try
            {
                string Com = comboBox5.SelectedItem.ToString();
                _portName.Remove(Com);
                ClearItem();
                if (comboBox5.Items.Contains(Com) == false)
                    comboBox5.Items.Add(Com);
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.COM, Com);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonOK6_Click(object sender, EventArgs e)
        {
            try
            {
                string Com = comboBox6.SelectedItem.ToString();
                _portName.Remove(Com);
                ClearItem();
                if (comboBox6.Items.Contains(Com) == false)
                    comboBox6.Items.Add(Com);
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.COM, Com);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "";
        }
    }
}
