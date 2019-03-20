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


        public static IEnumerable<XElement> GetElements(string file, string elementName)
        {
            using (XmlReader reader = XmlReader.Create(file))
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

        public static XElement GetElement(string file, string elementName)
        {
            using (XmlReader reader = XmlReader.Create(file))
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
                                return el;
                            }
                        }
                    }
                }
                return null;
            }
        }

        public static string GetShiledBoxAttribute(string file, int id, ShiledBoxData attribute)
        {
            XElement root = XElement.Load(file);

            XElement elem = root
              .Elements(ShiledBoxData.ShiledBox.ToString())
              .Elements(ShiledBoxData.Box.ToString())
              .Single(itemName => itemName.Attribute(ShiledBoxData.Id.ToString()).Value == id.ToString())
              .Parent
              .Element(ShiledBoxData.Box.ToString());

            return elem.Attribute(attribute.ToString()).Value;
        }

        public static void SetShiledBox(string file, int id, ShiledBoxData attribute, object value)
        {
            XElement root = XElement.Load(file);

            XElement elem = root
              .Elements(ShiledBoxData.ShiledBox.ToString())
              .Elements(ShiledBoxData.Box.ToString())
              .Single(itemName => itemName.Attribute(ShiledBoxData.Id.ToString()).Value == id.ToString())
              .Parent
              .Element(ShiledBoxData.Box.ToString());

            elem.Attribute(attribute.ToString()).Value = value.ToString();

            root.Save(file);
        }

        public static void CreateStorageFile(string file)
        {
            XElement root;

            //if (File.Exists(file))
            //    root = XElement.Load(file);
            //else
            root = new XElement(ShiledBoxData.RackData.ToString());

            #region ShiledBox
            XElement ShiledBox = new XElement(ShiledBoxData.ShiledBox.ToString());

            XElement Box1 = new XElement(ShiledBoxData.Box.ToString());
            Box1.Add(
                new XAttribute(ShiledBoxData.Id.ToString(), 1),
                new XAttribute(ShiledBoxData.CarrierHeight.ToString(), 600),
                new XAttribute(ShiledBoxData.DoorHeight .ToString(), 750),
                new XAttribute(ShiledBoxData.XPosition.ToString(), 100),
                new XAttribute(ShiledBoxData.YPosition.ToString(), 300)
                );

            XElement Box2 = new XElement(ShiledBoxData.Box.ToString());
            Box2.Add(
                new XAttribute(ShiledBoxData.Id.ToString(), 2),
                new XAttribute(ShiledBoxData.CarrierHeight.ToString(), 600),
                new XAttribute(ShiledBoxData.DoorHeight.ToString(), 750),
                new XAttribute(ShiledBoxData.XPosition.ToString(), 100),
                new XAttribute(ShiledBoxData.YPosition.ToString(), 300)
                );

            ShiledBox.Add(Box1);
            ShiledBox.Add(Box2);

            #endregion

            XElement Conveyor = new XElement(ConveyorData.Conveyor.ToString());
            Conveyor.Add(new XAttribute(ConveyorData.PickConveyorHeight.ToString(), 550));
            Conveyor.Add(new XAttribute(ConveyorData.BinConveyorHeight.ToString(), 650));



            root.Add(ShiledBox);
            root.Add(Conveyor);

            root.Save(file);
        }
    }
}
