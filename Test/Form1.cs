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
using Motion;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Api ch = new Api();
        EthercatMotion Motion;

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ch.OpenCommEthernet("192.168.8.18", 701);
                Motion = new EthercatMotion(ch, 3);
                Motion.Setup();

           
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
                //Motion.Test();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString();
            Motion.SetSpeedImm(Convert.ToDouble(trackBar1.Value));
        }
    }
}
