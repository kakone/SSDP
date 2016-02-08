using System.Xml.Serialization;

namespace UPnP
{
    /// <summary>
    /// DIDL_Lite class
    /// </summary>
    [XmlRoot("DIDL-Lite", Namespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/")]
    public class DIDL_Lite
    {
        /// <summary>
        /// Gets or sets the item
        /// </summary>
        [XmlElement("item")]
        public Item Item { get; set; }
    }
}
