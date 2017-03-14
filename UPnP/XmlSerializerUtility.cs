using System.Xml;
using System.Xml.Serialization;

namespace UPnP
{
    /// <summary>
    /// Utility class for XML serialization
    /// </summary>
    public static class XmlSerializerUtility
    {
        /// <summary>
        /// Serializes an object into an XML document
        /// </summary>
        /// <param name="obj">object to serialize</param>
        /// <param name="namespacePrefixes">namespace prefixes</param>
        /// <returns>a string that contains the XML document</returns>
        public static string Serialize(object obj, params XmlQualifiedName[] namespacePrefixes)
        {
            return Serialize(obj, null, namespacePrefixes);
        }

        /// <summary>
        /// Serializes an object into an XML document
        /// </summary>
        /// <param name="obj">object to serialize</param>
        /// <param name="rootAttribute">root attribute to use</param>
        /// <param name="namespacePrefixes">namespace prefixes</param>
        /// <returns>a string that contains the XML document</returns>
        public static string Serialize(object obj, XmlRootAttribute rootAttribute, params XmlQualifiedName[] namespacePrefixes)
        {
            if (obj == null)
            {
                return null;
            }

            using (var textWriter = new StringWriterUTF8())
            {
                var type = obj.GetType();
                var xmlAttributeOverrides = new XmlAttributeOverrides();
                if (rootAttribute != null)
                {
                    var xmlAttributes = new XmlAttributes()
                    {
                        XmlRoot = rootAttribute
                    };
                    xmlAttributeOverrides.Add(type, xmlAttributes);
                }
                using (var xmWriter = XmlWriter.Create(textWriter, new XmlWriterSettings() { OmitXmlDeclaration = true }))
                {
                    var namespaces = new XmlSerializerNamespaces();
                    if (namespacePrefixes != null)
                    {
                        foreach (var ns in namespacePrefixes)
                        {
                            namespaces.Add(ns.Name, ns.Namespace);
                        }
                    }
                    new XmlSerializer(type, xmlAttributeOverrides).Serialize(xmWriter, obj, namespaces);
                }
                return textWriter.ToString();
            }
        }

    }
}
