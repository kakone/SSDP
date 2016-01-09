using System.Threading.Tasks;

namespace UPnP
{
    /// <summary>
    /// Abstract class for premium link generator
    /// </summary>
    public abstract class PremiumLinkGenerator : IPremiumLinkGenerator
    {
        /// <summary>
        /// Gets or sets a value indicating whether the generator is enabled or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Unrestrict a link
        /// </summary>
        /// <param name="uri">link to unrestrict</param>
        /// <returns>a premium link</returns>
        public abstract Task<IPremiumLink> Unrestrict(string uri);
    }
}
