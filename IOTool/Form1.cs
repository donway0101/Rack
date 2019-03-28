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


namespace IOTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Api ch = new Api();
        EthercatIO IORobot;


        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                ch.OpenCommEthernet("192.168.8.18", 701);

                IORobot = new EthercatIO(ch, 72, 7, 3);

                IORobot.Setup();
                IORobot.SetOutput(1, 6, true);
                //bool v = IO.GetInput(2, 6);
                //IORobot.SetOutput(Output.RedLight, true);
                ;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
    }
}

