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
        #region Login
        public static void CreateLoginDataFile(string file)
        {
            XElement root;

            if (File.Exists(file))
                throw new Exception(file + " already exist.");
            //root = XElement.Load(file);
            else
                root = new XElement("LoginData");
            XElement AccoutOne = new XElement(LoginType.Accout.ToString());
            AccoutOne.Add(
                new XAttribute(LogicInformation.LoginName.ToString(), "Admin"),
                new XAttribute(LogicInformation.LoginPassWord.ToString(), "rSsjupHpsE8="),
                new XAttribute(LoginType.LogicType.ToString(), "Administrator")
            );

            XElement AccoutTwo = new XElement(LoginType.Accout.ToString());
            AccoutTwo.Add(
                new XAttribute(LogicInformation.LoginName.ToString(), "Technician"),
                new XAttribute(LogicInformation.LoginPassWord.ToString(), "rSsjupHpsE8="),
                new XAttribute(LoginType.LogicType.ToString(), "Technician")
            );
            XElement AccoutThree = new XElement(LoginType.Accout.ToString());
            AccoutThree.Add(
                new XAttribute(LogicInformation.LoginName.ToString(), "Operator"),
                new XAttribute(LogicInformation.LoginPassWord.ToString(), "rSsjupHpsE8="),
                new XAttribute(LoginType.LogicType.ToString(), "Operator")
            );
            root.Add(AccoutOne);
            root.Add(AccoutTwo);
            root.Add(AccoutThree);
            root.Save(file);
        }

        public static void SetLoginAttribute(string file, LoginType Type, LogicInformation attribute, string newValue)
        {
            XElement root = XElement.Load(file);

            XElement elem = root
                .Elements(LoginType.Accout.ToString())
                .Single(itemName => itemName.Attribute(LoginType.LogicType.ToString()).Value == Type.ToString());

            elem.Attribute(attribute.ToString()).Value = newValue;

            root.Save(file);
        }

        public static string GetLoginAttribute(string file, LoginType Type, LogicInformation attribute)
        {
            XElement root = XElement.Load(file);

            XElement elem = root
                .Elements(LoginType.Accout.ToString())
                .Single(itemName => itemName.Attribute(LoginType.LogicType.ToString()).Value == Type.ToString());

            return elem.Attribute(attribute.ToString()).Value;
        }
        #endregion
    }
}
