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
using IO;


namespace IOTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private readonly Api _ch = new Api();
        private static EthercatIO _ioRobot;
        private Thread _inputUpdate;


        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                _ch.OpenCommEthernet("192.168.8.18", 701);

                _ioRobot = new EthercatIO(_ch, 72, 7, 4);

                _ioRobot.Setup();

                //_ioRobot.SetOutput(1, 6, true);
                //bool v = IO.GetInput(2, 6);
                //IORobot.SetOutput(Output.RedLight, true);

                _inputUpdate = new Thread(InputUpdate)
                {
                    IsBackground = true
                };
                _inputUpdate.Start();

                button1.Enabled = false;
                

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

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
                    Thread.Sleep(1000);
                    Invoke(button1, () => { button1.Enabled = true; });
                    break;
                    //throw;
                }
            }
        }

        public static void Invoke(Control control, MethodInvoker action)
        {
            control.Invoke(action);
        }

        public static void Invoke(Control control, int moduleId, int inputPinNum)
        {
            control.Invoke((MethodInvoker)(() => { control.Text = _ioRobot.GetInput(moduleId, inputPinNum).ToString(); }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Button bt = (Button) sender;
            //_ioRobot.SetOutput(0,0, Convert.ToBoolean(bt.Text));
            bt.Text = bt.Text == "true" ? "false" : "true";
        }
    }
}

