using System.Runtime.Serialization;

namespace UPnP.RealDebrid
{
    /// <summary>
    /// RealDebrid link
    /// </summary>
    [DataContract]
    public class RealDebridLink : IPremiumLink
    {
        /// <summary>
        /// Gets or sets the title of the media
        /// </summary>
        [DataMember(Name = "file_name")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the main link
        /// </summary>
        [DataMember(Name = "main_link")]
        public virtual string MainLink { get; set; }
    }
}
