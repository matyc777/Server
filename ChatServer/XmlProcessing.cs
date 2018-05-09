using System;
using System.Xml;
using System.IO;
using System.Xml.Linq;

namespace ChatServer
{
    class XmlProcessing
    {
        static void ReadXML()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("XML.xml");
            foreach (XmlNode node in doc.DocumentElement)
            {
                string name = node.Attributes[0].Value;
                string time = node["time"].InnerText;
                string nick = node["nick"].InnerText;
                Console.WriteLine("{0} ({1}, from {2})", name, time, nick);
            }
            Console.ReadKey();
        }

        public static void WriteXML(string path, string message, string sender, string times)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.Load(path + @"\ChatHistory.xml");
                XmlNode root = doc.DocumentElement;
                XmlNode elem = doc.CreateElement("message");
                XmlAttribute attribute = doc.CreateAttribute("text");
                attribute.Value = message;
                elem.Attributes.Append(attribute);
                XmlNode time = doc.CreateElement("time");
                time.InnerText = times;
                elem.AppendChild(time);
                XmlNode nick = doc.CreateElement("nick");
                nick.InnerText = sender;
                elem.AppendChild(nick);
                root.InsertAfter(elem, root.FirstChild);
                doc.Save(path  + @"\ChatHistory.xml");
            }
            catch
            {
                XmlDocument document = new XmlDocument();
                document.CreateXmlDeclaration("1.0", "utf-8", null);
                XmlNode root = document.CreateElement("chathistory");
                document.AppendChild(root);
                XmlNode theme = document.CreateElement("message");
                document.DocumentElement.AppendChild(theme);
                XmlAttribute attribute = document.CreateAttribute("text");
                attribute.Value = message;
                theme.Attributes.Append(attribute);
                XmlNode time = document.CreateElement("time");
                time.InnerText = times;
                theme.AppendChild(time);
                XmlNode nick = document.CreateElement("nick");
                nick.InnerText = sender;
                theme.AppendChild(nick);
                document.Save(path + @"\ChatHistory.xml");
            }
        }
    }
}