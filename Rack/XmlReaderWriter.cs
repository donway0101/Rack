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

        public static string GetTeachAttribute(string file, TeachData PosName, TeachData attribute)
        {
            XElement root = XElement.Load(file);

            XElement elem = root
              .Elements(TeachData.Teach.ToString())
              .Elements(TeachData.Pos.ToString())
              .Single(itemName => itemName.Attribute(TeachData.Name.ToString()).Value == PosName.ToString())
              .Parent
              .Element(TeachData.Pos.ToString());

            return elem.Attribute(attribute.ToString()).Value;
        }

        public static void SetShiledBoxAttribute(string file, int id, ShiledBoxData attribute, object value)
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

            if (File.Exists(file))
                throw new Exception(file + " already exist.");
                //root = XElement.Load(file);
            else
                root = new XElement(ShiledBoxData.RackData.ToString());

            #region ShiledBox
            XElement ShiledBox = new XElement(ShiledBoxData.ShiledBox.ToString());

            XElement Box1 = new XElement(ShiledBoxData.Box.ToString());
            Box1.Add(
                new XAttribute(ShiledBoxData.Id.ToString(), 1),
                new XAttribute(ShiledBoxData.CarrierHeight.ToString(), 600),
                new XAttribute(ShiledBoxData.DoorHeight .ToString(), 750)
                );

            XElement Box2 = new XElement(ShiledBoxData.Box.ToString());
            Box2.Add(
                new XAttribute(ShiledBoxData.Id.ToString(), 2),
                new XAttribute(ShiledBoxData.CarrierHeight.ToString(), 600),
                new XAttribute(ShiledBoxData.DoorHeight.ToString(), 750)         
                );

            ShiledBox.Add(Box1);
            ShiledBox.Add(Box2);

            #endregion

            XElement Conveyor = new XElement(ConveyorData.Conveyor.ToString());
            Conveyor.Add(new XAttribute(ConveyorData.PickConveyorHeight.ToString(), 550));
            Conveyor.Add(new XAttribute(ConveyorData.BinConveyorHeight.ToString(), 650));

            #region Teach
            XElement Teach = new XElement(TeachData.Teach.ToString());

            XElement PickPos = new XElement(TeachData.Pos.ToString());
            PickPos.Add(
                new XAttribute(TeachData.Name.ToString(), TeachData.Pick.ToString()),
                new XAttribute(TeachData.XPos.ToString(), 0),
                new XAttribute(TeachData.YPos.ToString(), 0),
                new XAttribute(TeachData.ZPos.ToString(), 1000)
                );

            XElement BinPos = new XElement(TeachData.Pos.ToString());
            BinPos.Add(
                     new XAttribute(TeachData.Name.ToString(), TeachData.Bin.ToString()),
                     new XAttribute(TeachData.XPos.ToString(), 0),
                     new XAttribute(TeachData.YPos.ToString(), 0),
                     new XAttribute(TeachData.ZPos.ToString(), 1000)
                     );

            XElement Holder1 = new XElement(TeachData.Pos.ToString());
            Holder1.Add(
                     new XAttribute(TeachData.Name.ToString(), TeachData.Holder1.ToString()),
                     new XAttribute(TeachData.XPos.ToString(), 0),
                     new XAttribute(TeachData.YPos.ToString(), 0),
                     new XAttribute(TeachData.ZPos.ToString(), 1000)
                     );

            XElement Holder2 = new XElement(TeachData.Pos.ToString());
            Holder2.Add(
                     new XAttribute(TeachData.Name.ToString(), TeachData.Holder2.ToString()),
                     new XAttribute(TeachData.XPos.ToString(), 0),
                     new XAttribute(TeachData.YPos.ToString(), 0),
                     new XAttribute(TeachData.ZPos.ToString(), 1000)
                     );

            XElement Holder3 = new XElement(TeachData.Pos.ToString());
            Holder3.Add(
                     new XAttribute(TeachData.Name.ToString(), TeachData.Holder3.ToString()),
                     new XAttribute(TeachData.XPos.ToString(), 0),
                     new XAttribute(TeachData.YPos.ToString(), 0),
                     new XAttribute(TeachData.ZPos.ToString(), 1000)
                     );

            XElement Holder4 = new XElement(TeachData.Pos.ToString());
            Holder4.Add(
                     new XAttribute(TeachData.Name.ToString(), TeachData.Holder4.ToString()),
                     new XAttribute(TeachData.XPos.ToString(), 0),
                     new XAttribute(TeachData.YPos.ToString(), 0),
                     new XAttribute(TeachData.ZPos.ToString(), 1000)
                     );

            XElement Holder5 = new XElement(TeachData.Pos.ToString());
            Holder5.Add(
                     new XAttribute(TeachData.Name.ToString(), TeachData.Holder5.ToString()),
                     new XAttribute(TeachData.XPos.ToString(), 0),
                     new XAttribute(TeachData.YPos.ToString(), 0),
                     new XAttribute(TeachData.ZPos.ToString(), 1000)
                     );

            XElement Holder6 = new XElement(TeachData.Pos.ToString());
            Holder6.Add(
                     new XAttribute(TeachData.Name.ToString(), TeachData.Holder6.ToString()),
                     new XAttribute(TeachData.XPos.ToString(), 0),
                     new XAttribute(TeachData.YPos.ToString(), 0),
                     new XAttribute(TeachData.ZPos.ToString(), 1000)
                     );

            XElement Gold1 = new XElement(TeachData.Pos.ToString());
            Gold1.Add(
                     new XAttribute(TeachData.Name.ToString(), TeachData.Gold1.ToString()),
                     new XAttribute(TeachData.XPos.ToString(), 0),
                     new XAttribute(TeachData.YPos.ToString(), 0),
                     new XAttribute(TeachData.ZPos.ToString(), 1000)
                     );

            XElement Gold2 = new XElement(TeachData.Pos.ToString());
            Gold2.Add(
                     new XAttribute(TeachData.Name.ToString(), TeachData.Gold2.ToString()),
                     new XAttribute(TeachData.XPos.ToString(), 0),
                     new XAttribute(TeachData.YPos.ToString(), 0),
                     new XAttribute(TeachData.ZPos.ToString(), 1000)
                     );

            XElement Gold3 = new XElement(TeachData.Pos.ToString());
            Gold3.Add(
                     new XAttribute(TeachData.Name.ToString(), TeachData.Gold3.ToString()),
                     new XAttribute(TeachData.XPos.ToString(), 0),
                     new XAttribute(TeachData.YPos.ToString(), 0),
                     new XAttribute(TeachData.ZPos.ToString(), 1000)
                     );

            XElement Gold4 = new XElement(TeachData.Pos.ToString());
            Gold4.Add(
                     new XAttribute(TeachData.Name.ToString(), TeachData.Gold4.ToString()),
                     new XAttribute(TeachData.XPos.ToString(), 0),
                     new XAttribute(TeachData.YPos.ToString(), 0),
                     new XAttribute(TeachData.ZPos.ToString(), 1000)
                     );

            XElement Gold5 = new XElement(TeachData.Pos.ToString());
            Gold5.Add(
                     new XAttribute(TeachData.Name.ToString(), TeachData.Gold5.ToString()),
                     new XAttribute(TeachData.XPos.ToString(), 0),
                     new XAttribute(TeachData.YPos.ToString(), 0),
                     new XAttribute(TeachData.ZPos.ToString(), 1000)
                     );

            Teach.Add(PickPos);
            Teach.Add(BinPos);
            Teach.Add(Holder1);
            Teach.Add(Holder2);
            Teach.Add(Holder3);
            Teach.Add(Holder4);
            Teach.Add(Holder5);
            Teach.Add(Holder6);

            Teach.Add(Gold1);
            Teach.Add(Gold2);
            Teach.Add(Gold3);
            Teach.Add(Gold4);
            Teach.Add(Gold5);

            #endregion


            root.Add(ShiledBox);
            root.Add(Conveyor);
            root.Add(Teach);

            root.Save(file);
        }
    }
}
