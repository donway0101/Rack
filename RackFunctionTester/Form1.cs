using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rack;

namespace RackFunctionTester
{
    public partial class RackFunctionTester : Form
    {
        public RackFunctionTester()
        {
            InitializeComponent();
        }

        private readonly CqcRack _rack = new CqcRack("192.168.8.18");
        Tester tester = new Tester("192.168.1.12", 8080);
        Tester tester2 = new Tester("192.168.1.11", 8081);

        private void button1_Click(object sender, EventArgs e)
        {
            //_rack.StartPhoneServer();
            tester.Start();
            tester2.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tester.MessageReceived += Tester_MessageReceived;
        }

        private void Tester_MessageReceived(object sender, string message)
        {
            Console.WriteLine(message);
        }
    }
}
