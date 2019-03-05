using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GripperStepper;

namespace StepperTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        Stepper stepper = new Stepper("COM3");

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                stepper.Initialization();
                stepper.Connect();
                button1.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                stepper.HomeMotor(GripperMotor.One, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                stepper.Stop(GripperMotor.One);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                stepper.Stop(GripperMotor.Two);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            try
            {
                stepper.HomeMotor(GripperMotor.Two, 20);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            try
            {
                stepper.Stop(GripperMotor.Two);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
