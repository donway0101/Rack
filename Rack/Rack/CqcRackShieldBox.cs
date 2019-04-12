using System;
using System.Threading;

namespace Rack
{
    public partial class CqcRack
    {
        private void ShieldBoxSetup()
        {
            ShieldBox1 = new ShieldBox(1);
            ShieldBox2 = new ShieldBox(2);
            ShieldBox3 = new ShieldBox(3);
            ShieldBox4 = new ShieldBox(4);
            ShieldBox5 = new ShieldBox(5);
            ShieldBox6 = new ShieldBox(6);

            ShieldBoxs = new ShieldBox[6] { ShieldBox1, ShieldBox2, ShieldBox3, ShieldBox4, ShieldBox5, ShieldBox6 };

            foreach (var box in ShieldBoxs)
            {
                box.PortName = XmlReaderWriter.GetBoxAttribute(Files.BoxData, box.Id, ShieldBoxItem.COM);
                box.Enabled = XmlReaderWriter.GetBoxAttribute(Files.BoxData, box.Id, ShieldBoxItem.State) == "Enable";
                if (box.Enabled)
                {
                    try
                    {
                        box.GreenLight();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Box " + box.Id + " is enabled but can't communicate: " + e.Message);
                    }
                }
                //Todo different type of shield box.
                //XmlReaderWriter.GetBoxAttribute(Files.BoxData, 1, ShieldBoxItem.Type);
            }
        }
 
        public void OpenAllBox()
        {
            foreach (var box in ShieldBoxs)
            {
                if (box.Enabled)
                {
                    box.OpenBox();
                }
            }
        }

       

 

    }
}
