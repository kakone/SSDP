using System;
using System.Text.RegularExpressions;
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
        /// Gets the service name
        /// </summary>
        [XmlIgnore]
        public string ServiceName { get; private set; }

        /// <summary>
        /// Gets the service version
        /// </summary>
        [XmlIgnore]
        public int ServiceVersion { get; private set; }

        private string _serviceType;
        /// <summary>
        /// Gets or sets the service type
        /// </summary>
        [XmlElement("serviceType")]
        public string ServiceType
        {
            get { return _serviceType; }
            set
            {
                if (_serviceType != value)
                {
                    _serviceType = value;
                    if (value == null)
                    {
                        ServiceName = null;
                        ServiceVersion = 0;
                    }
                    else
                    {
                        var regex = new Regex(@":service:(?<serviceName>\w+):(?<serviceVersion>\d+)$");
                        var groups = regex.Match(value).Groups;
                        ServiceName = groups["serviceName"]?.Value;
                        var serviceVersion = groups["serviceVersion"]?.Value;
                        ServiceVersion = (String.IsNullOrWhiteSpace(serviceVersion) ? 0 : Convert.ToInt32(serviceVersion));
                    }
                }
            }
        }


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
