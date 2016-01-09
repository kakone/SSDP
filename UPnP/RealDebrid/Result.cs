using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace UPnP.RealDebrid
{
    /// <summary>
    /// Response message to RealDebrid service
    /// </summary>
    [DataContract]
    public class Result : RealDebridLink
    {
        /// <summary>
        /// Initializes a new instance of Result class
        /// </summary>
        public Result()
        {
            Links = new List<List<string>>();
        }

        /// <summary>
        /// Gets or sets the error code
        /// </summary>
        [DataMember(Name = "error")]
        public int ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        [DataMember(Name = "message")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the main link
        /// </summary>
        [DataMember(Name = "main_link")]
        public override string MainLink
        {
            get
            {
                if (base.MainLink == null)
                {
                    var link = Links.FirstOrDefault();
                    if (link != null)
                    {
                        base.MainLink = link.ElementAtOrDefault(2);
                    }
                }
                return base.MainLink;
            }
        }

        /// <summary>
        /// Gets or sets the generated links
        /// </summary>
        [DataMember(Name = "generated_links")]
        public IEnumerable<IEnumerable<string>> Links { get; private set; }
    }
}
