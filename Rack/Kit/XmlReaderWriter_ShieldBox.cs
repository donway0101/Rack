using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rack
{
    public partial class XmlReaderWriter
    {
        #region ShieldBox
        public static void CreateBoxDataFile(string file)
        {
            XElement root;

            if (File.Exists(file))
                throw new Exception(file + " already exist.");
            //root = XElement.Load(file);
            else
                root = new XElement("BoxData");
            XElement Box1 = new XElement(ShieldBoxItem.ShieldBox.ToString());
            Box1.Add(
                new XAttribute(ShieldBoxItem.BoxId.ToString(), 1),
                new XAttribute(ShieldBoxItem.COM.ToString(), "COM1"),
                new XAttribute(ShieldBoxItem.State.ToString(), "Enable"),
                new XAttribute(ShieldBoxItem.Type.ToString(), ShieldBoxType.BT.ToString()),
                new XAttribute(ShieldBoxItem.Label.ToString(), LabelType.A),
                new XAttribute(ShieldBoxItem.Ip.ToString(), "None"),
                new XAttribute(ShieldBoxItem.Port.ToString(), "None")
                );

            XElement Box2 = new XElement(ShieldBoxItem.ShieldBox.ToString());
            Box2.Add(
                new XAttribute(ShieldBoxItem.BoxId.ToString(), 2),
                new XAttribute(ShieldBoxItem.COM.ToString(), "COM1"),
                new XAttribute(ShieldBoxItem.State.ToString(), "Enable"),
                new XAttribute(ShieldBoxItem.Type.ToString(), ShieldBoxType.BT.ToString()),
                new XAttribute(ShieldBoxItem.Label.ToString(), LabelType.A),
                new XAttribute(ShieldBoxItem.Ip.ToString(), "None"),
                new XAttribute(ShieldBoxItem.Port.ToString(), "None")
                );
            XElement Box3 = new XElement(ShieldBoxItem.ShieldBox.ToString());
            Box3.Add(
                new XAttribute(ShieldBoxItem.BoxId.ToString(), 3),
                new XAttribute(ShieldBoxItem.COM.ToString(), "COM1"),
                new XAttribute(ShieldBoxItem.State.ToString(), "Enable"),
                new XAttribute(ShieldBoxItem.Type.ToString(), ShieldBoxType.BT.ToString()),
                new XAttribute(ShieldBoxItem.Label.ToString(), LabelType.A),
                new XAttribute(ShieldBoxItem.Ip.ToString(), "None"),
                new XAttribute(ShieldBoxItem.Port.ToString(), "None")
                );

            XElement Box4 = new XElement(ShieldBoxItem.ShieldBox.ToString());
            Box4.Add(
                new XAttribute(ShieldBoxItem.BoxId.ToString(), 4),
                new XAttribute(ShieldBoxItem.COM.ToString(), "COM1"),
                new XAttribute(ShieldBoxItem.State.ToString(), "Enable"),
                new XAttribute(ShieldBoxItem.Type.ToString(), ShieldBoxType.BT.ToString()),
                new XAttribute(ShieldBoxItem.Label.ToString(), LabelType.A),
                new XAttribute(ShieldBoxItem.Ip.ToString(), "None"),
                new XAttribute(ShieldBoxItem.Port.ToString(), "None")
                );
            XElement Box5 = new XElement(ShieldBoxItem.ShieldBox.ToString());
            Box5.Add(
                new XAttribute(ShieldBoxItem.BoxId.ToString(), 5),
                new XAttribute(ShieldBoxItem.COM.ToString(), "COM1"),
                new XAttribute(ShieldBoxItem.State.ToString(), "Enable"),
                new XAttribute(ShieldBoxItem.Type.ToString(), ShieldBoxType.BT.ToString()),
                new XAttribute(ShieldBoxItem.Label.ToString(), LabelType.A),
                new XAttribute(ShieldBoxItem.Ip.ToString(), "None"),
                new XAttribute(ShieldBoxItem.Port.ToString(), "None")
                );
            XElement Box6 = new XElement(ShieldBoxItem.ShieldBox.ToString());
            Box6.Add(
                new XAttribute(ShieldBoxItem.BoxId.ToString(), 6),
                new XAttribute(ShieldBoxItem.COM.ToString(), "COM1"),
                new XAttribute(ShieldBoxItem.State.ToString(), "Enable"),
                new XAttribute(ShieldBoxItem.Type.ToString(), ShieldBoxType.BT.ToString()),
                new XAttribute(ShieldBoxItem.Label.ToString(), LabelType.A),
                new XAttribute(ShieldBoxItem.Ip.ToString(), "None"),
                new XAttribute(ShieldBoxItem.Port.ToString(), "None")
                );
            root.Add(Box1);
            root.Add(Box2);
            root.Add(Box3);
            root.Add(Box4);
            root.Add(Box5);
            root.Add(Box6);
            root.Save(file);
        }

        public static void SetBoxAttribute(string file, int BoxId, ShieldBoxItem attribute, string newValue)
        {
            XElement root = XElement.Load(file);

            XElement elem = root
                .Elements(ShieldBoxItem.ShieldBox.ToString())
                .Single(itemName => itemName.Attribute(ShieldBoxItem.BoxId.ToString()).Value == BoxId.ToString());

            elem.Attribute(attribute.ToString()).Value = newValue;

            root.Save(file);
        }

        public static string GetBoxAttribute(string file, int BoxId, ShieldBoxItem attribute)
        {
            XElement root = XElement.Load(file);

            XElement elem = root
              .Elements(ShieldBoxItem.ShieldBox.ToString())
              .Single(itemName => itemName.Attribute(ShieldBoxItem.BoxId.ToString()).Value == BoxId.ToString());

            return elem.Attribute(attribute.ToString()).Value;
        } 
        #endregion
    }
}
