using System.Collections.Generic;
using System.Threading.Tasks;

namespace UPnP.AVTransport
{
    /// <summary>
    /// Interface for AVTransport control point
    /// </summary>
    public interface IAVTransportControlPoint
    {
        /// <summary>
        /// Refresh the collection of media renderers
        /// </summary>
        /// <returns>the new collection of media renderers</returns>
        Task<IEnumerable<Device>> GetMediaRenderersAsync();

        /// <summary>
        /// Play a media HTTP link to a media renderer
        /// </summary>
        /// <param name="mediaRenderer">media renderer</param>
        /// <param name="uri">uri of the media</param>
        /// <returns>Task</returns>
        Task PlayAsync(Device mediaRenderer, string uri);

        /// <summary>
        /// Play a media HTTP link to a media renderer
        /// </summary>
        /// <param name="mediaRenderer">media renderer</param>
        /// <param name="mediaInfo">informations about the media</param>
        /// <returns>Task</returns>
        Task PlayAsync(Device mediaRenderer, MediaInfo mediaInfo);

        /// <summary>
        /// Play
        /// </summary>
        /// <param name="mediaRenderer">media renderer</param>
        /// <returns>Task</returns>
        Task PlayAsync(Device mediaRenderer);

        /// <summary>
        /// Pause
        /// </summary>
        /// <param name="mediaRenderer">media renderer</param>
        /// <returns>Task</returns>
        Task PauseAsync(Device mediaRenderer);

        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="mediaRenderer">media renderer</param>
        /// <returns>Task</returns>
        Task StopAsync(Device mediaRenderer);
    }
}
