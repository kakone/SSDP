using System.Threading.Tasks;

namespace UPnP
{
    /// <summary>
    /// Interface for premium link generator
    /// </summary>
    public interface IPremiumLinkGenerator
    {
        /// <summary>
        /// Gets or sets a value indicating whether the generator is enabled or not
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Unrestrict a link
        /// </summary>
        /// <param name="uri">link to unrestrict</param>
        /// <returns>a premium link</returns>
        Task<IPremiumLink> Unrestrict(string uri);
    }
}
