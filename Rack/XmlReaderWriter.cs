using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Rack
{
    public class XmlReaderWriter
    {
        private XmlReader Reader { get; set; }

        public static IEnumerable<XElement> SimpleStreamAxis(string inputUrl,
                                              string elementName)
        {
            using (XmlReader reader = XmlReader.Create(inputUrl))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == elementName)
                        {
                            XElement el = XNode.ReadFrom(reader) as XElement;
                            if (el != null)
                            {
                                yield return el;
                            }
                        }
                    }
                }
            }
        }

        public static void Save(string file)
        {
            XElement root;

            if (File.Exists(file))
                root = XElement.Load(file);
            else
                root = new XElement("Devices");

            XElement bookParticipants =
                new XElement("Boxs",
                new XElement("Box",
                new XAttribute("Id", "1"),
                new XAttribute("CarrierHeight", "600"),
                new XAttribute("DoorHeight", "750"),
                new XAttribute("XPosition", "100"),
                new XAttribute("YPosition", "300"),
                new XElement("FirstName", "J"),
                new XElement("LastName", "Ri")));

            

            root.Add(bookParticipants);
            root.Save(file);
        }
    }
}
