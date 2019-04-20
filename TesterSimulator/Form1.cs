using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TesterSimulator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private SocketClient _client1 = new SocketClient(1001);
        private SocketClient _client2 = new SocketClient(1002);
        private SocketClient _client3 = new SocketClient(1003);
        private SocketClient _client4 = new SocketClient(1004);
        private SocketClient _client5 = new SocketClient(1005);
        private SocketClient _client6 = new SocketClient(1006);
        private SocketClient _selectedSocketClient = null;

        private void button1_Click(object sender, EventArgs e)
        {
            _selectedSocketClient = _client1;
            _client1.Start();
            _client2.Start();
            _client3.Start();
            _client4.Start();
            _client5.Start();
            _client6.Start();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            _selectedSocketClient = _client1;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            _selectedSocketClient = _client2;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            _selectedSocketClient = _client3;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            _selectedSocketClient = _client4;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            _selectedSocketClient = _client5;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            _selectedSocketClient = _client6;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _selectedSocketClient.SetPass();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _selectedSocketClient.SetFail();
        }
    }
}
