using System.Collections.Generic;
using System.Xml.Serialization;

namespace UPnP
{
    /// <summary>
    /// UPnP Device
    /// </summary>
    [XmlRoot("device", Namespace = "urn:schemas-upnp-org:device-1-0")]
    public class Device
    {
        /// <summary>
        /// Gets or sets the base URL for all relative URLs
        /// </summary>
        public string URLBase { get; set; }

        /// <summary>
        /// Gets or sets the device type
        /// </summary>
        [XmlElement("deviceType")]
        public string DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the short user-friendly title
        /// </summary>
        [XmlElement("friendlyName")]
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer name
        /// </summary>
        [XmlElement("manufacturer")]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the URL to manufacturer site
        /// </summary>
        [XmlElement("manufacturerURL")]
        public string ManufacturerURL { get; set; }

        /// <summary>
        /// Gets or sets the long user-friendly title
        /// </summary>
        [XmlElement("modelDescription")]
        public string ModelDescription { get; set; }

        /// <summary>
        /// Gets or sets the model name
        /// </summary>
        [XmlElement("modelName")]
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the model number
        /// </summary>
        [XmlElement("modelNumber")]
        public string ModelNumber { get; set; }

        /// <summary>
        /// Gets or sets the URL to model site
        /// </summary>
        [XmlElement("modelURL")]
        public string ModelURL { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer's serial number
        /// </summary>
        [XmlElement("serialNumber")]
        public string SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public string UDN { get; set; }

        /// <summary>
        /// Gets or sets the Universal Product Code
        /// </summary>
        public string UPC { get; set; }

        /// <summary>
        /// Gets the collection of icons
        /// </summary>
        [XmlArray("iconList")]
        [XmlArrayItem("icon")]
        public List<Icon> Icons { get; set; }

        /// <summary>
        /// Gets the collection
        /// </summary>
        [XmlArray("serviceList")]
        [XmlArrayItem("service")]
        public List<Service> Services { get; set; }

        /// <summary>
        /// Returns a string that represents the current object
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString()
        {
            return FriendlyName;
        }
    }
}
