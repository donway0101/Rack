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
using System.IO;
using System.Collections.Generic;

namespace RackTool
{
    public partial class Main : Form
    {
        #region Define
        private readonly CqcRack _rack = new CqcRack("199.199.0.18");
        private TeachPos _selectedTargetPosition;
        private RackGripper _selectedGripper;
        private Thread _uiUpdateThread;
        private ArrayList _portName;
        private Power _power = Power.None;
        private bool _isStart = false;
        private bool _autoPass = false;
        private TabControlPage currentPage = TabControlPage.Setting;

        private List<CqcRackError> _errorsList = new List<CqcRackError>();
        private List<CqcRackError> _warningsList = new List<CqcRackError>();

        private string _msgBuffer = string.Empty;
        private string _errorText = string.Empty;
        private string _warningText = string.Empty;

        private bool _systemFault = true;
        private bool _systemWarning = true;

        private bool _keepMonitoringSystem = true;
        private bool _scrollRichTextBox = true;

        private SocketClient _client1 = new SocketClient(1001);
        private SocketClient _client2 = new SocketClient(1002);
        private SocketClient _client3 = new SocketClient(1003);
        private SocketClient _client4 = new SocketClient(1004);
        private SocketClient _client5 = new SocketClient(1005);
        private SocketClient _client6 = new SocketClient(1006);
        private SocketClient _selectedSocketClient = null;

        #endregion

        #region Struct
        public Main()
        {
            InitializeComponent();

            //Set tab draw mode to ownerdraw
            tabControlUi.DrawItem += new DrawItemEventHandler(tabControl1_DrawItem);

            tabControlUi.SelectedIndex = (int)TabControlPage.Main;
            comboBoxPower.SelectedIndex = 0;

            _rack.ErrorOccured += OnErrorOccured;
            _rack.WarningOccured += OnWarningOccured;
            _rack.InfoOccured += OnInfoOccured;
            _rack.ProductionComplete += _rack_ProductionComplete;
        }

        private void _rack_ProductionComplete(object sender, bool pass, string sn, string footprint, string description)
        {
            if (pass)
            {
                NewLog.Instance.Pass(sn, footprint);
            }
            else
            {
                NewLog.Instance.Ng(sn, footprint, description);
            }
        }
        #endregion

        #region EventMethod
        private void OnInfoOccured(object sender, int code, string description)
        {
            try
            {
                NewLog.Instance.Info(code.ToString() + " " + description);
                AddMessageToTextBox(code, description);

                if (code == 20032)
                {
                    Task.Run(() => { MessageBox.Show(description); });
                }
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
                NewLog.Instance.Warn(code.ToString() + " " + description);
                AddMessageToTextBox(code, description);
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
                if (code == (int)Error.OpenBoxFail)
                {
                    //Todo add to history error list, if reset, then the list is cleared.
                }

                if (code == 40007)
                {
                    ShowError(description);
                }

                NewLog.Instance.Error(code.ToString() + " " + description);
                AddMessageToTextBox(code, description);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ShowWarning(string description)
        {
            Task.Run(() => { MessageBox.Show(description, "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Warning); });
        }

        private void ShowError(string description)
        {
            Task.Run(() => { MessageBox.Show(description, "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Error); });
        }

        private void AddMessageToTextBox(int code, string description)
        {
            richTextBoxMessage.BeginInvoke((MethodInvoker)(() =>
            {
                string newMsg = DateTime.Now.ToString("HH:mm:ss ") + code.ToString() + " " + description + Environment.NewLine;
                
                if (_scrollRichTextBox)
                {
                    richTextBoxMessage.AppendText(_msgBuffer);
                    _msgBuffer = string.Empty;
                    richTextBoxMessage.AppendText(newMsg);
                    //richTextBoxMessage.SelectionStart = richTextBoxMessage.Text.Length;
                    //richTextBoxMessage.ScrollToCaret();
                }
                else
                {
                    _msgBuffer += newMsg;
                }
                
            }));
        }

        private void tabControl1_DrawItem(Object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush;

            // Get the item from the collection.
            TabPage _tabPage = tabControlUi.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            Rectangle _tabBounds = tabControlUi.GetTabRect(e.Index);

            if (e.State == DrawItemState.Selected)
            {

                // Draw a different background color, and don't paint a focus rectangle.
                _textBrush = new SolidBrush(Color.DarkRed);
                g.FillRectangle(Brushes.LightGray, e.Bounds);
            }
            else
            {
                _textBrush = new System.Drawing.SolidBrush(e.ForeColor);
                e.DrawBackground();
            }

            // Use our own font.
            Font _tabFont = new Font("Arial", 14.0f, FontStyle.Bold, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Center;
            _stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));
        }

        #endregion

        #region Main

        private async void button_Start_Click(object sender, EventArgs e)
        {
            bool startSuccessful = false;
            buttonStart.Enabled = false;
            await Task.Run(() =>
            {
                try
                {
                    OnInfoOccured(this, 20006, "System is starting.");
                    _rack.Start();
                    _isStart = true;

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

                    startSuccessful = true;
                }
                catch (Exception ex)
                {
                    OnErrorOccured(this, 40013, "System start fail due to:" + ex.Message);
                    MessageBox.Show(ex.Message);
                }
            });

            if (startSuccessful)
            {                
                SetupUiForTeaching();
                OnInfoOccured(this, 20005, "System start OK.");
            }

            if (_rack.ShieldBox3 != null)
            {
                if (_rack.ShieldBox3.Enabled == false)
                {
                    MessageBox.Show("如果3号位有屏蔽箱，请确保屏蔽箱门已经关闭，以免在机器手放NG料时产生干涉。");
                }
            }

            richTextBoxMessage.SelectionStart = richTextBoxMessage.Text.Length;
            richTextBoxMessage.ScrollToCaret();
            groupBoxMain.Enabled = startSuccessful;
            buttonStart.Enabled = true;
        }

        private void ShowException(Exception e)
        {
            richTextBoxMessage.BeginInvoke((MethodInvoker)(
                () => { richTextBoxMessage.Text = e.Message; }
                ));
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

        private void SetupUiForTeaching()
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
                    OnInfoOccured(this, 20007, "Homing robot.");
                    _rack.HomeRobot(_rack.HomeSpeed);
                    OnInfoOccured(this, 20008, "Home robot complete.");
                    MessageBox.Show("Home Succeed.");
                }
                catch (Exception ex)
                {
                    OnInfoOccured(this, 40014, "Home robot failed due to:" + ex.Message);
                    MessageBox.Show(ex.Message);
                }
            });
            buttonHome.Enabled = true;
        }
        private void buttonStartPhoneServer_Click(object sender, EventArgs e)
        {
            _rack.StartPhoneServer();
            buttonHome.Enabled = false;
        }

        private async void buttonCheckBox_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.Enabled = false;
            await Task.Run(() =>
            {
                try
                {
                    _rack.CheckBox();
                }
                catch (Exception ex)
                {
                    OnErrorOccured(this, 40015, "Test shield box failed.");
                    MessageBox.Show(ex.Message);
                }
            });
            button.Enabled = true;
        }

        private void buttonPausePhoneServer_Click(object sender, EventArgs e)
        {
            _rack.PausePhoneServer();
            buttonHome.Enabled = true;
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            //var gripper =  _rack.GetAvailableGripper();
            //Task.Run(() => { MessageBox.Show("Hello."); });
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
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        #endregion
        private void trackBarSetSpeed2_Scroll(object sender, EventArgs e)
        {
            _rack.SetRobotSpeed(Convert.ToDouble(trackBarSetSpeed2.Value));
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
                    ShieldBox box = _rack.ConverterTeachPosToShieldBox(_selectedTargetPosition);
                    _rack.UnloadAndLoad(box, _selectedGripper, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private async void buttonLoadForTeach_Click(object sender, EventArgs e)
        {
            _rack.SetRobotSpeed(defaultTestSpeed);

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
                _rack.CalculateG1ToG2Offset(_selectedTargetPosition);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private async void buttonLoad_Click(object sender, EventArgs e)
        {
            if (Ask("Load") == false)
            {
                return;
            }
            _rack.SetRobotSpeed(defaultTestSpeed);

            await Task.Run((Action)(() =>
            {
                try
                {
                    TargetPosition target = _rack.ConverterTeachPosToTargetPosition(_selectedTargetPosition);
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

        private bool Ask(string question)
        {
            return MessageBox.Show(question, "确认",
                             MessageBoxButtons.YesNo,
                             MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private double defaultTestSpeed = 50;

        private async void buttonPick_Click(object sender, EventArgs e)
        {
            if (Ask("Pick") == false)
            {
                return;
            }

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
            if (Ask("Place") == false)
            {
                return;
            }

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
            if (Ask("Unload") == false)
            {
                return;
            }

            _rack.SetRobotSpeed(defaultTestSpeed);

            await Task.Run((Action)(() =>
            {
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

        private void checkBoxPickConveyorMoveForward_CheckedChanged(object sender, EventArgs e)
        {
            _rack.Conveyor.ConveyorMovingForward = checkBoxPickConveyorMoveForward.Checked;
            XmlReaderWriter.SetConveyorSetting(RackSetting.ConveyorMovingForward, _rack.Conveyor.ConveyorMovingForward.ToString());
            //bool b = Convert.ToBoolean(XmlReaderWriter.GetConveyorSetting(RackSetting.ConveyorMovingForward));
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
               DialogResult result = MessageBox.Show("是否覆盖  " + _selectedTargetPosition + "  的示教位置？",
                    "警告", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    return;
                }
                _rack.SaveTeachPosition(_selectedTargetPosition);
                MessageBox.Show("  " + _selectedTargetPosition + "  的示教位置更新成功");
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
                _rack.SaveApproachHeight(_selectedTargetPosition);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
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
            _rack.SetRobotSpeed(1);
        }

        private void buttonMiddleSpeed_Click(object sender, EventArgs e)
        {
            _rack.SetRobotSpeed(100);
        }
        private void buttonHighSpeed_Click(object sender, EventArgs e)
        {
            _rack.SetRobotSpeed(200);
        }
        #endregion
        #endregion 
        #endregion

        #region UI update
        private void UiUpdate()
        {
            while (true)
            {
                try
                {
                    switch (currentPage)
                    {
                        case TabControlPage.Main:
                            UpdateMainUi();
                            break;
                        case TabControlPage.Robot:
                            UpdateRobotUi();
                            break;
                        case TabControlPage.Conveyor:
                            RefleshConveyorUi();
                            break;
                        case TabControlPage.Box:
                            break;
                        case TabControlPage.Tester:
                            UpdateTesterUi();
                            break;
                        case TabControlPage.Log:
                            break;
                        case TabControlPage.Setting:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    SystemMonitor();

                    Thread.Sleep(500);
                }
                catch (Exception e)
                {
                    OnErrorOccured(this, 40010, "Ui Update error: " + e.Message);
                    Thread.Sleep(5000);
                }
            }
        }

        private void SystemMonitor()
        {
            //if (_keepMonitoringSystem == false)
            //{
            //    return;
            //}

            _errorsList.Clear();

            foreach (var box in _rack.ShieldBoxs)
            {
                if (box.Enabled)
                {
                    if (box.Phone != null)
                    {
                        if (box.Phone.TestCycleTimeStopWatch.ElapsedMilliseconds >
                            box.Phone.MaxTestCycleTimeSec * 1000)
                        {
                            OnErrorOccured(this, 40011, "Phone test timeout in box:" + box.Id);
                            try
                            {
                                box.OpenBox();
                                box.Phone.FailCount = 3; //Set as NG.
                                box.Phone.TestResult = TestResult.Fail;
                                box.Phone.TestCycleTimeStopWatch.Reset();
                            }
                            catch (Exception ex)
                            {
                                OnErrorOccured(this, 40008, ex.Message);
                            }                          
                            _errorsList.Add(new CqcRackError()
                            { Code = 40011, Description = "Phone test timeout in box:" + box.Id });
                        }
                    }
                    if (box.TesterComputerConnected == false)
                    {
                        _errorsList.Add(new CqcRackError()
                        { Code = 40012, Description = "Tester computer of box:" + box.Id + " not connect." });
                    }
                }
            }

            if (_rack.BoxChecked == false)
            {
                _errorsList.Add(new CqcRackError()
                { Code = 40019, Description = "Box are not checked." });
            }

            if (_rack.RobotHomeComplete == false)
            {
                _errorsList.Add(new CqcRackError()
                { Code = 40020, Description = "Robot not homed." });
            }

            _systemFault = false;
            if (_errorsList.Count > 0)
            {
                _systemFault = true;
                _errorText = string.Empty;
                foreach (var error in _errorsList)
                {
                    _errorText += error.Code + " " + error.Description + Environment.NewLine;
                }
            }
            _rack.SystemFault = _systemFault;

            if (_systemFault == false)
            {
                _warningsList.Clear();

                if (_rack.Conveyor.NgFullWarning)
                {
                    _warningsList.Add(new CqcRackError()
                    { Code = 30001, Description = "Ng conveyor full." });
                }

                _systemWarning = false;
                if (_warningsList.Count > 0)
                {
                    _systemWarning = true;
                    _warningText = string.Empty;
                    foreach (var warning in _warningsList)
                    {
                        _warningText += warning.Code + " " + warning.Description + Environment.NewLine;
                    }
                }
            }

            richTextBoxError.BeginInvoke((MethodInvoker)(() =>
            {
                if (_systemFault)
                {
                    richTextBoxError.Text = _errorText;
                    richTextBoxError.BackColor = Color.Red;
                    richTextBoxError.ForeColor = Color.White;
                }
                else
                {
                    if (_systemWarning)
                    {
                        richTextBoxError.Text = _warningText;
                        richTextBoxError.BackColor = Color.Yellow;
                        richTextBoxError.ForeColor = Color.Black;
                    }
                    else
                    {
                        richTextBoxError.Text = "系统正常。";
                        richTextBoxError.BackColor = Color.White;
                        richTextBoxError.ForeColor = Color.Black;
                    }
                }

            }));
        }

        private void UpdateMainUi()
        {

        }

        private void Delay(int ms = 50)
        {
            Thread.Sleep(ms);
        }

        private void UpdateRobotUi()
        {
            labelPositionG1.BeginInvoke((MethodInvoker)(() => { labelPositionG1.Text = _rack.Steppers.GetPosition(RackGripper.One).ToString("0.00"); }));
            Delay();
            labelPositionG2.BeginInvoke((MethodInvoker)(() => { labelPositionG2.Text = _rack.Steppers.GetPosition(RackGripper.Two).ToString("0.00"); }));
            Delay();
            labelPositionX.BeginInvoke((MethodInvoker)(() => { labelPositionX.Text = _rack.Motion.GetPositionX().ToString("0.00"); }));
            Delay();
            labelPositionY.BeginInvoke((MethodInvoker)(() => { labelPositionY.Text = _rack.Motion.GetPosition(_rack.Motion.MotorY).ToString("0.00"); }));
            Delay();
            labelPositionZ.BeginInvoke((MethodInvoker)(() => { labelPositionZ.Text = _rack.Motion.GetPosition(_rack.Motion.MotorZ).ToString("0.00"); }));
            Delay();
            labelPositionR.BeginInvoke((MethodInvoker)(() => { labelPositionR.Text = _rack.Motion.GetPosition(_rack.Motion.MotorR).ToString("0.00"); }));
            Delay();
            trackBarSetSpeed2.BeginInvoke((MethodInvoker)(() =>
            {
                trackBarSetSpeed2.Value = (int)(_rack.Motion.GetVelocity(_rack.Motion.MotorZ) / _rack.Motion.MotorZ.SpeedFactor);
            }));

            labelSpeed2.BeginInvoke((MethodInvoker)(() =>
            {
                labelSpeed2.Text = trackBarSetSpeed2.Value.ToString();
            }));

            Delay();
            buttonEableZ.BeginInvoke((MethodInvoker)(() => { buttonEableZ.Text = _rack.Motion.IsEnabled(_rack.Motion.MotorZ) ? "Disable" : "Enable";}));
            Delay();
            buttonEableX1.BeginInvoke((MethodInvoker)(() => { buttonEableX1.Text = _rack.Motion.IsEnabled(_rack.Motion.MotorX1)? "Disable" : "Enable"; }));
            Delay();
            buttonEableX2.BeginInvoke((MethodInvoker)(() => { buttonEableX2.Text = _rack.Motion.IsEnabled(_rack.Motion.MotorX2) ? "Disable" : "Enable"; }));
            Delay();
            buttonEableY.BeginInvoke((MethodInvoker)(() => { buttonEableY.Text = _rack.Motion.IsEnabled(_rack.Motion.MotorY)  ? "Disable" : "Enable"; }));
            Delay();
            buttonEableR.BeginInvoke((MethodInvoker)(() => { buttonEableR.Text = _rack.Motion.IsEnabled(_rack.Motion.MotorR) ? "Disable" : "Enable"; }));
            Delay();
            buttonG1TightOrLoose.BeginInvoke((MethodInvoker)(() => { buttonG1TightOrLoose.Text = _rack.EcatIo.GetInput(Input.Gripper01Tight) ? "G1Open" : "G1Close"; }));
            Delay();
            buttonG2TightOrLoose.BeginInvoke((MethodInvoker)(() => { buttonG2TightOrLoose.Text = _rack.EcatIo.GetInput(Input.Gripper02Tight) ? "G2Open" : "G2Close"; }));
            Delay();
            buttonEableG1.BeginInvoke((MethodInvoker)(() => { buttonEableG1.Text = _rack.Steppers.GetStatus(RackGripper.One, StatusCode.Enabled) ? "Disable" : "Enable"; }));
            Delay();
            buttonEableG2.BeginInvoke((MethodInvoker)(() => { buttonEableG2.Text = _rack.Steppers.GetStatus(RackGripper.Two, StatusCode.Enabled) ? "Disable" : "Enable"; }));
            Delay();
        }
        #endregion

        #region ShieldBox

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
                    case "Box1": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.Type, ShieldBoxType.Bt.ToString()); break;
                    case "Box2": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.Type, ShieldBoxType.Bt.ToString()); break;
                    case "Box3": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.Type, ShieldBoxType.Bt.ToString()); break;
                    case "Box4": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.Type, ShieldBoxType.Bt.ToString()); break;
                    case "Box5": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.Type, ShieldBoxType.Bt.ToString()); break;
                    case "Box6": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.Type, ShieldBoxType.Bt.ToString()); break;
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
                    case "Box1": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.Type, ShieldBoxType.Wifi.ToString()); break;
                    case "Box2": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.Type, ShieldBoxType.Wifi.ToString()); break;
                    case "Box3": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.Type, ShieldBoxType.Wifi.ToString()); break;
                    case "Box4": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.Type, ShieldBoxType.Wifi.ToString()); break;
                    case "Box5": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.Type, ShieldBoxType.Wifi.ToString()); break;
                    case "Box6": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.Type, ShieldBoxType.Wifi.ToString()); break;
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
                    case "Box1": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.Type, ShieldBoxType.Rf.ToString()); break;
                    case "Box2": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 2, ShieldBoxItem.Type, ShieldBoxType.Rf.ToString()); break;
                    case "Box3": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 3, ShieldBoxItem.Type, ShieldBoxType.Rf.ToString()); break;
                    case "Box4": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 4, ShieldBoxItem.Type, ShieldBoxType.Rf.ToString()); break;
                    case "Box5": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 5, ShieldBoxItem.Type, ShieldBoxType.Rf.ToString()); break;
                    case "Box6": XmlReaderWriter.SetBoxAttribute(Files.BoxData, 6, ShieldBoxItem.Type, ShieldBoxType.Rf.ToString()); break;
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
                try
                {
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
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
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
                try
                {
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
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
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
                    case "Box1": BoxId = 1;
                        _rack.ShieldBox1.Enabled = true;
                        break;
                    case "Box2": BoxId = 2;
                        _rack.ShieldBox2.Enabled = true;
                        break;
                    case "Box3": BoxId = 3;
                        _rack.ShieldBox3.Enabled = true;
                        break;
                    case "Box4": BoxId = 4;
                        _rack.ShieldBox4.Enabled = true;
                        break;
                    case "Box5": BoxId = 5;
                        _rack.ShieldBox5.Enabled = true;
                        break;
                    case "Box6": BoxId = 6;
                        _rack.ShieldBox6.Enabled = true;
                        _rack.ShieldBox6.Enabled = true;
                        _rack.ShieldBox6.Enabled = true;
                        _rack.ShieldBox6.Enabled = true;
                        break;
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
                        case "Box1": BoxId = 1; Com = comboBox1.Text; comboBox1.Text = "None";
                            //Todo check null
                            _rack.ShieldBox1.Enabled = false;
                            _rack.ShieldBox1.Available = false;
                            break;
                        case "Box2": BoxId = 2; Com = comboBox2.Text; comboBox2.Text = "None";
                            _rack.ShieldBox2.Enabled = false;
                            _rack.ShieldBox2.Available = false;
                            break;
                        case "Box3":
                            BoxId = 3; Com = comboBox3.Text; comboBox3.Text = "None";
                            _rack.ShieldBox3.Enabled = false;
                            _rack.ShieldBox3.Available = false;
                            break;
                        case "Box4": BoxId = 4; Com = comboBox4.Text; comboBox4.Text = "None";
                            _rack.ShieldBox4.Enabled = false;
                            _rack.ShieldBox4.Available = false;
                            break;
                        case "Box5": BoxId = 5; Com = comboBox5.Text; comboBox5.Text = "None";
                            _rack.ShieldBox5.Enabled = false;
                            _rack.ShieldBox5.Available = false;
                            break;
                        case "Box6": BoxId = 6; Com = comboBox6.Text; comboBox6.Text = "None";
                            _rack.ShieldBox6.Enabled = false;
                            _rack.ShieldBox6.Available = false;
                            break;
                        default:
                            break;
                    }
                    XmlReaderWriter.SetBoxAttribute(Files.BoxData, BoxId, ShieldBoxItem.State, "Disable");
                    //XmlReaderWriter.SetBoxAttribute(Files.BoxData, BoxId, ShieldBoxItem.COM, "None");
                    //if (_portName.Contains(Com) == false)
                    //    _portName.Add(Com);
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

        #region Conveyor
        private void buttonUpBlockSeparateForward_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonUpBlockSeparateForward.Text == "Down")
                    _rack.Conveyor.SetCylinder(Output.UpBlockSeparateForward, Input.UpBlockSeparateForward, false);
                else
                    _rack.Conveyor.ResetCylinder(Output.UpBlockSeparateForward, Input.UpBlockSeparateForward);
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
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void buttonStartConveyorManager_Click(object sender, EventArgs e)
        {
            _rack.StartConveyorManager();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            richTextBoxMessage.Text = "System OK";
            _rack.Reset();
        }

        private void buttonBeltPick_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonBeltPick.Text == "Run")
                    _rack.EcatIo.SetOutput(Output.BeltPick, true);
                else
                    _rack.EcatIo.SetOutput(Output.BeltPick, false);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonBeltConveyorOne_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonBeltConveyorOne.Text == "Run")
                    _rack.EcatIo.SetOutput(Output.BeltConveyorOne, true);
                else
                    _rack.EcatIo.SetOutput(Output.BeltConveyorOne, false);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonBeltBin_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonBeltBin.Text == "Run")
                    _rack.EcatIo.SetOutput(Output.BeltBin, true);
                else
                    _rack.EcatIo.SetOutput(Output.BeltBin, false);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void RefleshConveyorUi()
        {
            try
            {
                #region LabelReflesh
                SetIndicator(labelConveyorBinInTwo, Input.ConveyorBinInTwo);
                SetIndicator(labelConveyorBinIn, Input.ConveyorBinIn);
                SetIndicator(labelConveyorOneIn, Input.ConveyorOneIn);
                SetIndicator(labelConveyorOneOut, Input.ConveyorOneOut);
                SetIndicator(labelPickBufferHasPhoneForward, Input.PickBufferHasPhoneForward);
                SetIndicator(labelPickHasPhone, Input.PickHasPhone);
                SetIndicator(labelPickBufferHasPhoneBackward, Input.PickBufferHasPhoneBackward);

                SetIndicator(labelGold1, Input.Gold1);
                SetIndicator(labelGold2, Input.Gold2);
                SetIndicator(labelGold3, Input.Gold3);
                SetIndicator(labelGold4, Input.Gold4);
                SetIndicator(labelGold5, Input.Gold5);
                #endregion

                #region ButtonReflesh
                SetButtonText(buttonUpBlockSeparateForward, Input.UpBlockSeparateForward, "Down", "Up");
                SetButtonText(buttonUpBlockSeparateForward, Input.UpBlockSeparateForward, "Down", "Up");
                SetButtonText(buttonUpBlockPickBackward, Input.UpBlockPickBackward, "Down", "Up");
                SetButtonText(buttonUpBlockPickForward, Input.UpBlockPickForward, "Down", "Up");
                SetButtonText(buttonUpBlockSeparateBackward, Input.UpBlockSeparateBackward, "Down", "Up");

                SetButtonText(buttonOpenOrClose, Input.ClampTightPick, "Open", "Close");
                SetButtonText(buttonSideBlockSeparateForward, Input.SideBlockSeparateForward, "Stretch", "Retract");
                SetButtonText(buttonSideBlockPick, Input.SideBlockPick, "Stretch", "Retract");
                SetButtonText(buttonSideBlockSeparateBackward, Input.SideBlockSeparateBackward, "Stretch", "Retract");

                SetButtonText(buttonBeltPick, Output.BeltPick, "Stop", "Run");
                SetButtonText(buttonBeltBin, Output.BeltBin, "Stop", "Run");
                SetButtonText(buttonBeltConveyorOne, Output.BeltConveyorOne, "Stop", "Run");

                #endregion
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void SetIndicator(Label label, Input input)
        {
            label.BeginInvoke((MethodInvoker)(() =>
            {
                label.ForeColor = _rack.EcatIo.GetInput(input) ? Color.DarkGreen : Color.LightGray;
            }));
        }

        private void SetIndicator(Label label, bool state)
        {
            label.BeginInvoke((MethodInvoker)(() =>
            {
                label.ForeColor = state ? Color.DarkGreen : Color.LightGray;
            }));
        }

        private void SetText(Label label, object obj)
        {
            label.BeginInvoke((MethodInvoker)(() =>
            {
                if (obj!=null)
                {
                    label.Text = obj.ToString();
                }
                else
                {
                    label.Text = "???";
                }              
            }));
        }

        private void SetButtonText(Button button, Input input, string trueText, string falseText)
        {
            button.BeginInvoke((MethodInvoker)(() =>
            {
                button.Text = _rack.EcatIo.GetInput(input) ? trueText : falseText;
            }));
        }

        private void SetButtonText(Button button, Output output, string trueText, string falseText)
        {
            button.BeginInvoke((MethodInvoker)(() =>
            {
                button.Text = _rack.EcatIo.GetOutput(output) ? trueText : falseText;
            }));
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
                tabControlUi.SelectedIndex = 0;
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

        private void checkBoxMotionSimulate_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            _rack.RobotInSimulateMode = checkBox.Checked;
        }
        #endregion

        #region Tabcontrol page change event
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                currentPage = (TabControlPage)tabControlUi.SelectedIndex;

                //if (toolStripStatusLabelPower.Text == "None")
                //{
                //    if (tabControl1.SelectedIndex != 6)
                //    {
                //        tabControl1.SelectedIndex = 6;
                //        throw new Exception("Please Login in");
                //    }
                //}

                //if (_isStart == false && tabControl1.SelectedIndex != 6 && tabControl1.SelectedIndex != 0)
                //if (_isStart == false && tabControl1.SelectedIndex != 6 && tabControl1.SelectedIndex != 0)
                //{
                //    tabControl1.SelectedIndex = 0;
                //    throw new Exception("Please click \"Start\",or you can not click anywhere except \"Setting\"");
                //}


                switch (tabControlUi.SelectedIndex)
                {
                    case 0: break;
                    case 1: break;
                    case 2: break;
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

        public enum TabControlPage
        {
            Main,
            Robot,
            Conveyor,
            Box,
            Tester,
            Log,
            Setting,
        }

        #endregion

        #region Tester
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

        private void buttonStartTesterSimulator_Click(object sender, EventArgs e)
        {
            _selectedSocketClient = _client1;
            _client1.Start();
            _client2.Start();
            _client3.Start();
            _client4.Start();
            _client5.Start();
            _client6.Start();
        }

        private void radioButtonBox1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if (radioButton.Checked)
            {
                _selectedSocketClient = _client1;
            }
        }

        private void radioButtonBox2_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if (radioButton.Checked)
            {
                _selectedSocketClient = _client2;
            }
        }

        private void radioButtonBox3_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if (radioButton.Checked)
            {
                _selectedSocketClient = _client3;
            }
        }

        private void radioButtonBox4_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if (radioButton.Checked)
            {
                _selectedSocketClient = _client4;
            }
        }

        private void radioButtonBox5_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if (radioButton.Checked)
            {
                _selectedSocketClient = _client5;
            }
        }

        private void radioButtonBox6_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if (radioButton.Checked)
            {
                _selectedSocketClient = _client6;
            }
        }

        private void buttonTesterSendPass_Click(object sender, EventArgs e)
        {
            _selectedSocketClient.SetPass();
        }

        private void buttonTesterSendFail_Click(object sender, EventArgs e)
        {
            _selectedSocketClient.SetFail();
        }

        private void UpdateTesterUi()
        {
            if (_rack.ShieldBox1 != null)
            {
                if (_rack.ShieldBox1.Enabled)
                {
                    SetIndicator(labelTester01Connected, _rack.Tester1.Connected);
                    if (_rack.ShieldBox1.Phone != null)
                    {
                        SetText(labelTester01PhoneId, _rack.ShieldBox1.Phone.Id);
                        SetText(labelTester01PhoneSerialNumber, _rack.ShieldBox1.Phone.SerialNumber);
                        SetText(labelTester01PhoneStep, _rack.ShieldBox1.Phone.Step);

                        SetText(labelTester01PhoneTestResult, _rack.ShieldBox1.Phone.TestResult);
                        SetText(labelTester01PhoneFailCount, _rack.ShieldBox1.Phone.FailCount);
                        SetText(labelTester01PhoneTestTime,
                            _rack.ShieldBox1.Phone.TestCycleTimeStopWatch.ElapsedMilliseconds / 1000);
                    }
                    else
                    {
                        SetText(labelTester01PhoneId, "----");
                        SetText(labelTester01PhoneSerialNumber, "----");
                        SetText(labelTester01PhoneStep, "----");
                        SetText(labelTester01PhoneTestResult, "----");
                        SetText(labelTester01PhoneFailCount, "----");
                        SetText(labelTester01PhoneTestTime, "----");
                    }
                }
            }

            if (_rack.ShieldBox2 != null)
            {
                if (_rack.ShieldBox2.Enabled)
                {
                    SetIndicator(labelTester02Connected, _rack.Tester2.Connected);
                    if (_rack.ShieldBox2.Phone != null)
                    {
                        SetText(labelTester02PhoneId, _rack.ShieldBox2.Phone.Id);
                        SetText(labelTester02PhoneSerialNumber, _rack.ShieldBox2.Phone.SerialNumber);
                        SetText(labelTester02PhoneStep, _rack.ShieldBox2.Phone.Step);

                        SetText(labelTester02PhoneTestResult, _rack.ShieldBox2.Phone.TestResult);
                        SetText(labelTester02PhoneFailCount, _rack.ShieldBox2.Phone.FailCount);
                        SetText(labelTester02PhoneTestTime,
                            _rack.ShieldBox2.Phone.TestCycleTimeStopWatch.ElapsedMilliseconds / 1000);
                    }
                    else
                    {
                        SetText(labelTester02PhoneId, "----");
                        SetText(labelTester02PhoneSerialNumber, "----");
                        SetText(labelTester02PhoneStep, "----");
                        SetText(labelTester02PhoneTestResult, "----");
                        SetText(labelTester02PhoneFailCount, "----");
                        SetText(labelTester02PhoneTestTime, "----");
                    }
                }
            }

            if (_rack.ShieldBox3 != null)
            {
                if (_rack.ShieldBox3.Enabled)
                {
                    SetIndicator(labelTester03Connected, _rack.Tester3.Connected);
                    if (_rack.ShieldBox3.Phone != null)
                    {
                        SetText(labelTester03PhoneId, _rack.ShieldBox3.Phone.Id);
                        SetText(labelTester03PhoneSerialNumber, _rack.ShieldBox3.Phone.SerialNumber);
                        SetText(labelTester03PhoneStep, _rack.ShieldBox3.Phone.Step);

                        SetText(labelTester03PhoneTestResult, _rack.ShieldBox3.Phone.TestResult);
                        SetText(labelTester03PhoneFailCount, _rack.ShieldBox3.Phone.FailCount);
                        SetText(labelTester03PhoneTestTime,
                            _rack.ShieldBox3.Phone.TestCycleTimeStopWatch.ElapsedMilliseconds / 1000);
                    }
                    else
                    {
                        SetText(labelTester03PhoneId, "----");
                        SetText(labelTester03PhoneSerialNumber, "----");
                        SetText(labelTester03PhoneStep, "----");
                        SetText(labelTester03PhoneTestResult, "----");
                        SetText(labelTester03PhoneFailCount, "----");
                        SetText(labelTester03PhoneTestTime, "----");
                    }
                }
            }

            if (_rack.ShieldBox4 != null)
            {
                if (_rack.ShieldBox4.Enabled)
                {
                    SetIndicator(labelTester04Connected, _rack.Tester4.Connected);
                    if (_rack.ShieldBox4.Phone != null)
                    {
                        SetText(labelTester04PhoneId, _rack.ShieldBox4.Phone.Id);
                        SetText(labelTester04PhoneSerialNumber, _rack.ShieldBox4.Phone.SerialNumber);
                        SetText(labelTester04PhoneStep, _rack.ShieldBox4.Phone.Step);

                        SetText(labelTester04PhoneTestResult, _rack.ShieldBox4.Phone.TestResult);
                        SetText(labelTester04PhoneFailCount, _rack.ShieldBox4.Phone.FailCount);
                        SetText(labelTester04PhoneTestTime,
                            _rack.ShieldBox4.Phone.TestCycleTimeStopWatch.ElapsedMilliseconds / 1000);
                    }
                    else
                    {
                        SetText(labelTester04PhoneId, "----");
                        SetText(labelTester04PhoneSerialNumber, "----");
                        SetText(labelTester04PhoneStep, "----");
                        SetText(labelTester04PhoneTestResult, "----");
                        SetText(labelTester04PhoneFailCount, "----");
                        SetText(labelTester04PhoneTestTime, "----");
                    }
                }
            }

            if (_rack.ShieldBox5 != null)
            {
                if (_rack.ShieldBox5.Enabled)
                {
                    SetIndicator(labelTester05Connected, _rack.Tester5.Connected);
                    if (_rack.ShieldBox5.Phone != null)
                    {
                        SetText(labelTester05PhoneId, _rack.ShieldBox5.Phone.Id);
                        SetText(labelTester05PhoneSerialNumber, _rack.ShieldBox5.Phone.SerialNumber);
                        SetText(labelTester05PhoneStep, _rack.ShieldBox5.Phone.Step);

                        SetText(labelTester05PhoneTestResult, _rack.ShieldBox5.Phone.TestResult);
                        SetText(labelTester05PhoneFailCount, _rack.ShieldBox5.Phone.FailCount);
                        SetText(labelTester05PhoneTestTime,
                            _rack.ShieldBox5.Phone.TestCycleTimeStopWatch.ElapsedMilliseconds / 1000);
                    }
                    else
                    {
                        SetText(labelTester05PhoneId, "----");
                        SetText(labelTester05PhoneSerialNumber, "----");
                        SetText(labelTester05PhoneStep, "----");
                        SetText(labelTester05PhoneTestResult, "----");
                        SetText(labelTester05PhoneFailCount, "----");
                        SetText(labelTester05PhoneTestTime, "----");
                    }
                }
            }

            if (_rack.ShieldBox6 != null)
            {
                if (_rack.ShieldBox6.Enabled)
                {
                    SetIndicator(labelTester06Connected, _rack.Tester6.Connected);
                    if (_rack.ShieldBox6.Phone != null)
                    {
                        SetText(labelTester06PhoneId, _rack.ShieldBox6.Phone.Id);
                        SetText(labelTester06PhoneSerialNumber, _rack.ShieldBox6.Phone.SerialNumber);
                        SetText(labelTester06PhoneStep, _rack.ShieldBox6.Phone.Step);

                        SetText(labelTester06PhoneTestResult, _rack.ShieldBox6.Phone.TestResult);
                        SetText(labelTester06PhoneFailCount, _rack.ShieldBox6.Phone.FailCount);
                        SetText(labelTester06PhoneTestTime,
                            _rack.ShieldBox6.Phone.TestCycleTimeStopWatch.ElapsedMilliseconds / 1000);
                    }
                    else
                    {
                        SetText(labelTester06PhoneId, "----");
                        SetText(labelTester06PhoneSerialNumber, "----");
                        SetText(labelTester06PhoneStep, "----");
                        SetText(labelTester06PhoneTestResult, "----");
                        SetText(labelTester06PhoneFailCount, "----");
                        SetText(labelTester06PhoneTestTime, "----");
                    }
                }
            }
        }

        #endregion

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("是否退出程序？", "退出", MessageBoxButtons.YesNo);

            if (dialogResult != DialogResult.Yes)
            {
                return;
            }

            NewLog.Instance.Info("20004 User close program.");

            if (_uiUpdateThread != null)
            {
                _uiUpdateThread.Abort();
                //_uiUpdateThread.Join();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            var result = MessageBox.Show("是否退出程序？", "退出",
                             MessageBoxButtons.YesNo,
                             MessageBoxIcon.Question);

            e.Cancel = (result == DialogResult.No);

            if (result == DialogResult.Yes)
            {
                NewLog.Instance.Info("20004 User close program.");

                if (_uiUpdateThread != null)
                {
                    _uiUpdateThread.Abort();
                    //_uiUpdateThread.Join();
                }
            }
        }

        private void richTextBoxMessage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (richTextBoxMessage.Height > this.Height / 2)
            {
                WhenTextBoxClose();
            }
            else
            {
                WhenTextBoxOpen();
            }
        }

        private void richTextBoxError_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (richTextBoxError.Height > this.Height / 2)
            {
                WhenTextBoxClose();
            }
            else
            {
                WhenTextBoxOpen();
            }
        }

        private void WhenTextBoxClose()
        {
            richTextBoxMessage.Height = tabControlUi.Location.Y - 10;
            richTextBoxError.Height = tabControlUi.Location.Y - 10;
            tabControlUi.Visible = true;
            _scrollRichTextBox = true;
            AddMessageToTextBox(20000, "User collapes message text.");
        }

        private void WhenTextBoxOpen()
        {
            richTextBoxError.Height = this.Height - 100;
            richTextBoxMessage.Height = this.Height - 100;
            tabControlUi.Visible = false;
            _keepMonitoringSystem = false;
            _scrollRichTextBox = false;
        }

        private void buttonEmptyNg_Click(object sender, EventArgs e)
        {
            if (_rack.Conveyor != null)
            {
                _rack.Conveyor.NgCount = 0;
            }
        }

        private void buttonEmptyBox05_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.RemovePhoneToBeServed(_rack.ShieldBox5.Phone);
                _rack.Unlink(_rack.ShieldBox5.Phone);
                MessageBox.Show("清空箱子成功.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("清空箱子失败：" + ex.Message);
            }
        }

        private void buttonSetSerialNumber_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Scanner.SerialNumber = textBox1.Text;
                _rack.Scanner.ScanSuccessful = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Set serial number fail：" + ex.Message);
            }
        }

        private void buttonPassTester05_Click(object sender, EventArgs e)
        {
            try
            {
                if (_rack.ShieldBox5.IsClosed() == true)
                    _rack.ShieldBox5.OpenBox();
                _rack.ShieldBox5.Phone.TestResult = TestResult.Pass;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Set passed fail：" + ex.Message);
            }
        }

        //private void buttonSetClosedBox05_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        _rack.ShieldBox5.State = ShieldBoxState.Close;
        //        _rack.ShieldBox5.ReadyForTesting = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Set box closed fail：" + ex.Message);
        //    }
        //}

        private void checkBoxServerSimulate_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            _rack.ServerInSimulateMode = checkBox.Checked;
        }

        private void loadLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string FileName = string.Empty;
                OpenFileDialog OpenFile = new OpenFileDialog()
                {
                    InitialDirectory = System.IO.Path.GetDirectoryName(
                        System.AppDomain.CurrentDomain.BaseDirectory + @"\Logs\"),
                    Filter = "|*.log",
                };

                DialogResult Result = OpenFile.ShowDialog();
                if (Result == DialogResult.OK)
                    FileName = OpenFile.FileName;
                else
                    return;
                dataGridView1.Rows.Clear();
                StreamReader Reader = new StreamReader(FileName, Encoding.Default);
                while (!Reader.EndOfStream)
                {

                    string[] Items = Reader.ReadLine().Split(' ');
                    //if (Items.Length != 5)
                    //{
                    //    //Reader.Close();
                    //    //Reader.Dispose();
                    //    continue;
                    //}

                    string description = string.Empty;
                    for (int i = 4; i < Items.Length; i++)
                    {
                        description += Items[i] + " ";
                    }

                    DataGridViewRow dataGridViewRow = new DataGridViewRow();
                    foreach (DataGridViewColumn Item in dataGridView1.Columns)
                    {
                        dataGridViewRow.Cells.Add(Item.CellTemplate.Clone() as DataGridViewCell);
                    }
                    dataGridViewRow.Cells[0].Value = Items[0] + " " + Items[1];
                    dataGridViewRow.Cells[1].Value = Items[2];
                    dataGridViewRow.Cells[2].Value = Items[3];
                    dataGridViewRow.Cells[3].Value = description;
                    dataGridView1.Rows.Add(dataGridViewRow);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonSetSerial05_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Tester5.SimulateSerialNumber = textBoxSerial05.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Set serial number fail：" + ex.Message);
            }
        }

        private void checkBoxTesterSimulate05_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            _rack.Tester5.SimulateMode = checkBox.Checked;
        }

        private void buttonSetSerial02_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Tester2.SimulateSerialNumber = textBoxSerial02.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Set serial number fail：" + ex.Message);
            }
        }

        private void checkBoxTesterSimulate02_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            _rack.Tester2.SimulateMode = checkBox.Checked;
        }

        public int RfCycleTime { get; set; } = 46;
        public int WifiCycleTime { get; set; } = 35;

        private void checkBoxAutoPass_CheckedChanged(object sender, EventArgs e)
        {
            _autoPass = checkBoxAutoPass.Checked;
            Task.Run(() => {
                while (_autoPass)
                {
                    foreach (var box in _rack.ShieldBoxs)
                    {
                        if (box.Phone!=null)
                        {
                            if (box.Type == ShieldBoxType.Rf && 
                            box.Phone.TestCycleTimeStopWatch.ElapsedMilliseconds/1000 > RfCycleTime)
                            {                                
                                try
                                {
                                    if (box.IsClosed()==false)
                                    {
                                        continue;
                                    }
                                    box.OpenBox();
                                    box.Phone.TestCycleTimeStopWatch.Reset();
                                }
                                catch (Exception)
                                {
                                    Delay(3000);
                                }                              
                                box.Phone.TestResult = TestResult.Pass;
                            }

                            if (box.Type == ShieldBoxType.Wifi && 
                            box.Phone.TestCycleTimeStopWatch.ElapsedMilliseconds / 1000 > WifiCycleTime)
                            {
                                try
                                {
                                    if (box.IsClosed() == false)
                                    {
                                        continue;
                                    }
                                    box.OpenBox();
                                    box.Phone.TestCycleTimeStopWatch.Reset();
                                }
                                catch (Exception)
                                {
                                    Delay(3000);
                                }
                                box.Phone.TestResult = TestResult.Pass;
                            }

                        }
                    }

                    Delay(100);
                }
            });
        }

        private void buttonBox05GoldStart_Click(object sender, EventArgs e)
        {
            _rack.ShieldBox5.GoldPhoneCheckRequest = true;
            MessageBox.Show("GoldPhoneCheckRequest set.");
        }

        private void buttonBox05GoldEnd_Click(object sender, EventArgs e)
        {
            _rack.ShieldBox5.GoldPhoneChecked = true;
            MessageBox.Show("GoldPhoneChecked set.");
        }

        private void buttonBox01GoldStart_Click(object sender, EventArgs e)
        {
            _rack.ShieldBox1.GoldPhoneCheckRequest = true;
            MessageBox.Show("GoldPhoneCheckRequest set.");
        }

        private void buttonBox01GoldEnd_Click(object sender, EventArgs e)
        {
            _rack.ShieldBox1.GoldPhoneChecked = true;
            MessageBox.Show("GoldPhoneChecked set.");
        }

        private void checkBoxTesterSimulate03_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            _rack.Tester3.SimulateMode = checkBox.Checked;
        }

        private void buttonBox03GoldStart_Click(object sender, EventArgs e)
        {
            _rack.ShieldBox3.GoldPhoneCheckRequest = true;
            MessageBox.Show("GoldPhoneCheckRequest set.");
        }

        private void buttonBox03GoldEnd_Click(object sender, EventArgs e)
        {
            _rack.ShieldBox3.GoldPhoneChecked = true;
            MessageBox.Show("GoldPhoneChecked set.");
        }

        private void buttonBox02GoldStart_Click(object sender, EventArgs e)
        {
            _rack.ShieldBox2.GoldPhoneCheckRequest = true;
            MessageBox.Show("GoldPhoneCheckRequest set.");
        }

        private void buttonBox04GoldStart_Click(object sender, EventArgs e)
        {
            _rack.ShieldBox4.GoldPhoneCheckRequest = true;
            MessageBox.Show("GoldPhoneCheckRequest set.");
        }

        private void buttonBox06GoldStart_Click(object sender, EventArgs e)
        {
            _rack.ShieldBox6.GoldPhoneCheckRequest = true;
            MessageBox.Show("GoldPhoneCheckRequest set.");
        }

        private void buttonBox02GoldEnd_Click(object sender, EventArgs e)
        {
            _rack.ShieldBox2.GoldPhoneChecked = true;
            MessageBox.Show("GoldPhoneChecked set.");
        }

        private void buttonBox04GoldEnd_Click(object sender, EventArgs e)
        {
            _rack.ShieldBox4.GoldPhoneChecked = true;
            MessageBox.Show("GoldPhoneChecked set.");
        }

        private void buttonBox06GoldEnd_Click(object sender, EventArgs e)
        {
            _rack.ShieldBox6.GoldPhoneChecked = true;
            MessageBox.Show("GoldPhoneChecked set.");
        }

        private void checkBoxTesterSimulate04_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            _rack.Tester4.SimulateMode = checkBox.Checked;
        }

        private void checkBoxTesterSimulate01_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            _rack.Tester1.SimulateMode = checkBox.Checked;
        }

        private void checkBoxTesterSimulate06_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            _rack.Tester6.SimulateMode = checkBox.Checked;
        }

        private void buttonEmptyBox02_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.RemovePhoneToBeServed(_rack.ShieldBox2.Phone);
                _rack.Unlink(_rack.ShieldBox2.Phone);
                MessageBox.Show("清空箱子成功.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("清空箱子失败：" + ex.Message);
            }
        }

        private void buttonEmptyBox01_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.RemovePhoneToBeServed(_rack.ShieldBox1.Phone);
                _rack.Unlink(_rack.ShieldBox1.Phone);
                MessageBox.Show("清空箱子成功.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("清空箱子失败：" + ex.Message);
            }
        }

        private void buttonEmptyBox03_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.RemovePhoneToBeServed(_rack.ShieldBox3.Phone);
                _rack.Unlink(_rack.ShieldBox3.Phone);
                MessageBox.Show("清空箱子成功.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("清空箱子失败：" + ex.Message);
            }
        }

        private void buttonEmptyBox04_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.RemovePhoneToBeServed(_rack.ShieldBox4.Phone);
                _rack.Unlink(_rack.ShieldBox4.Phone);
                MessageBox.Show("清空箱子成功.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("清空箱子失败：" + ex.Message);
            }
        }

        private void buttonEmptyBox06_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.RemovePhoneToBeServed(_rack.ShieldBox6.Phone);
                _rack.Unlink(_rack.ShieldBox6.Phone);
                MessageBox.Show("清空箱子成功.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("清空箱子失败：" + ex.Message);
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void checkBoxOneTestInRack_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            _rack.OneTestInRack = checkBox.Checked;
        }

        private void buttonSetHomeSpeed_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.HomeSpeed = Convert.ToDouble(textBoxHomeRobotSpeed.Text);               
            }
            catch (Exception ex)
            {
                MessageBox.Show("设置回零速度失败：" + ex.Message);
            }
        }

        private void checkBoxNoSoftwareNgWarning_CheckedChanged(object sender, EventArgs e)
        {

        }

        private async void buttonEstopReset_Click(object sender, EventArgs e)
        {
            buttonEstopReset.Enabled = false;
            await Task.Run(() => {

                try
                {
                    _rack.Motion.Reboot();
                    MessageBox.Show("急停恢复成功！");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("急停恢复失败：" + ex.Message);
                }

            });
            buttonEstopReset.Enabled = true;
        }

        private void loadNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string FileName = string.Empty;
                OpenFileDialog OpenFile = new OpenFileDialog()
                {
                    InitialDirectory = System.IO.Path.GetDirectoryName(
                        System.AppDomain.CurrentDomain.BaseDirectory + @"\SN\"),
                    Filter = "|*.SN",
            };

                DialogResult Result = OpenFile.ShowDialog();
                if (Result == DialogResult.OK)
                    FileName = OpenFile.FileName;
                else
                    return;
                dataGridView1.Rows.Clear();
                StreamReader Reader = new StreamReader(FileName, Encoding.Default);
                while (!Reader.EndOfStream)
                {

                    string[] Items = Reader.ReadLine().Split(' ');
                    //if (Items.Length != 5)
                    //{
                    //    //Reader.Close();
                    //    //Reader.Dispose();
                    //    continue;
                    //}

                    string description = string.Empty;
                    for (int i = 4; i < Items.Length; i++)
                    {
                        description += Items[i] + " ";
                    }

                    DataGridViewRow dataGridViewRow = new DataGridViewRow();
                    foreach (DataGridViewColumn Item in dataGridView1.Columns)
                    {
                        dataGridViewRow.Cells.Add(Item.CellTemplate.Clone() as DataGridViewCell);
                    }
                    dataGridViewRow.Cells[0].Value = Items[0] + " " + Items[1];
                    dataGridViewRow.Cells[1].Value = Items[2];
                    dataGridViewRow.Cells[2].Value = Items[3];
                    dataGridViewRow.Cells[3].Value = description;
                    dataGridView1.Rows.Add(dataGridViewRow);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonSetCycleTime_Click(object sender, EventArgs e)
        {
            try
            {
                RfCycleTime = Convert.ToInt32(textBoxRfCycleTime.Text);
                WifiCycleTime = Convert.ToInt32(textBoxWifiCycleTime.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
