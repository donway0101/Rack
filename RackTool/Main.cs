using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rack;
using System.Threading;
using ACS.SPiiPlusNET;
using System.IO.Ports;
using System.Collections;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Data;
using System.IO;

namespace RackTool
{
    public partial class Main : Form
    {
        #region Define
        private bool testLoop = true;
        private readonly CqcRack _rack = new CqcRack("192.168.8.18"); //"192.168.8.18"
        private TeachPos _selectedTargetPosition;
        private RackGripper _selectedGripper;
        private Thread _uiUpdateThread;
        private ArrayList _portName;
        private Power _power = Power.None;
        private bool _isStart = false;
        #endregion

        #region Struct
        public Main()
        {
            InitializeComponent();
            //Todo set tab draw mode to ownerdraw
            tabControl1.DrawItem += new DrawItemEventHandler(tabControl1_DrawItem);
            tabControl1.SelectedIndex = 6;
            comboBoxPower.SelectedIndex = 0;

            _rack.ErrorOccured += OnErrorOccured;
            _rack.WarningOccured += OnWarningOccured;
            _rack.InfoOccured += OnInfoOccured;
        }
        #endregion

        #region EventMethod
        private void OnInfoOccured(object sender, int code, string description)
        {
            try
            {

                NewLog.Instance.Info(code.ToString(), description);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void OnWarningOccured(object sender, int code, string description)
        {
            try
            {
                NewLog.Instance.Warn(code.ToString(), description);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void OnErrorOccured(object sender, int code, string description)
        {
            try
            {
                NewLog.Instance.Error(code.ToString() + " " + description);
                richTextBoxMessage.Text = code.ToString() + " " + description;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
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

        #endregion

        #region Main
        private void trackBarSetSpeed1_Scroll(object sender, EventArgs e)
        {
            _rack.SetRobotSpeedImm(Convert.ToDouble(trackBarSetSpeed1.Value));
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            testLoop = checkBoxIsLoop.Enabled;
            buttonTest.Enabled = true;
        }
        private async void button_Start_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;
            await Task.Run(() =>
            {
                try
                {
                    _rack.Start();
                    _isStart = true;


                    if (_uiUpdateThread == null)
                    {
                        _uiUpdateThread = new Thread(PositionAndSpeedUpdate)
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
        private async void buttonAutoRun_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    while (checkBoxAutoRun.Checked == true)
                    {
                        Stopwatch watch = new Stopwatch();
                        _rack.SetRobotSpeed(500);
                        //_rack.HomeRobot();
                        _rack.ReadyThePhone();
                        _rack.OpenGripper(RackGripper.One);
                        _rack.OpenGripper(RackGripper.Two);
                        watch.Start();
                        _rack.Pick(RackGripper.One);
                        //Thread.Sleep(200);
                        TargetPosition target = _rack.ConverterTeachPosToTargetPosition(TeachPos.ShieldBox1);
                        _rack.UnloadAndLoad(target, RackGripper.Two);
                        //Thread.Sleep(200);
                        _rack.Place(RackGripper.Two);
                    }

                    //MessageBox.Show(watch.ElapsedMilliseconds.ToString());
                });

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void Invoke(Control control, MethodInvoker action)
        {
            control.Invoke(action);
        }

        private void SetupForTeaching()
        {

            comboBoxGripper.Items.Clear();
            foreach (var pos in Enum.GetValues(typeof(RackGripper)))
            {
                comboBoxGripper.Items.Add(pos);
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
                    MessageBox.Show("Home Succeed!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
            buttonHome.Enabled = true;
        }
        #endregion

        #region Robot
        #region Enable
        private void button22_Click(object sender, EventArgs e)
        {
            if (_rack.Steppers.GetStatus(RackGripper.One, StatusCode.Enabled))
            {
                _rack.Steppers.Disable(RackGripper.One);
            }
            else
            {
                _rack.Steppers.Enable(RackGripper.One);
            }
            RefleshRobotUi();
        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (_rack.Steppers.GetStatus(RackGripper.Two, StatusCode.Enabled))
            {
                _rack.Steppers.Disable(RackGripper.Two);
            }
            else
            {
                _rack.Steppers.Enable(RackGripper.Two);
            }
            RefleshRobotUi();
        }

        private void buttonEableX1_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonEableX1.Text == "Enable")
                {
                    _rack.Motion.Enable(_rack.Motion.MotorX1);
                }
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
        private void trackBarSetSpeed2_Scroll(object sender, EventArgs e)
        {
            _rack.SetRobotSpeedImm(Convert.ToDouble(trackBarSetSpeed2.Value));
        }
        private void buttonG1TightOrLoose_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonG1TightOrLoose.Text == "G1Open")
                {
                    _rack.OpenGripper(RackGripper.One);
                    buttonG1TightOrLoose.Text = "G1Close";
                }
                else
                {
                    _rack.CloseGripper(RackGripper.One);
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
                    _rack.OpenGripper(RackGripper.Two);
                    buttonG2TightOrLoose.Text = "G2Close";
                }
                else
                {
                    _rack.CloseGripper(RackGripper.Two);
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
            _rack.SetRobotSpeed(defaultTestSpeed);

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
            _rack.SetRobotSpeed(defaultTestSpeed);

            await Task.Run(() =>
            {
                try
                {
                    DialogResult Result = MessageBox.Show("Now run the \"Unload And Load\", Please make sure the Unload Gripper not clamping the phone");
                    if (Result == DialogResult.No)
                        return;
                    TargetPosition target = _rack.ConverterTeachPosToTargetPosition(_selectedTargetPosition);
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
            _rack.SetRobotSpeed(defaultTestSpeed);

            await Task.Run(() =>
            {
                try
                {
                    _rack.LoadForTeaching(_selectedGripper, _selectedTargetPosition);
                    RefleshRobotUi();
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
                _rack.CalculateG1ToG2Offset(_selectedTargetPosition);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private async void buttonLoad_Click(object sender, EventArgs e)
        {
            _rack.SetRobotSpeed(defaultTestSpeed);

            await Task.Run((Action)(() =>
            {
                //Todo complete condition.
                TargetPosition target = _rack.ConverterTeachPosToTargetPosition(_selectedTargetPosition);

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
            _selectedGripper = (RackGripper)comboBoxGripper.SelectedItem;
        }

        private void comboBoxMovePos_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedTargetPosition = (TeachPos)comboBoxMovePos.SelectedItem;
        }

        private double defaultTestSpeed = 5;

        private async void buttonPick_Click(object sender, EventArgs e)
        {
            _rack.SetRobotSpeed(defaultTestSpeed);

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
            _rack.SetRobotSpeed(defaultTestSpeed);

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
            _rack.SetRobotSpeed(defaultTestSpeed);

            await Task.Run((Action)(() =>
            {
                //Todo complete condition.
                TargetPosition target = _rack.ConverterTeachPosToTargetPosition(_selectedTargetPosition);

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
            XmlReaderWriter.SetConveyorSetting(RackSetting.ConveyorMovingForward, _rack.Conveyor.ConveyorMovingForward.ToString());
            //bool b = Convert.ToBoolean(XmlReaderWriter.GetConveyorSetting(RackSetting.ConveyorMovingForward));
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

        private void trackBarSetSpeed2_ValueChanged(object sender, EventArgs e)
        {
            labelSpeed2.Text = trackBarSetSpeed2.Value.ToString();
            
        }

        private void trackBarSetSpeed1_ValueChanged(object sender, EventArgs e)
        {
            labelSpeed1.Text = trackBarSetSpeed1.Value.ToString();
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
                buttonEableG1.Text = _rack.Steppers.GetStatus(RackGripper.One, StatusCode.Enabled) ? "Disable" : "Enable";
                buttonEableG2.Text = _rack.Steppers.GetStatus(RackGripper.Two, StatusCode.Enabled) ? "Disable" : "Enable";
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

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
        private void PositionAndSpeedUpdate()
        {
            while (true)
            {
                try
                {
                    labelPositionG1.Invoke((MethodInvoker)(() => { labelPositionG1.Text = _rack.Steppers.GetPosition(RackGripper.One).ToString("F2"); }));
                    labelPositionG2.Invoke((MethodInvoker)(() => { labelPositionG2.Text = _rack.Steppers.GetPosition(RackGripper.Two).ToString("F2"); }));
                    labelPositionX.Invoke((MethodInvoker)(() => { labelPositionX.Text = _rack.Motion.GetPositionX().ToString("F2"); }));
                    labelPositionY.Invoke((MethodInvoker)(() => { labelPositionY.Text = _rack.Motion.GetPosition(_rack.Motion.MotorY).ToString("F2"); }));
                    labelPositionZ.Invoke((MethodInvoker)(() => { labelPositionZ.Text = _rack.Motion.GetPosition(_rack.Motion.MotorZ).ToString("F2"); }));
                    labelPositionR.Invoke((MethodInvoker)(() => { labelPositionR.Text = _rack.Motion.GetPosition(_rack.Motion.MotorR).ToString("F2"); }));
                    trackBarSetSpeed1.Value = (int)(_rack.Motion.GetVelocity(_rack.Motion.MotorZ)/_rack.Motion.MotorZ.SpeedFactor);
                    trackBarSetSpeed2.Value = trackBarSetSpeed1.Value;
                    Thread.Sleep(500);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
            }
        }
        #region Manual control

        private void buttonEnableAll_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Motion.EnableAll();
                _rack.Steppers.Enable(RackGripper.One);
                _rack.Steppers.Enable(RackGripper.Two);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonX1MoveTo_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Motion.ToPoint(_rack.Motion.MotorX1, Convert.ToDouble(textBoxDistanceX1.Text));
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonX2MoveTo_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Motion.ToPoint(_rack.Motion.MotorX2, Convert.ToDouble(textBoxDistanceX2.Text));
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonYMoveTo_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Motion.ToPoint(_rack.Motion.MotorY, Convert.ToDouble(textBoxDistanceY.Text));
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonRMoveTo_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Motion.ToPoint(_rack.Motion.MotorR, Convert.ToDouble(textBoxDistanceR.Text));
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonZMoveTo_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Motion.ToPoint(_rack.Motion.MotorZ, Convert.ToDouble(textBoxDistanceZ.Text));
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

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
                _rack.Steppers.ToPoint(RackGripper.One, Convert.ToDouble(textBoxDistanceG1.Text));
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
                _rack.Steppers.ToPoint(RackGripper.Two, Convert.ToDouble(textBoxDistanceG2.Text));
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

        #region SpeedSwitch
        private void buttonLowSpeed_Click(object sender, EventArgs e)
        {
            _rack.SetRobotSpeedImm(1);
        }

        private void buttonMiddleSpeed_Click(object sender, EventArgs e)
        {
            _rack.SetRobotSpeedImm(100);
        }
        private void buttonHighSpeed_Click(object sender, EventArgs e)
        {
            _rack.SetRobotSpeedImm(200);
        }
        #endregion
        #endregion 
        #endregion

        #region ShieldBoxOperate

        #region Right-click menu Event
        private void bTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wIFIToolStripMenuItem.Checked = false;
            rFToolStripMenuItem.Checked = false;
            bTToolStripMenuItem.Checked = true;
            if (listViewBox.SelectedItems.Count != 0)
            {
                switch (listViewBox.SelectedItems[0].Text)
                {
                    case "Box1": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.Type, ShieldBoxType.BT.ToString()); break;
                    case "Box2": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.Type, ShieldBoxType.BT.ToString()); break;
                    case "Box3": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.Type, ShieldBoxType.BT.ToString()); break;
                    case "Box4": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.Type, ShieldBoxType.BT.ToString()); break;
                    case "Box5": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.Type, ShieldBoxType.BT.ToString()); break;
                    case "Box6": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.Type, ShieldBoxType.BT.ToString()); break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Please Select Box!");
            }

        }

        private void wIFIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wIFIToolStripMenuItem.Checked = true;
            rFToolStripMenuItem.Checked = false;
            bTToolStripMenuItem.Checked = false;
            if (listViewBox.SelectedItems.Count != 0)
            {
                switch (listViewBox.SelectedItems[0].Text)
                {
                    case "Box1": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.Type, ShieldBoxType.WIFI.ToString()); break;
                    case "Box2": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.Type, ShieldBoxType.WIFI.ToString()); break;
                    case "Box3": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.Type, ShieldBoxType.WIFI.ToString()); break;
                    case "Box4": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.Type, ShieldBoxType.WIFI.ToString()); break;
                    case "Box5": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.Type, ShieldBoxType.WIFI.ToString()); break;
                    case "Box6": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.Type, ShieldBoxType.WIFI.ToString()); break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Please Select Box!");
            }
        }

        private void rFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wIFIToolStripMenuItem.Checked = false;
            rFToolStripMenuItem.Checked = true;
            bTToolStripMenuItem.Checked = false;
            if (listViewBox.SelectedItems.Count != 0)
            {
                switch (listViewBox.SelectedItems[0].Text)
                {
                    case "Box1": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.Type, ShieldBoxType.RF.ToString()); break;
                    case "Box2": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.Type, ShieldBoxType.RF.ToString()); break;
                    case "Box3": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.Type, ShieldBoxType.RF.ToString()); break;
                    case "Box4": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.Type, ShieldBoxType.RF.ToString()); break;
                    case "Box5": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.Type, ShieldBoxType.RF.ToString()); break;
                    case "Box6": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.Type, ShieldBoxType.RF.ToString()); break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Please Select Box!");
            }
        }
        private void labelTypeToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            if (listViewBox.SelectedItems.Count != 0)
            {
                ToolStripMenuItem[] Items = { aToolStripMenuItem, bToolStripMenuItem, cToolStripMenuItem };
                switch (listViewBox.SelectedItems[0].Text)
                {
                    case "Box1": Check(Items, 1, ShieldBoxItem.Label); break;
                    case "Box2": Check(Items, 2, ShieldBoxItem.Label); break;
                    case "Box3": Check(Items, 3, ShieldBoxItem.Label); break;
                    case "Box4": Check(Items, 4, ShieldBoxItem.Label); break;
                    case "Box5": Check(Items, 5, ShieldBoxItem.Label); break;
                    case "Box6": Check(Items, 6, ShieldBoxItem.Label); break;
                    default:
                        break;
                }
            }
        }
        private void typeToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            if (listViewBox.SelectedItems.Count != 0)
            {
                ToolStripMenuItem[] Items = { bTToolStripMenuItem, wIFIToolStripMenuItem, rFToolStripMenuItem };
                switch (listViewBox.SelectedItems[0].Text)
                {
                    case "Box1": Check(Items, 1, ShieldBoxItem.Type); break;
                    case "Box2": Check(Items, 2, ShieldBoxItem.Type); break;
                    case "Box3": Check(Items, 3, ShieldBoxItem.Type); break;
                    case "Box4": Check(Items, 4, ShieldBoxItem.Type); break;
                    case "Box5": Check(Items, 5, ShieldBoxItem.Type); break;
                    case "Box6": Check(Items, 6, ShieldBoxItem.Type); break;
                    default:
                        break;
                }
            }
        }

        private void Check(ToolStripMenuItem[] items, int boxId, ShieldBoxItem shieldBoxItem)
        {
            string Result = XmlReaderWriter.GetBoxAttribute(Files.BoxData, boxId, shieldBoxItem);
            switch (Result)
            {
                case "BT":
                    items[0].Checked = true;
                    items[1].Checked = false;
                    items[2].Checked = false;
                    break;
                case "WIFI":
                    items[0].Checked = false;
                    items[1].Checked = true;
                    items[2].Checked = false;
                    break;
                case "RF":
                    items[0].Checked = false;
                    items[1].Checked = false;
                    items[2].Checked = true;
                    break;
                case "A":
                    items[0].Checked = true;
                    items[1].Checked = false;
                    items[2].Checked = false;
                    break;
                case "B":
                    items[0].Checked = false;
                    items[1].Checked = true;
                    items[2].Checked = false;
                    break;
                case "C":
                    items[0].Checked = false;
                    items[1].Checked = false;
                    items[2].Checked = true;
                    break;
                default:
                    break;
            }
        }

        private void aToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aToolStripMenuItem.Checked = true;
            bTToolStripMenuItem.Checked = false;
            cToolStripMenuItem.Checked = false;
            if (listViewBox.SelectedItems.Count != 0)
            {
                switch (listViewBox.SelectedItems[0].Text)
                {
                    case "Box1": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.Label, LabelType.A.ToString()); break;
                    case "Box2": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.Label, LabelType.A.ToString()); break;
                    case "Box3": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.Label, LabelType.A.ToString()); break;
                    case "Box4": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.Label, LabelType.A.ToString()); break;
                    case "Box5": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.Label, LabelType.A.ToString()); break;
                    case "Box6": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.Label, LabelType.A.ToString()); break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Please Select Box!");
            }
        }

        private void bToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aToolStripMenuItem.Checked = false;
            bTToolStripMenuItem.Checked = true;
            cToolStripMenuItem.Checked = false;
            if (listViewBox.SelectedItems.Count != 0)
            {
                switch (listViewBox.SelectedItems[0].Text)
                {
                    case "Box1": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.Label, LabelType.B.ToString()); break;
                    case "Box2": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.Label, LabelType.B.ToString()); break;
                    case "Box3": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.Label, LabelType.B.ToString()); break;
                    case "Box4": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.Label, LabelType.B.ToString()); break;
                    case "Box5": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.Label, LabelType.B.ToString()); break;
                    case "Box6": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.Label, LabelType.B.ToString()); break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Please Select Box!");
            }

        }

        private void cToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aToolStripMenuItem.Checked = false;
            bTToolStripMenuItem.Checked = false;
            cToolStripMenuItem.Checked = true;
            if (listViewBox.SelectedItems.Count != 0)
            {
                switch (listViewBox.SelectedItems[0].Text)
                {
                    case "Box1": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.Label, LabelType.C.ToString()); break;
                    case "Box2": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.Label, LabelType.C.ToString()); break;
                    case "Box3": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.Label, LabelType.C.ToString()); break;
                    case "Box4": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.Label, LabelType.C.ToString()); break;
                    case "Box5": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.Label, LabelType.C.ToString()); break;
                    case "Box6": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.Label, LabelType.C.ToString()); break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Please Select Box!");
            }
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewBox.SelectedItems.Count != 0)
            {
                listViewBox.SelectedItems[0].ImageIndex = 0;
                switch (listViewBox.SelectedItems[0].Text)
                {
                    case "Box1": _rack.ShieldBox1.OpenBox(); break;
                    case "Box2": _rack.ShieldBox2.OpenBox(); break;
                    case "Box3": _rack.ShieldBox3.OpenBox(); break;
                    case "Box4": _rack.ShieldBox4.OpenBox(); break;
                    case "Box5": _rack.ShieldBox5.OpenBox(); break;
                    case "Box6": _rack.ShieldBox6.OpenBox(); break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Please Select Box!");
            }
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewBox.SelectedItems.Count != 0)
            {
                listViewBox.SelectedItems[0].ImageIndex = 0;
                switch (listViewBox.SelectedItems[0].Text)
                {
                    case "Box1": _rack.ShieldBox1.CloseBox(); break;
                    case "Box2": _rack.ShieldBox2.CloseBox(); break;
                    case "Box3": _rack.ShieldBox3.CloseBox(); break;
                    case "Box4": _rack.ShieldBox4.CloseBox(); break;
                    case "Box5": _rack.ShieldBox5.CloseBox(); break;
                    case "Box6": _rack.ShieldBox6.CloseBox(); break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Please Select Box!");
            }
        }
        private void EnableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int BoxId = 0;
            if (listViewBox.SelectedItems.Count != 0)
            {
                listViewBox.SelectedItems[0].ImageIndex = 0;
                switch (listViewBox.SelectedItems[0].Text)
                {
                    case "Box1": BoxId = 1; break;
                    case "Box2": BoxId = 2; break;
                    case "Box3": BoxId = 3; break;
                    case "Box4": BoxId = 4; break;
                    case "Box5": BoxId = 5; break;
                    case "Box6": BoxId = 6; break;
                    default:
                        break;
                }
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, BoxId, ShieldBoxItem.State, "Enable");
                RefleshBoxUi();
            }
            else
            {
                MessageBox.Show("Please Select Box!");
            }
        }

        private void DisableToolStripMenuItem_Click(object sender, EventArgs e)
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
                        case "Box1": BoxId = 1; Com = comboBox1.Text; comboBox1.Text = "None"; break;
                        case "Box2": BoxId = 2; Com = comboBox2.Text; comboBox2.Text = "None"; break;
                        case "Box3": BoxId = 3; Com = comboBox3.Text; comboBox3.Text = "None"; break;
                        case "Box4": BoxId = 4; Com = comboBox4.Text; comboBox4.Text = "None"; break;
                        case "Box5": BoxId = 5; Com = comboBox5.Text; comboBox5.Text = "None"; break;
                        case "Box6": BoxId = 6; Com = comboBox6.Text; comboBox6.Text = "None"; break;
                        default:
                            break;
                    }
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, BoxId, ShieldBoxItem.State, "Disable");
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, BoxId, ShieldBoxItem.COM, "None");
                    if (_portName.Contains(Com) == false)
                        _portName.Add(Com);
                    RefleshBoxUi();
                }
                else
                {
                    MessageBox.Show("Please Select Box!");
                }
            }
        }
        #endregion

        private void ClearItem()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();
            comboBox4.Items.Clear();
            comboBox5.Items.Clear();
            comboBox6.Items.Clear();
        }

        private void RefleshBoxUi()
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

        #region ButtonOk
        private void buttonOK1_Click(object sender, EventArgs e)
        {
            try
            {
                string Com = comboBox1.SelectedItem.ToString();
                _portName.Remove(Com);
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.COM, Com);
                RefleshBoxUi();
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
                string Com = comboBox2.SelectedItem.ToString();
                _portName.Remove(Com);
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.COM, Com);
                RefleshBoxUi();
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
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.COM, Com);
                RefleshBoxUi();
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
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.COM, Com);
                RefleshBoxUi();
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
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.COM, Com);
                RefleshBoxUi();
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
                XmlReaderWriter.SetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.COM, Com);
                RefleshBoxUi();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        #endregion
        #endregion

        #region ConveyorCylinderOperate
        private void buttonUpBlockSeparateForward_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonUpBlockSeparateForward.Text == "Down")
                    _rack.Conveyor.SetCylinder(Output.UpBlockSeparateForward, Input.UpBlockSeparateForward, false);
                else
                    _rack.Conveyor.ResetCylinder(Output.UpBlockSeparateForward, Input.UpBlockSeparateForward);
                RefleshConveyorUi();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void buttonClampOrLoose_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonOpenOrClose.Text == "Close")

                {
                    _rack.Conveyor.Clamp(true);
                    buttonOpenOrClose.Text = "Open";
                }
                else
                {
                    _rack.Conveyor.Clamp(false);
                    buttonOpenOrClose.Text = "Close";
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonSideBlockPick_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonSideBlockPick.Text == "Stretch")
                    _rack.Conveyor.PushPickInpos(true);
                else
                    _rack.Conveyor.PushPickInpos(false);
                RefleshConveyorUi();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonSideBlockSeparateForward_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonSideBlockSeparateForward.Text == "Stretch")
                    _rack.Conveyor.SetCylinder(Output.SideBlockSeparateForward, Input.SideBlockSeparateForward, false);
                else
                    _rack.Conveyor.ResetCylinder(Output.SideBlockSeparateForward, Input.SideBlockSeparateForward);
                RefleshConveyorUi();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonSideBlockSeparateBackward_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonSideBlockSeparateBackward.Text == "Stretch")
                    _rack.Conveyor.SetCylinder(Output.SideBlockSeparateBackward, Input.SideBlockSeparateBackward, false);
                else
                    _rack.Conveyor.ResetCylinder(Output.SideBlockSeparateBackward, Input.SideBlockSeparateBackward);
                RefleshConveyorUi();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonUpBlockPickBackward_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonUpBlockPickBackward.Text == "Down")
                    _rack.Conveyor.SetCylinder(Output.UpBlockPickBackward, Input.UpBlockPickBackward, false);
                else
                    _rack.Conveyor.ResetCylinder(Output.UpBlockPickBackward, Input.UpBlockPickBackward);

                RefleshConveyorUi();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonUpBlockPickForward_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonUpBlockPickForward.Text == "Down")
                    _rack.Conveyor.SetCylinder(Output.UpBlockPickForward, Input.UpBlockPickForward, false);
                else
                    _rack.Conveyor.ResetCylinder(Output.UpBlockPickForward, Input.UpBlockPickForward);
                RefleshConveyorUi();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonUpBlockSeparateBackward_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonUpBlockSeparateBackward.Text == "Down")
                    _rack.Conveyor.SetCylinder(Output.UpBlockSeparateBackward, Input.UpBlockSeparateBackward, false);
                else
                    _rack.Conveyor.ResetCylinder(Output.UpBlockSeparateBackward, Input.UpBlockSeparateBackward);
                RefleshConveyorUi();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Setting
        #region Login
        
        #region Encrypt And Decryption
        private String Encrypt(string information)
        {
            try
            {
                string EncryptKey = "Test";
                byte[] Data = Encoding.UTF8.GetBytes(information);
                using (MD5CryptoServiceProvider Md5 = new MD5CryptoServiceProvider())
                {
                    byte[] Keys = Md5.ComputeHash(Encoding.UTF8.GetBytes(EncryptKey));
                    using (TripleDESCryptoServiceProvider Triple = new TripleDESCryptoServiceProvider()
                    {
                        Key = Keys,
                        Mode = CipherMode.ECB,
                        Padding = PaddingMode.PKCS7
                    })
                    {
                        ICryptoTransform TranForm = Triple.CreateEncryptor();
                        byte[] Result = TranForm.TransformFinalBlock(Data, 0, Data.Length);
                        return Convert.ToBase64String(Result, 0, Result.Length);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private String Decryption(string EncryptStr)
        {
            try
            {
                string EncryptKey = "Test";
                byte[] Data = Convert.FromBase64String(EncryptStr);
                using (MD5CryptoServiceProvider Md5 = new MD5CryptoServiceProvider())
                {
                    byte[] Keys = Md5.ComputeHash(Encoding.UTF8.GetBytes(EncryptKey));
                    using (TripleDESCryptoServiceProvider Triple = new TripleDESCryptoServiceProvider()
                    {
                        Key = Keys,
                        Mode = CipherMode.ECB,
                        Padding = PaddingMode.PKCS7
                    })
                    {
                        ICryptoTransform TranForm = Triple.CreateDecryptor();
                        byte[] Result = TranForm.TransformFinalBlock(Data, 0, Data.Length);
                        return Encoding.UTF8.GetString(Result);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
        private void buttonLogin_Click_1(object sender, EventArgs e)
        {
            try
            {
                string LoginName = string.Empty;
                string Password = string.Empty;
                if (textBoxLoginName.Text == string.Empty)
                {
                    throw new Exception("Please Input Login Name!");
                }
                if (textBoxPassWord.Text == string.Empty)
                {
                    throw new Exception("Please Input Password!");
                }
                if (comboBoxPower.Text == string.Empty)
                {
                    throw new Exception("Please Select Power!");
                }
                switch (comboBoxPower.SelectedIndex)
                {
                    case 0:
                        LoginName = XmlReaderWriter.GetLoginAttribute(Files.LoginData, LoginType.Administrator, LogicInformation.LoginName);
                        Password = XmlReaderWriter.GetLoginAttribute(Files.LoginData, LoginType.Administrator, LogicInformation.LoginPassWord);
                        if (textBoxLoginName.Text != LoginName)
                            throw new Exception("LoginName is error,Please check!");
                        if (textBoxPassWord.Text != Decryption(Password))
                            throw new Exception("PassWord is error,Please check!");
                        _power = Power.Administrator;
                        buttonChangePassword.Enabled = true;
                        break;
                    case 1:
                        LoginName = XmlReaderWriter.GetLoginAttribute(Files.LoginData, LoginType.Technician, LogicInformation.LoginName);
                        Password = XmlReaderWriter.GetLoginAttribute(Files.LoginData, LoginType.Technician, LogicInformation.LoginPassWord);
                        if (textBoxLoginName.Text != LoginName)
                            throw new Exception("LoginName is error,Please check!");
                        if (textBoxPassWord.Text != Decryption(Password))
                            throw new Exception("PassWord is error,Please check!");
                        _power = Power.Technician;
                        buttonChangePassword.Enabled = false;
                        break;
                    case 2:
                        LoginName = XmlReaderWriter.GetLoginAttribute(Files.LoginData, LoginType.Operator, LogicInformation.LoginName);
                        Password = XmlReaderWriter.GetLoginAttribute(Files.LoginData, LoginType.Operator, LogicInformation.LoginPassWord);
                        if (textBoxLoginName.Text != LoginName)
                            throw new Exception("LoginName is error,Please check!");
                        if (textBoxPassWord.Text != Decryption(Password))
                            throw new Exception("PassWord is error,Please check!");
                        _power = Power.Operator;
                        buttonChangePassword.Enabled = false;
                        break;
                    default:
                        throw new Exception("Please do not input power ");
                        //Enable controls of disable controls in here.

                }
                ///Enable some or disable some
                switch (_power)
                {
                    case Power.Administrator:
                        buttonSaveApproach.Enabled = true;
                        buttonCalOffset.Enabled = true;
                        buttonCreateXml.Enabled = true;
                        buttonBoxSave.Enabled = true;
                        buttonCreateFile.Enabled = true;
                        labelTypeToolStripMenuItem.Enabled = true;
                        typeToolStripMenuItem.Enabled = true;
                        break;
                    case Power.Technician:
                        buttonSaveApproach.Enabled = false;
                        buttonCalOffset.Enabled = false;
                        buttonCreateXml.Enabled = false;
                        buttonBoxSave.Enabled = false;
                        buttonCreateFile.Enabled = false;
                        labelTypeToolStripMenuItem.Enabled = false;
                        typeToolStripMenuItem.Enabled = false;
                        break;
                    case Power.Operator:
                        buttonSaveApproach.Enabled = false;
                        buttonCalOffset.Enabled = false;
                        buttonCreateXml.Enabled = false;
                        buttonBoxSave.Enabled = false;
                        buttonCreateFile.Enabled = false;
                        labelTypeToolStripMenuItem.Enabled = false;
                        typeToolStripMenuItem.Enabled = false;
                        break;
                    default:
                        break;
                }
                toolStripStatusLabelName.Text = LoginName;
                toolStripStatusLabelPower.Text = _power.ToString();
                tabControl1.SelectedIndex = 0;
                MessageBox.Show("Login Succeed!");

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonChangePassword_Click(object sender, EventArgs e)
        {
            groupBoxChangePassword.Visible = true;
            textBoxChangeLoginName.Text = string.Empty;
            textBoxOldPassword.Text = string.Empty;
            textBoxNewPassword.Text = string.Empty;
        }

        private void buttonComfirm_Click(object sender, EventArgs e)
        {
            try
            {
                string LoginName = string.Empty;
                string Password = string.Empty;
                if (textBoxChangeLoginName.Text == string.Empty)
                {
                    throw new Exception("Please Input Login Name!");
                }
                if (textBoxOldPassword.Text == string.Empty)
                {
                    throw new Exception("Please Input Old Password!");
                }
                if (comboBoxChangePower.Text == string.Empty)
                {
                    throw new Exception("Please Select Power!");
                }
                switch (comboBoxChangePower.SelectedIndex)
                {
                    case 0:
                        LoginName = XmlReaderWriter.GetLoginAttribute(Files.LoginData, LoginType.Administrator, LogicInformation.LoginName);
                        Password = XmlReaderWriter.GetLoginAttribute(Files.LoginData, LoginType.Administrator, LogicInformation.LoginPassWord);
                        if (textBoxChangeLoginName.Text != LoginName)
                            throw new Exception("LoginName is error,Please check!");
                        if (textBoxOldPassword.Text != Decryption(Password))
                            throw new Exception("Old PassWord is error,Please check!");
                        XmlReaderWriter.SetLoginAttribute(Files.LoginData, LoginType.Administrator, LogicInformation.LoginPassWord, Encrypt(textBoxNewPassword.Text));
                        MessageBox.Show("Change Succeed!");
                        break;
                    case 1:
                        LoginName = XmlReaderWriter.GetLoginAttribute(Files.LoginData, LoginType.Technician, LogicInformation.LoginName);
                        Password = XmlReaderWriter.GetLoginAttribute(Files.LoginData, LoginType.Technician, LogicInformation.LoginPassWord);
                        if (textBoxChangeLoginName.Text != LoginName)
                            throw new Exception("LoginName is error,Please check!");
                        if (textBoxOldPassword.Text != Decryption(Password))
                            throw new Exception("Old PassWord is error,Please check!");
                        XmlReaderWriter.SetLoginAttribute(Files.LoginData, LoginType.Technician, LogicInformation.LoginPassWord, Encrypt(textBoxNewPassword.Text));
                        MessageBox.Show("Change Succeed!");
                        break;
                    case 2:
                        LoginName = XmlReaderWriter.GetLoginAttribute(Files.LoginData, LoginType.Operator, LogicInformation.LoginName);
                        Password = XmlReaderWriter.GetLoginAttribute(Files.LoginData, LoginType.Operator, LogicInformation.LoginPassWord);
                        if (textBoxChangeLoginName.Text != LoginName)
                            throw new Exception("LoginName is error,Please check!");
                        if (textBoxOldPassword.Text != Decryption(Password))
                            throw new Exception("Old PassWord is error,Please check!");
                        XmlReaderWriter.SetLoginAttribute(Files.LoginData, LoginType.Operator, LogicInformation.LoginPassWord, Encrypt(textBoxNewPassword.Text));
                        MessageBox.Show("Change Succeed!");
                        break;
                    default:
                        throw new Exception("Please do not input power ");
                }
                groupBoxChangePassword.Visible = false;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        #endregion 
        private void buttonBoxIpPortOk_Click(object sender, EventArgs e)
        {
            switch (comboBoxShieldBox.SelectedIndex)
            {
                case 0:
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.Ip, textBoxIp.Text);
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.Port, textBoxPort.Text);
                    break;
                case 1:
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.Ip, textBoxIp.Text);
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.Port, textBoxPort.Text);
                    break;
                case 2:
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.Ip, textBoxIp.Text);
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.Port, textBoxPort.Text);
                    break;
                case 3:
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.Ip, textBoxIp.Text);
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.Port, textBoxPort.Text);
                    break;
                case 4:
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.Ip, textBoxIp.Text);
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.Port, textBoxPort.Text);
                    break;
                case 5:
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.Ip, textBoxIp.Text);
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.Port, textBoxPort.Text);
                    break;
                default:
                    throw new Exception("Please select the shieldBox");
            }

        }

        private void buttonStepperOK_Click(object sender, EventArgs e)
        {
            string Com = comboBoxStepper.SelectedItem.ToString();
            _portName.Remove(Com);
            XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.COM, Com);
        }

        private void comboBoxStepper_Click(object sender, EventArgs e)
        {
            string[] PortName = SerialPort.GetPortNames();
            _portName = new ArrayList(PortName);
            comboBoxStepper.Items.Clear();
            foreach (var item in _portName)
            {
                comboBoxStepper.Items.Add(item);
            }
        }
        #endregion

        #region Tabcontrol page change event
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (toolStripStatusLabelPower.Text == "None")
                {
                    if (tabControl1.SelectedIndex != 6)
                    {
                        tabControl1.SelectedIndex = 6;
                        throw new Exception("Please Login in");
                    }
                }

                //if (_isStart == false && tabControl1.SelectedIndex != 6 && tabControl1.SelectedIndex != 0)
                //if (_isStart == false && tabControl1.SelectedIndex != 6 && tabControl1.SelectedIndex != 0)
                //{
                //    tabControl1.SelectedIndex = 0;
                //    throw new Exception("Please click \"Start\",or you can not click anywhere except \"Setting\"");
                //}


                switch (tabControl1.SelectedIndex)
                {
                    case 0: break;
                    case 1: RefleshRobotUi(); break;
                    case 2: RefleshConveyorUi(); break;
                    case 3:
                        string[] PortName = SerialPort.GetPortNames();
                        _portName = new ArrayList(PortName);
                        RefleshBoxUi();
                        break;
                    case 4: break;
                    case 5: break;
                    case 6:
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region Log
        private void buttonLogLoad_Click_1(object sender, EventArgs e)
        {
            try
            {
                string FileName = string.Empty;
                OpenFileDialog OpenFile = new OpenFileDialog()
                    { InitialDirectory = System.IO.Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().Location + @"\\Logs") };

                DialogResult Result = OpenFile.ShowDialog();
                if (Result == DialogResult.OK)
                    FileName = OpenFile.FileName;
                else
                    return;
                dataGridView1.Rows.Clear();
                StreamReader Reader = new StreamReader(FileName, Encoding.Default); ;
                while (!Reader.EndOfStream)
                {

                    string[] Items = Reader.ReadLine().Split(' ');
                    if (Items.Length != 5)
                    {
                        Reader.Close();
                        Reader.Dispose();
                        continue;
                    }

                    DataGridViewRow dataGridViewRow = new DataGridViewRow();
                    foreach (DataGridViewColumn Item in dataGridView1.Columns)
                    {
                        dataGridViewRow.Cells.Add(Item.CellTemplate.Clone() as DataGridViewCell);
                    }
                    dataGridViewRow.Cells[0].Value = Items[0] + Items[1];
                    dataGridViewRow.Cells[1].Value = Items[2];
                    dataGridViewRow.Cells[2].Value = Items[3];
                    dataGridViewRow.Cells[3].Value = Items[4]; ;
                    dataGridView1.Rows.Add(dataGridViewRow);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region Tester

        private void buttonTest_Click(object sender, EventArgs e)
        {
            _rack.SetRobotSpeed(defaultTestSpeed);

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
        private void buttonBoxSave_Click_1(object sender, EventArgs e)
        {

            try
            {
                XmlReaderWriter.CreateBoxDataFile(Files.BoxData);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonCreateXml_Click_1(object sender, EventArgs e)
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

        private void buttonCreateFile_Click(object sender, EventArgs e)
        {

            try
            {
                XmlReaderWriter.CreateLoginDataFile(Files.LoginData);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        } 
        #endregion

        
    }
}
