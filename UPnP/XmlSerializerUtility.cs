using System.Linq;
using System.Text;
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
                    new XmlSerializer(type).Serialize(xmWriter, obj, namespaces);
                }

                if (!string.IsNullOrEmpty(rootAttribute?.Namespace))    // Bug workaround in UWP release mode instead of new XmlSerializer(type, rootAttribute)
                {
                    var prefix = namespacePrefixes.FirstOrDefault(n => n.Namespace == rootAttribute.Namespace)?.Name;
                    if (prefix != null)
                    {
                        prefix += ":";
                        var sb = new StringBuilder(textWriter.ToString());
                        sb.Insert(1, prefix);
                        sb.Insert(sb.Length - rootAttribute.ElementName.Length - 1, prefix);
                        return sb.ToString();
                    }
                }
                return textWriter.ToString();
            }
        }

    }
}
