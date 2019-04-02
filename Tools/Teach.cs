using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Tools
{
    public partial class Teach : Form
    {
        public Teach()
        {
            InitializeComponent();
        }

        private string _file = @"D:\BP\ChongqingRack\Rack\Rack\RackTool\bin\Debug\RackData.xml";
        private string _basePath = @"D:\BP\ChongqingRack\Rack\Rack\RackTool\bin\Debug\RackDataBackup\";
        private TeachPos _selectPos;

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            foreach (var pos in Enum.GetValues(typeof(TeachPos)))
            {
                comboBox1.Items.Add(pos);
            }
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectPos = (TeachPos)comboBox1.SelectedItem;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            XmlReaderWriter.SetTeachAttribute(_file, TeachPos.Pick, PosItem.APos, "888");
            XmlReaderWriter.Backup(_file, _basePath);
        }

        private void SavePosition()
        {
            
        }
    }
}
