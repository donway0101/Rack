using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ACS.SPiiPlusNET;
using EcatIo;


namespace IOTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private readonly Api _ch = new Api();
        private static EthercatIo _ioRobot;
        private Thread _inputUpdate;

        private void InputUpdate()
        {
            while (true)
            {
                Thread.Sleep(200);
                try
                {
                    //0
                    Invoke(label1, 0, 0);
                    Invoke(label2, 0, 1);
                    Invoke(label3, 0, 2);
                    Invoke(label4, 0, 3);
                    Invoke(label5, 0, 4);
                    Invoke(label6, 0, 5);
                    Invoke(label7, 0, 6);
                    Invoke(label8, 0, 7);
                    //1
                    Invoke(label11, 1, 0);
                    Invoke(label12, 1, 1);
                    Invoke(label13, 1, 2);
                    Invoke(label14, 1, 3);
                    Invoke(label15, 1, 4);
                    Invoke(label16, 1, 5);
                    Invoke(label17, 1, 6);
                    Invoke(label18, 1, 7);
                    //2
                    Invoke(label21, 2, 0);
                    Invoke(label22, 2, 1);
                    Invoke(label23, 2, 2);
                    Invoke(label24, 2, 3);
                    Invoke(label25, 2, 4);
                    Invoke(label26, 2, 5);
                    Invoke(label27, 2, 6);
                    Invoke(label28, 2, 7);
                    //3
                    Invoke(label31, 3, 0);
                    Invoke(label32, 3, 1);
                    Invoke(label33, 3, 2);
                    Invoke(label34, 3, 3);
                    Invoke(label35, 3, 4);
                    Invoke(label36, 3, 5);
                    Invoke(label37, 3, 6);
                    Invoke(label38, 3, 7);
                    //4
                    Invoke(label41, 4, 0);
                    Invoke(label42, 4, 1);
                    Invoke(label43, 4, 2);
                    Invoke(label44, 4, 3);
                    Invoke(label45, 4, 4);
                    Invoke(label46, 4, 5);
                    Invoke(label47, 4, 6);
                    Invoke(label48, 4, 7);
                }
                catch (Exception)
                {
                    Invoke(button9, () => { button9.Enabled = true; });
                    break;
                    //throw;
                }
            }
        }

        public static void Invoke(Control control, MethodInvoker action)
        {
            control.Invoke(action);
        }

        private void GetOutput()
        {
            button1.Text = _ioRobot.GetOutput(0, 0).ToString();
            button2.Text = _ioRobot.GetOutput(0, 1).ToString();
            button3.Text = _ioRobot.GetOutput(0, 2).ToString();
            button4.Text = _ioRobot.GetOutput(0, 3).ToString();
            button5.Text = _ioRobot.GetOutput(0, 4).ToString();
            button6.Text = _ioRobot.GetOutput(0, 5).ToString();
            button7.Text = _ioRobot.GetOutput(0, 6).ToString();
            button8.Text = _ioRobot.GetOutput(0, 7).ToString();

            button11.Text = _ioRobot.GetOutput(1, 0).ToString();
            button12.Text = _ioRobot.GetOutput(1, 1).ToString();
            button13.Text = _ioRobot.GetOutput(1, 2).ToString();
            button14.Text = _ioRobot.GetOutput(1, 3).ToString();
            button15.Text = _ioRobot.GetOutput(1, 4).ToString();
            button16.Text = _ioRobot.GetOutput(1, 5).ToString();
            button17.Text = _ioRobot.GetOutput(1, 6).ToString();
            button18.Text = _ioRobot.GetOutput(1, 7).ToString();

            //button21.Text = _ioRobot.GetOutput(2, 0).ToString();
            //button22.Text = _ioRobot.GetOutput(2, 1).ToString();
            //button23.Text = _ioRobot.GetOutput(2, 2).ToString();
            //button24.Text = _ioRobot.GetOutput(2, 3).ToString();
            //button25.Text = _ioRobot.GetOutput(2, 4).ToString();
            //button26.Text = _ioRobot.GetOutput(2, 5).ToString();
            //button27.Text = _ioRobot.GetOutput(2, 6).ToString();
            //button28.Text = _ioRobot.GetOutput(2, 7).ToString();
        }

        public static void Invoke(Control control, int moduleId, int inputPinNum)
        {
            control.Invoke((MethodInvoker)(() => { control.Text = _ioRobot.GetInput(moduleId, inputPinNum).ToString(); }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 0, 0);
        }

        private void SetOutput(object sender, int moduleId, int outputPinNum)
        {
            Button bt = (Button)sender;
            bt.Text = bt.Text == "True" ? "False" : "True";
            _ioRobot.SetOutput(moduleId, outputPinNum, !Convert.ToBoolean(bt.Text));
            //bt.Text = bt.Text == "True" ? "False" : "True";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                _ch.OpenCommEthernet("192.168.8.18", 701);
                _ioRobot = new EthercatIo(_ch, 72, 7, 4);
                _ioRobot.Setup();

                GetOutput();

                _inputUpdate = new Thread(InputUpdate)
                {
                    IsBackground = true
                };
                _inputUpdate.Start();

                button9.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #region Output

        private void button2_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 0, 1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 0, 2);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 0, 3);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 0, 4);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 0, 5);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 0, 6);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 0, 7);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 1, 0);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 1, 1);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 1, 2);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 1, 3);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 1, 4);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 1, 5);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 1, 6);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 1, 7);
        }

        private void button21_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 2, 0);
        }

        private void button22_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 2, 1);
        }

        private void button23_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 2, 2);
        }

        private void button24_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 2, 3);
        }

        private void button25_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 2, 4);
        }

        private void button26_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 2, 5);
        }

        private void button27_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 2, 6);
        }

        private void button28_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 2, 7);
        }

        private void button31_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 3, 0);
        }

        private void button32_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 3, 1);
        }

        private void button33_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 3, 2);
        }

        private void button34_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 3, 3);
        }

        private void button35_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 3, 4);
        }

        private void button36_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 3, 5);
        }

        private void button37_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 3, 6);
        }

        private void button38_Click(object sender, EventArgs e)
        {
            SetOutput(sender, 3, 7);
        } 
        #endregion
    }
}

