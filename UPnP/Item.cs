using System.Xml.Serialization;

namespace UPnP
{
    /// <summary>
    /// Media item description
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Gets or sets the item identifier
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; } = "f-0";

        /// <summary>
        /// Gets or sets the identifier of the parent item
        /// </summary>
        [XmlAttribute("parentID")]
        public string ParentId { get; set; } = "0";

        /// <summary>
        /// Gets or sets a value indicating whether the item can be modified or deleted
        /// </summary>
        [XmlAttribute("restricted")]
        public bool Restricted { get; set; } = true;

        /// <summary>
        /// Gets or sets the title of the item
        /// </summary>
        [XmlElement("title", Namespace = "http://purl.org/dc/elements/1.1/")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the creator of the item
        /// </summary>
        [XmlElement("creator", Namespace = "http://purl.org/dc/elements/1.1/")]
        public string Creator { get; set; }

        /// <summary>
        /// Gets or sets the class of the object
        /// </summary>
        [XmlElement("class", Namespace = "urn:schemas-upnp-org:metadata-1-0/upnp/")]
        public string Class { get; set; }

        /// <summary>
        /// Gets or sets the resource (typically a media file)
        /// </summary>
        [XmlElement("res")]
        public Resource Res { get; set; }
    }
}
