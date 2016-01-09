using System.Threading.Tasks;

namespace UPnP
{
    /// <summary>
    /// Encapsulates the method needed to retrieve feeds from a URI asynchronously
    /// </summary>
    public interface ISyndicationClient
    {
        /// <summary>
        /// Retrieve first feed from a URI asynchronously
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns>MediaInfo object that contains the informations of the first feed</returns>
        Task<MediaInfo> RetrieveFirstFeedAsync(string uri);
    }
}
