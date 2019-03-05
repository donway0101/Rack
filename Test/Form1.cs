using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ACS.SPiiPlusNET;
using IO;
using GripperStepper;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Api ch = new Api();
        EthercatIO IO;

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //ch.OpenCommEthernet("192.168.8.18", 701);
                IO = new EthercatIO(ch, 40, 3, 2);
                //IO.Setup();
                //IO.SetOutput(1, 6, false);
                //bool v = IO.GetInput(2, 6);
                //IO.SetOutput(Output.RedLight, true);
                ;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);

            }
            
        }

        Stepper stepper = new Stepper("COM3");

        private void button2_Click(object sender, EventArgs e)
        {
            stepper.Initialization();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //stepper.ResetAlarm(GripperMotor.One);
            stepper.ToPointAsync1(GripperMotor.One, 90, GripperMotor.Two, 60, 10);
        }
    }
}
