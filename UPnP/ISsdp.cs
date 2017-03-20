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
        /// Search devices
        /// </summary>
        /// <param name="deviceType">device type</param>
        /// <returns>a collection of notifications</returns>
        Task<IEnumerable<DeviceNotification>> Search(string deviceType);

        /// <summary>
        /// Search devices
        /// </summary>
        /// <param name="deviceType">device type</param>
        /// <returns>a collection of found devices</returns>
        Task<IEnumerable<Device>> SearchDevices(string deviceType);

        /// <summary>
        /// Search UPnP devices
        /// </summary>
        /// <param name="deviceType">UPnP device type</param>
        /// <param name="deviceVersion">UPnP device version</param>
        /// <returns>a collection of found devices</returns>
        Task<IEnumerable<Device>> SearchUPnPDevices(string deviceType, int deviceVersion = 1);        
    }
}
