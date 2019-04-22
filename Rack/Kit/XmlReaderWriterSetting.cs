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
        public static void CreateEmptySettingFile(string file = Files.RackSetting)
        {
            XElement root = new XElement("RackSetting");
            XElement conveyorElement = new XElement(RackSetting.ConveyorSetting.ToString());
            conveyorElement.Add(new XAttribute(RackSetting.ConveyorMovingForward.ToString(), false));

            root.Add(conveyorElement);
            root.Save(file);
        }

        public static void SetConveyorSetting(RackSetting setting, string value, string file = Files.RackSetting)
        {
            XElement root = XElement.Load(file);

            XElement elem = root
                .Elements(RackSetting.ConveyorSetting.ToString())
                .Single(itemName => itemName.Attribute(RackSetting.ConveyorMovingForward.ToString()).Name == setting.ToString());

            elem.Attribute(setting.ToString()).Value = value;
            root.Save(file);
        }

        public static string GetConveyorSetting(RackSetting setting, string file = Files.RackSetting)
        {
            XElement root = XElement.Load(file);

            XElement elem = root
                .Elements(RackSetting.ConveyorSetting.ToString())
                .Single(itemName => itemName.Attribute(RackSetting.ConveyorMovingForward.ToString()).Name == setting.ToString());

            return elem.Attribute(setting.ToString()).Value;
        }
    }
}
