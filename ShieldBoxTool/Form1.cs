using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShieldBox;

namespace ShieldBoxTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        BpShieldBox shieldBox = new BpShieldBox(1,"COM5");

        private void button1_Click(object sender, EventArgs e)
        {
            shieldBox.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            shieldBox.OpenBox();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            shieldBox.CloseBox();
        }
    }
}
