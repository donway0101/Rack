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
                stepper.HomeMotor(GripperMotor.Two, 0);
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

        private void button7_Click(object sender, EventArgs e)
        {
            stepper.Enable(GripperMotor.One);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            stepper.Enable(GripperMotor.Two);

        }

        private void button9_Click(object sender, EventArgs e)
        {
            stepper.Disable(GripperMotor.One);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            stepper.Disable(GripperMotor.Two);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            button10.Enabled = false;
            stepper.ToPoint(GripperMotor.One, Convert.ToDouble(textBox1.Text));
            button10.Enabled = true;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            button11.Enabled = false;
            stepper.ToPoint(GripperMotor.Two, Convert.ToDouble(textBox2.Text));
            button11.Enabled = true;            
        }

        int testTimes = 0;
        int successTimes = 0;
        private async void button12_Click(object sender, EventArgs e)
        {
            button12.Enabled = false;
            while (loop)
            {
                Console.WriteLine("Motor moving");
                Task<bool> a = stepper.ToPointAsync(GripperMotor.One, 90, GripperMotor.Two, 60, 10);
                Console.WriteLine("Doing other job");
                Thread.Sleep(100);
                Console.WriteLine("Doing other job");
                Thread.Sleep(100);
                Console.WriteLine("Waiting moving result");
                bool result = await a;
                Console.WriteLine("Result is " + result);

                Console.WriteLine("Motor moving");
                a = stepper.ToPointAsync(GripperMotor.One, 0, GripperMotor.Two, 0, 10);
                Console.WriteLine("Doing other job");
                Thread.Sleep(100);
                Console.WriteLine("Doing other job");
                Thread.Sleep(100);
                Console.WriteLine("Waiting moving result");
                bool result1 = await a;
                Console.WriteLine("Result is " + result1);

                testTimes++;
                if (result & result1)
                {
                    successTimes++;
                }
                label1.Text = successTimes + " of " + testTimes + " success";
            }
            button12.Enabled = true;
        }

        bool loop = false;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            loop = checkBox1.Checked;

        }
    }
}
