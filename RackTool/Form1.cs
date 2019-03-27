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
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }           
        }

        CQCRack Rack = new CQCRack("192.168.8.18");

        private void button2_Click(object sender, EventArgs e)
        {
            
            try
            {
                Rack.HomeRobot();
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
            try
            {
                Rack.Test();
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
    }
}
