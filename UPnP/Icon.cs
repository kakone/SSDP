using System.Xml.Serialization;

namespace UPnP
{
    /// <summary>
    /// Icon
    /// </summary>
    [XmlRoot("icon")]
    public class Icon
    {
        /// <summary>
        /// Gets or sets the mime type of the icon
        /// </summary>
        [XmlElement("mimetype")]
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the width in pixels
        /// </summary>
        [XmlElement("width")]
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height in pixels
        /// </summary>
        [XmlElement("height")]
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the color depth
        /// </summary>
        [XmlElement("depth")]
        public int Depth { get; set; }

        /// <summary>
        /// Gets or sets the URL to icon
        /// </summary>
        [XmlElement("url")]
        public string URL { get; set; }
    }
}
