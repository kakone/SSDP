using System.Collections.Generic;
using System.Xml.Serialization;

namespace UPnP
{
    /// <summary>
    /// Mime type class
    /// </summary>
    [XmlRoot("mimeType")]
    public class MimeType
    {
        /// <summary>
        /// Gets or sets the mime type
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the possible extensions for this mime type
        /// </summary>
        [XmlArray("extensions")]
        [XmlArrayItem("extension")]
        public string[] Extensions { get; set; }
    }
}
