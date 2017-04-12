using System.Xml.Serialization;

namespace UPnP.AVTransport
{
    /// <summary>
    /// Play action
    /// </summary>
    [XmlRoot("Play")]
    public class PlayAction : Action
    {
        /// <summary>
        /// Gets or sets the play speed (example values : "1", "1/2", "-1", "1/10", etc.)
        /// </summary>
        [XmlElement(Namespace = "")]
        public string Speed { get; set; } = "1";
    }
}
