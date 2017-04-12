using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace UPnP.AVTransport
{
    /// <summary>
    /// Base class for the actions
    /// </summary>
    public abstract class Action
    {
        /// <summary>
        /// Gets or sets the instance ID
        /// </summary>
        [XmlElement(Namespace = "")]
        public uint InstanceID { get; set; }
    }
}
