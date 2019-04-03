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

namespace RackTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private readonly CqcRack _rack = new CqcRack("192.168.8.18");
        private TeachPos _selectedTeachPos;

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                _rack.Start();
                SetupForTeaching();
                button2.Enabled = true;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }           
        }

        private void SetupForTeaching()
        {
            comboBox1.Items.Clear();
            foreach (var pos in Enum.GetValues(typeof(TeachPos)))
            {
                comboBox1.Items.Add(pos);
            }
        }
      
        private void button2_Click(object sender, EventArgs e)
        {
            
            try
            {
                Task.Run(() => { _rack.HomeRobot();              
                });
                //button2.Enabled = false;
                //button1.Enabled = false;
                //Rack.Test();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Test
        private void button3_Click(object sender, EventArgs e)
        {
            //DialogResult result = MessageBox.Show("是否所有屏蔽箱门都打开了？", "!!!", MessageBoxButtons.YesNo);
            //if (result == DialogResult.No)
            //{
            //    return;
            //}
            _rack.SetSpeed(20);

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
                _rack.SaveTeachPosition(_selectedTeachPos);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }          
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedTeachPos = (TeachPos)comboBox1.SelectedItem;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                //Todo is approach is lower than teach, then exception.
                _rack.SaveApproachHeight(_selectedTeachPos);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void button8_MouseDown(object sender, MouseEventArgs e)
        {
            _rack._motion.Jog(_rack._motion.MotorX1, false);
        }

        private void button8_MouseUp(object sender, MouseEventArgs e)
        {
            _rack._motion.Halt(_rack._motion.MotorX1);
        }

        private void button9_MouseDown(object sender, MouseEventArgs e)
        {
            _rack._motion.Jog(_rack._motion.MotorX1, true);
        }

        private void button9_MouseUp(object sender, MouseEventArgs e)
        {
            _rack._motion.Halt(_rack._motion.MotorX1);
        }
    }
}
