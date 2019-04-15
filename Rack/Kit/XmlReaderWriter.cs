using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;


namespace Rack
{
    public partial class XmlReaderWriter
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

        public static void Backup(string file, string basePath)
        {
            XElement root = XElement.Load(file);
            root.Save(basePath + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xml");
        }
        
    }
}
