using System.Collections.Generic;
using System.Threading.Tasks;

namespace UPnP
{
    /// <summary>
    /// Interface for the discovery service
    /// </summary>
    public interface ISsdp
    {
        /// <summary>
        /// Search UPnP devices
        /// </summary>
        /// <param name="deviceType">device type (MediaRenderer by default)</param>
        /// <returns>a collection of found devices</returns>
        Task<IEnumerable<Device>> SearchUPnPDevices(string deviceType = "MediaRenderer");
    }
}
