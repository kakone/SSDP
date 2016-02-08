using System.Xml.Serialization;

namespace UPnP
{
    /// <summary>
    /// Resource (typically a media file)
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Gets or sets the protocol informations
        /// </summary>
        [XmlAttribute("protocolInfo")]
        public string ProtocolInfo { get; set; }

        /// <summary>
        /// Gets or sets the uri of the resource
        /// </summary>
        [XmlText]
        public string Uri { get; set; }
    }
}
