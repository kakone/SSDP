using System.Threading.Tasks;

namespace UPnP.AVTransport
{
    /// <summary>
    /// Interface to fetch media informations
    /// </summary>
    public interface IMediaInfoFetcher
    {
        /// <summary>
        /// Complete the informations about the media
        /// </summary>
        /// <param name="mediaInfo">media informations</param>
        /// <returns>true if the media informations are complete and no further research is required, false otherwise</returns>
        Task<bool> RetrieveMediaInfoAsync(MediaInfo mediaInfo);
    }
}
