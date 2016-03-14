using System;
using System.Xml;
using System.Xml.Serialization;

namespace UPnP.AVTransport
{
    /// <summary>
    /// SetAVTransportURI action
    /// </summary>
    [XmlRoot("SetAVTransportURI")]
    public class SetAVTransportURIAction
    {
        /// <summary>
        /// Gets or sets the instance ID
        /// </summary>
        [XmlElement(Namespace = "")]
        public uint InstanceID { get; set; }

        /// <summary>
        /// Gets or sets the URI of the resource to be controlled
        /// </summary>
        [XmlElement("CurrentURI", Namespace = "")]
        public string CurrentUri { get; set; }

        /// <summary>
        /// Gets or sets the meta data associated with the specified resource
        /// </summary>
        [XmlIgnore]
        public DIDL_Lite UriMetadata { get; set; } = new DIDL_Lite();

        /// <summary>
        /// Gets the meta data (DIDL-Lite XML fragment)
        /// </summary>
        [XmlElement("CurrentURIMetaData", Namespace = "")]
        public string CurrentUriMetadata
        {
            get
            {
                return XmlSerializerUtility.Serialize(UriMetadata,
                    new XmlQualifiedName("upnp", "urn:schemas-upnp-org:metadata-1-0/upnp/"),
                    new XmlQualifiedName("dc", "http://purl.org/dc/elements/1.1/"),
                    new XmlQualifiedName("sec", "http://www.sec.co.kr/"));
            }
            set { throw new NotImplementedException(); }
        }
    }
}
