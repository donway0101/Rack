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

        private readonly CqcRack _rack = new CqcRack("192.168.1.118");
        Tester tester = new Tester("192.168.1.12", 8080);
        Tester tester2 = new Tester("192.168.1.11", 8081);

        private void button1_Click(object sender, EventArgs e)
        {

            _rack.StartPhoneServer();
            _rack.PhoneServerManualResetEvent.Set();
            //tester.Start();
            //tester2.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tester.MessageReceived += Tester_MessageReceived;
        }

        private void Tester_MessageReceived(object sender, string message)
        {
            Console.WriteLine(message);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _rack.AddNewPhone();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _rack.AddPassPhone();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _rack.AddFailedPhone();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            _rack.ConveyorOnline = false;
            _rack.StepperOnline = false;
            _rack.Start();
            _rack.StartPhoneServer();
            _rack.PhoneServerManualResetEvent.Set();
            _rack.ShieldBoxSetupForSimulation();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            _rack.AddRetryPhone();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            _rack.AddGoldPhone();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            _rack.PhoneServerManualResetEvent.Reset();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            ShieldBox box = _rack.ConvertIdToShieldBox(comboBox1.SelectedIndex + 1);
            box.Phone.TestResult = TestResult.Pass;
            //Todo, in reality, it's true after box is opened.
            box.Available = true;
            _rack.AddPhoneToBeServed(box.Phone);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            ShieldBox box = _rack.ConvertIdToShieldBox(comboBox1.SelectedIndex + 1);
            box.Phone.TestResult = TestResult.Fail;
            box.Phone.FailCount++;
            box.Available = true;
            _rack.AddPhoneToBeServed(box.Phone);
        }
    }
}
