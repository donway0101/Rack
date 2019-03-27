﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;


namespace Tools
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

        public static string GetTeachAttribute(string file, TeachPos PosName, PosItem attribute)
        {
            XElement root = XElement.Load(file);

            XElement elem = root
              .Elements(PosItem.Teach.ToString())
              .Elements(PosItem.Pos.ToString())
              .Single(itemName => itemName.Attribute(PosItem.Name.ToString()).Value == PosName.ToString());

            return elem.Attribute(attribute.ToString()).Value;
        }

        public static void CreateStorageFile(string file)
        {
            XElement root;

            //if (File.Exists(file))
            //    throw new Exception(file + " already exist.");
            //    //root = XElement.Load(file);
            //else
                root = new XElement("RackData");

            #region Teach
            XElement Teach = new XElement(PosItem.Teach.ToString());
            const double defaultXPos = 800;
            const double defaultYPos = 50;
            const double defaultZPos = 720;
            const double defaultRPos = 26;
            const double defaultAPos = 0;
            const double defaultApproachHeight = 720;

            XElement HomePos = new XElement(PosItem.Pos.ToString());
            HomePos.Add(
                new XAttribute(PosItem.Name.ToString(), TeachPos.Home.ToString()),
                new XAttribute(PosItem.XPos.ToString(), 400),
                new XAttribute(PosItem.YPos.ToString(), 0),
                new XAttribute(PosItem.ZPos.ToString(), 720),
                new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                new XAttribute(PosItem.APos.ToString(), defaultAPos),
                new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                );

            XElement PickPos = new XElement(PosItem.Pos.ToString());
            PickPos.Add(
                new XAttribute(PosItem.Name.ToString(), TeachPos.Pick.ToString()),
                new XAttribute(PosItem.XPos.ToString(), 366),
                new XAttribute(PosItem.YPos.ToString(), 0),
                new XAttribute(PosItem.ZPos.ToString(), 300),
                new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                new XAttribute(PosItem.APos.ToString(), defaultAPos),
                new XAttribute(PosItem.ApproachHeight.ToString(), 300)
                );

            XElement BinPos = new XElement(PosItem.Pos.ToString());
            BinPos.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Bin.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 366),
                     new XAttribute(PosItem.YPos.ToString(), 150),
                     new XAttribute(PosItem.ZPos.ToString(), 600),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), 600)
                     );

            XElement ConveyorRight = new XElement(PosItem.Pos.ToString());
            ConveyorRight.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.ConveyorRight.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 745),
                     new XAttribute(PosItem.YPos.ToString(), defaultYPos),
                     new XAttribute(PosItem.ZPos.ToString(), defaultZPos),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     );

            XElement ConveyorLeft = new XElement(PosItem.Pos.ToString());
            ConveyorLeft.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.ConveyorLeft.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 40),
                     new XAttribute(PosItem.YPos.ToString(), defaultYPos),
                     new XAttribute(PosItem.ZPos.ToString(), defaultZPos),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     );

            #region ShieldBoxHolder

            XElement Holder1 = new XElement(PosItem.Pos.ToString());
            Holder1.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Holder1.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 1300),
                     new XAttribute(PosItem.YPos.ToString(), 300),
                     new XAttribute(PosItem.ZPos.ToString(), 670),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), 720)
                     );

            XElement Holder2 = new XElement(PosItem.Pos.ToString());
            Holder2.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Holder2.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 800),
                     new XAttribute(PosItem.YPos.ToString(), 300),
                     new XAttribute(PosItem.ZPos.ToString(), 670),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), 720)
                     );

            XElement Holder3 = new XElement(PosItem.Pos.ToString());
            Holder3.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Holder3.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 0),
                     new XAttribute(PosItem.YPos.ToString(), 300),
                     new XAttribute(PosItem.ZPos.ToString(), defaultZPos),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     );

            XElement Holder4 = new XElement(PosItem.Pos.ToString());
            Holder4.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Holder4.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 1300),
                     new XAttribute(PosItem.YPos.ToString(), 300),
                     new XAttribute(PosItem.ZPos.ToString(), 200),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), 250)
                     );

            XElement Holder5 = new XElement(PosItem.Pos.ToString());
            Holder5.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Holder5.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 800),
                     new XAttribute(PosItem.YPos.ToString(), 300),
                     new XAttribute(PosItem.ZPos.ToString(), 200),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), 250)
                     );

            XElement Holder6 = new XElement(PosItem.Pos.ToString());
            Holder6.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Holder6.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 0),
                     new XAttribute(PosItem.YPos.ToString(), 300),
                     new XAttribute(PosItem.ZPos.ToString(), 200),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), 250)
                     );
            #endregion

            #region GoldenPhone
            XElement Gold1 = new XElement(PosItem.Pos.ToString());
            Gold1.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Gold1.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), defaultXPos),
                     new XAttribute(PosItem.YPos.ToString(), defaultYPos),
                     new XAttribute(PosItem.ZPos.ToString(), defaultZPos),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     );

            XElement Gold2 = new XElement(PosItem.Pos.ToString());
            Gold2.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Gold2.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), defaultXPos),
                     new XAttribute(PosItem.YPos.ToString(), defaultYPos),
                     new XAttribute(PosItem.ZPos.ToString(), defaultZPos),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     );

            XElement Gold3 = new XElement(PosItem.Pos.ToString());
            Gold3.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Gold3.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), defaultXPos),
                     new XAttribute(PosItem.YPos.ToString(), defaultYPos),
                     new XAttribute(PosItem.ZPos.ToString(), defaultZPos),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     );

            XElement Gold4 = new XElement(PosItem.Pos.ToString());
            Gold4.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Gold4.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), defaultXPos),
                     new XAttribute(PosItem.YPos.ToString(), defaultYPos),
                     new XAttribute(PosItem.ZPos.ToString(), defaultZPos),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     );

            XElement Gold5 = new XElement(PosItem.Pos.ToString());
            Gold5.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Gold5.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), defaultXPos),
                     new XAttribute(PosItem.YPos.ToString(), defaultYPos),
                     new XAttribute(PosItem.ZPos.ToString(), defaultZPos),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     ); 
            #endregion

            Teach.Add(HomePos);
            Teach.Add(PickPos);
            Teach.Add(BinPos);
            Teach.Add(ConveyorLeft);
            Teach.Add(ConveyorRight);

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

            root.Add(Teach);

            root.Save(file);
        }
    }
}