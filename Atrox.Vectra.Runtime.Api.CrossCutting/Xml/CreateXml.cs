namespace CrossCutting.Xml
{
    using System.Xml;

    public static class CreateXml
    {
        public static void Xml(string xmlPath)
        {
            XmlDocument doc = new XmlDocument();

            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", null, null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement element1 = doc.CreateElement(string.Empty, "doc", string.Empty);
            doc.AppendChild(element1);

            XmlElement element2 = doc.CreateElement(string.Empty, "assembly", string.Empty);
            element1.AppendChild(element2);

            XmlElement element3 = doc.CreateElement(string.Empty, "name", string.Empty);
            XmlText text1 = doc.CreateTextNode("API.External.Atrox.Vectra.Runtime.Api");
            element3.AppendChild(text1);
            element2.AppendChild(element3);

            XmlElement element4 = doc.CreateElement(string.Empty, "members", string.Empty);
            element1.AppendChild(element4);

            XmlElement element5 = doc.CreateElement(string.Empty, "member", string.Empty);
            XmlAttribute atributo = doc.CreateAttribute("name");
            atributo.Value = "T:API.External.Atrox.Vectra.Runtime.Api.xml.Installers.CorsInstaller";
            element5.Attributes.Append(atributo);
            element4.AppendChild(element5);

            XmlElement element6 = doc.CreateElement(string.Empty, "summary", string.Empty);
            XmlText text2 = doc.CreateTextNode("Enable Cross-Origin Requests (CORS)");
            element6.AppendChild(text2);
            element5.AppendChild(element6);

            doc.Save(xmlPath);
        }
    }
}


