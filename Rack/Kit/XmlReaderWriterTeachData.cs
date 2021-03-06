﻿using System;
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
        #region RackData
        public static string GetTeachAttribute(string file, TeachPos PosName, PosItem attribute)
        {
            XElement root = XElement.Load(file);

            var child1 = root.Elements(PosItem.Teach.ToString());
            var child2 = child1.Elements(PosItem.Pos.ToString());
            var elem = child2.Single(element => element.Attribute(PosItem.Name.ToString()).Value == PosName.ToString());

            //XElement elem = root
            //  .Elements(PosItem.Teach.ToString())
            //  .Elements(PosItem.Pos.ToString())
            //  .Single(element => element.Attribute(PosItem.Name.ToString()).Value == PosName.ToString());

            return elem.Attribute(attribute.ToString()).Value;
        }

        public static void SetTeachAttribute(string file, TeachPos posName, PosItem attribute, string newValue)
        {
            XElement root = XElement.Load(file);

            XElement elem = root
                .Elements(PosItem.Teach.ToString())
                .Elements(PosItem.Pos.ToString())
                .Single(itemName => itemName.Attribute(PosItem.Name.ToString()).Value == posName.ToString());

            elem.Attribute(attribute.ToString()).Value = newValue;

            root.Save(file);
        }

        public static void CreateStorageFile(string file)
        {
            XElement root;

            if (File.Exists(file))
                throw new Exception(file + " already exist.");
            //root = XElement.Load(file);
            else
                root = new XElement("RackData");

            #region Teach

            XElement Teach = new XElement(PosItem.Teach.ToString());
            const double defaultXPos = 400;
            const double defaultYPos = 0;
            const double defaultZPos = 715;
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
                new XAttribute(PosItem.ZPos.ToString(), 290),
                new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                new XAttribute(PosItem.APos.ToString(), 90),
                new XAttribute(PosItem.ApproachHeight.ToString(), 300)
                );

            XElement BinPos = new XElement(PosItem.Pos.ToString());
            BinPos.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Bin.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 366),
                     new XAttribute(PosItem.YPos.ToString(), 30),
                     new XAttribute(PosItem.ZPos.ToString(), 470),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), 480)
                     );

            XElement conveyorRight = new XElement(PosItem.Pos.ToString());
            conveyorRight.Add(
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
                     new XAttribute(PosItem.Name.ToString(), TeachPos.ShieldBox1.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 1300),
                     new XAttribute(PosItem.YPos.ToString(), defaultYPos),
                     new XAttribute(PosItem.ZPos.ToString(), 670),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     );

            XElement Holder2 = new XElement(PosItem.Pos.ToString());
            Holder2.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.ShieldBox2.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 800),
                     new XAttribute(PosItem.YPos.ToString(), defaultYPos),
                     new XAttribute(PosItem.ZPos.ToString(), defaultZPos),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     );

            XElement Holder3 = new XElement(PosItem.Pos.ToString());
            Holder3.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.ShieldBox3.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 0),
                     new XAttribute(PosItem.YPos.ToString(), defaultYPos),
                     new XAttribute(PosItem.ZPos.ToString(), defaultZPos),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     );

            XElement Holder4 = new XElement(PosItem.Pos.ToString());
            Holder4.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.ShieldBox4.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 1300),
                     new XAttribute(PosItem.YPos.ToString(), defaultYPos),
                     new XAttribute(PosItem.ZPos.ToString(), defaultZPos),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     );

            XElement Holder5 = new XElement(PosItem.Pos.ToString());
            Holder5.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.ShieldBox5.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 800),
                     new XAttribute(PosItem.YPos.ToString(), defaultYPos),
                     new XAttribute(PosItem.ZPos.ToString(), defaultZPos),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     );

            XElement Holder6 = new XElement(PosItem.Pos.ToString());
            Holder6.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.ShieldBox6.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), 0),
                     new XAttribute(PosItem.YPos.ToString(), defaultYPos),
                     new XAttribute(PosItem.ZPos.ToString(), defaultZPos),
                     new XAttribute(PosItem.RPos.ToString(), defaultRPos),
                     new XAttribute(PosItem.APos.ToString(), defaultAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), defaultApproachHeight)
                     );
            #endregion

            #region GoldenPhone

            double baseXPos = 1330;
            double incXpos = 130;
            double GoldYPos = 0;
            double goldApproach = 720;
            double goldZPos = 720;
            double goldRPos = 26;
            double goldAPos = 0;

            XElement Gold1 = new XElement(PosItem.Pos.ToString());
            Gold1.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Gold1.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), baseXPos - incXpos * 0),
                     new XAttribute(PosItem.YPos.ToString(), GoldYPos),
                     new XAttribute(PosItem.ZPos.ToString(), goldZPos),
                     new XAttribute(PosItem.RPos.ToString(), goldRPos),
                     new XAttribute(PosItem.APos.ToString(), goldAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), goldApproach)
                     );

            XElement Gold2 = new XElement(PosItem.Pos.ToString());
            Gold2.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Gold2.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), baseXPos - incXpos * 1),
                     new XAttribute(PosItem.YPos.ToString(), GoldYPos),
                     new XAttribute(PosItem.ZPos.ToString(), goldZPos),
                     new XAttribute(PosItem.RPos.ToString(), goldRPos),
                     new XAttribute(PosItem.APos.ToString(), goldAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), goldApproach)
                     );

            XElement Gold3 = new XElement(PosItem.Pos.ToString());
            Gold3.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Gold3.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), baseXPos - incXpos * 2),
                     new XAttribute(PosItem.YPos.ToString(), GoldYPos),
                     new XAttribute(PosItem.ZPos.ToString(), goldZPos),
                     new XAttribute(PosItem.RPos.ToString(), goldRPos),
                     new XAttribute(PosItem.APos.ToString(), goldAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), goldApproach)
                     );

            XElement Gold4 = new XElement(PosItem.Pos.ToString());
            Gold4.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Gold4.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), baseXPos - incXpos * 3),
                     new XAttribute(PosItem.YPos.ToString(), GoldYPos),
                     new XAttribute(PosItem.ZPos.ToString(), goldZPos),
                     new XAttribute(PosItem.RPos.ToString(), goldRPos),
                     new XAttribute(PosItem.APos.ToString(), goldAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), goldApproach)
                     );

            XElement Gold5 = new XElement(PosItem.Pos.ToString());
            Gold5.Add(
                     new XAttribute(PosItem.Name.ToString(), TeachPos.Gold5.ToString()),
                     new XAttribute(PosItem.XPos.ToString(), baseXPos - incXpos * 4),
                     new XAttribute(PosItem.YPos.ToString(), GoldYPos),
                     new XAttribute(PosItem.ZPos.ToString(), goldZPos),
                     new XAttribute(PosItem.RPos.ToString(), goldRPos),
                     new XAttribute(PosItem.APos.ToString(), goldAPos),
                     new XAttribute(PosItem.ApproachHeight.ToString(), goldApproach)
                     );
            #endregion

            XElement g1ToG2Offset = new XElement(PosItem.Pos.ToString());
            g1ToG2Offset.Add(
                new XAttribute(PosItem.Name.ToString(), TeachPos.G1ToG2Offset.ToString()),
                new XAttribute(PosItem.XPos.ToString(), 2),
                new XAttribute(PosItem.YPos.ToString(), 2),
                new XAttribute(PosItem.ZPos.ToString(), 720),
                new XAttribute(PosItem.RPos.ToString(), 30),
                new XAttribute(PosItem.APos.ToString(), 2),
                new XAttribute(PosItem.ApproachHeight.ToString(), 720)
            );

            XElement pickOffset = new XElement(PosItem.Pos.ToString());
            pickOffset.Add(
                new XAttribute(PosItem.Name.ToString(), TeachPos.PickOffset.ToString()),
                new XAttribute(PosItem.XPos.ToString(), 1.5),
                new XAttribute(PosItem.YPos.ToString(), 0),
                new XAttribute(PosItem.ZPos.ToString(), 720),
                new XAttribute(PosItem.RPos.ToString(), 30),
                new XAttribute(PosItem.APos.ToString(), 90),
                new XAttribute(PosItem.ApproachHeight.ToString(), 720)
            );

            #endregion

            #region AddElements
            Teach.Add(HomePos);
            Teach.Add(PickPos);
            Teach.Add(BinPos);
            Teach.Add(ConveyorLeft);
            Teach.Add(conveyorRight);

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

            Teach.Add(g1ToG2Offset);
            Teach.Add(pickOffset);
            #endregion

            root.Add(Teach);
            root.Save(file);
        } 
        #endregion
    }
}
