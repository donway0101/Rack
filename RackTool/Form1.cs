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
using Rack;
using Motion;
using Tools;
using GripperStepper;
using System.Threading;

namespace RackTool
{
    public partial class Form1 : Form
    {
        private readonly CqcRack _rack = new CqcRack("192.168.8.18");
        private Gripper _selectedGripper;
        private TeachPos _selectedTargetPosition;
        private Thread _uiUpdateThread;

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

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Start();
                SetupForTeaching();

                if (_uiUpdateThread==null)
                {
                    _uiUpdateThread = new Thread(UiUpdate)
                    {
                        IsBackground = true
                    };
                }

                if ( _uiUpdateThread.IsAlive==false)
                {
                    _uiUpdateThread.Start();
                }
               
                button2.Enabled = true;
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
                    label6.Invoke((MethodInvoker) (() => { label6.Text=_rack.Stepper.GetPosition(Gripper.One).ToString(); }));
                    label7.Invoke((MethodInvoker)(() => { label7.Text = _rack.Stepper.GetPosition(Gripper.Two).ToString(); }));
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

            comboBox2.Items.Clear();
            foreach (var pos in Enum.GetValues(typeof(Gripper)))
            {
                comboBox2.Items.Add(pos);
            }

            comboBox3.Items.Clear();
            foreach (var pos in Enum.GetValues(typeof(TeachPos)))
            {
                comboBox3.Items.Add(pos);
            }
        }


        private void button2_Click(object sender, EventArgs e)
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

        private void button4_Click(object sender, EventArgs e)
        {
            XmlReaderWriter.CreateStorageFile("RackData.xml");
        }

        bool testLoop = true;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            testLoop = checkBox1.Enabled;
            button3.Enabled = true;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString();
            _rack.SetSpeedImm(Convert.ToDouble(trackBar1.Value));            
        }

        private void button7_Click(object sender, EventArgs e)
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

        private void button6_Click(object sender, EventArgs e)
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
            _rack.Motion.Jog(_rack.Motion.MotorX1, false);
        }

        private void button8_MouseUp(object sender, MouseEventArgs e)
        {
            _rack.Motion.Halt(_rack.Motion.MotorX1);
        }

        private void button9_MouseDown(object sender, MouseEventArgs e)
        {
            _rack.Motion.Jog(_rack.Motion.MotorX1, true);
        }

        private void button9_MouseUp(object sender, MouseEventArgs e)
        {
            _rack.Motion.Halt(_rack.Motion.MotorX1);
        }

        private void button10_MouseDown(object sender, MouseEventArgs e)
        {
            _rack.Motion.Jog(_rack.Motion.MotorX2, false);
        }

        private void button11_MouseDown(object sender, MouseEventArgs e)
        {
            _rack.Motion.Jog(_rack.Motion.MotorX2, true);
        }

        private void button15_MouseDown(object sender, MouseEventArgs e)
        {
            _rack.Motion.Jog(_rack.Motion.MotorY, false);
        }

        private void button13_MouseDown(object sender, MouseEventArgs e)
        {
            _rack.Motion.Jog(_rack.Motion.MotorZ, false);
        }

        private void button17_MouseDown(object sender, MouseEventArgs e)
        {
            _rack.Motion.Jog(_rack.Motion.MotorR, false);
        }

        private void button19_Click(object sender, EventArgs e)
        {
            _rack.Stepper.ToPoint(Gripper.One, Convert.ToDouble(textBox1.Text));
        }

        private void button18_Click(object sender, EventArgs e)
        {
            _rack.Stepper.ToPoint(Gripper.Two, Convert.ToDouble(textBox2.Text));
        }

        private void button14_MouseDown(object sender, MouseEventArgs e)
        {
            _rack.Motion.Jog(_rack.Motion.MotorY, true);
        }

        private void button12_MouseDown(object sender, MouseEventArgs e)
        {
            _rack.Motion.Jog(_rack.Motion.MotorZ, true);
        }

        private void button16_MouseDown(object sender, MouseEventArgs e)
        {
            _rack.Motion.Jog(_rack.Motion.MotorR, true);
        }

        private void button11_MouseUp(object sender, MouseEventArgs e)
        {
            _rack.Motion.Halt(_rack.Motion.MotorX2);
        }

        private void button14_MouseUp(object sender, MouseEventArgs e)
        {
            _rack.Motion.Halt(_rack.Motion.MotorY);
        }

        private void button12_MouseUp(object sender, MouseEventArgs e)
        {
            _rack.Motion.Halt(_rack.Motion.MotorZ);
        }

        private void button16_MouseUp(object sender, MouseEventArgs e)
        {
            _rack.Motion.Halt(_rack.Motion.MotorR);
        }

        private void button13_MouseUp(object sender, MouseEventArgs e)
        {
            _rack.Motion.Halt(_rack.Motion.MotorZ);
        }

        private void button17_MouseUp(object sender, MouseEventArgs e)
        {
            _rack.Motion.Halt(_rack.Motion.MotorR);
        }

        private void button15_MouseUp(object sender, MouseEventArgs e)
        {
            _rack.Motion.Halt(_rack.Motion.MotorY);
        }

        #endregion

        private void button20_Click(object sender, EventArgs e)
        {
            _rack.SetSpeed(defaultTestSpeed);

            Task.Run(() =>
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
            });

        }

        private void button22_Click(object sender, EventArgs e)
        {
            if ( _rack.Stepper.GetStatus(Gripper.One, StatusCode.Enabled))
            {
                _rack.Stepper.Disable(Gripper.One);
            }
            else
            {
                _rack.Stepper.Enable(Gripper.One);
            }
           
        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (_rack.Stepper.GetStatus(Gripper.Two, StatusCode.Enabled))
            {
                _rack.Stepper.Disable(Gripper.Two);
            }
            else
            {
                _rack.Stepper.Enable(Gripper.Two);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedGripper = (Gripper)comboBox2.SelectedItem;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedTargetPosition = (TeachPos) comboBox3.SelectedItem;
        }

        private double defaultTestSpeed = 5;

        private async void button23_Click(object sender, EventArgs e)
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

        private async void button24_Click(object sender, EventArgs e)
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

        private void button25_Click(object sender, EventArgs e)
        {
            _rack.SetSpeed(defaultTestSpeed);

            Task.Run(() =>
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
            });
        }

        private void button26_Click(object sender, EventArgs e)
        {
            _rack.ReadyThePhone();
        }

        private void button27_Click(object sender, EventArgs e)
        {
            _rack.CalculateG1ToG2Offset(_selectedTargetPosition);
        }

        private void button28_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
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

        private void button29_Click(object sender, EventArgs e)
        {
            _rack.EnableMotorsForTeaching();
        }
    }
}
