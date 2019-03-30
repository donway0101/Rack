﻿using System;
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
using Tools;

namespace RackTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Rack.Start();
                button2.Enabled = true;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }           
        }

        CqcRack Rack = new CqcRack("192.168.8.18");

        private void button2_Click(object sender, EventArgs e)
        {
            
            try
            {
                Task.Run(() => { Rack.HomeRobot();              
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

            Rack.SetSpeed(20);

            ////button3.Enabled = false;
            //Task.Run(()=> {
            //    do
            //    {
            //        try
            //        {
            //            Rack.Test();
            //        }
            //        catch (Exception ex)
            //        {

            //            MessageBox.Show(ex.Message);
            //        }
            //    } while (testLoop);

            //});


            Task.Run(() =>
            {

                try
                {
                    Rack.Test();
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
                Rack.Stop();     
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
            Rack.SetSpeedImm(Convert.ToDouble(trackBar1.Value));            
        }
    }
}
