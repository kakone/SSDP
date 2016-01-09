using System.Xml.Serialization;

namespace UPnP
{
    /// <summary>
    /// UPnP service
    /// </summary>
    [XmlRoot("service")]
    public class Service
    {
        /// <summary>
        /// Gets or sets the service type
        /// </summary>
        [XmlElement("serviceType")]
        public string ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the service identifier
        /// </summary>
        [XmlElement("serviceId")]
        public string ServiceId { get; set; }

        /// <summary>
        /// Gets or sets the URL to service description
        /// </summary>
        public string SCPDURL { get; set; }

        /// <summary>
        /// Gets or sets the URL for control
        /// </summary>
        [XmlElement("controlURL")]
        public string ControlURL { get; set; }

        /// <summary>
        /// Gets or sets the URL for eventing
        /// </summary>
        [XmlElement("eventSubURL")]
        public string EventSubURL { get; set; }
    }
}
