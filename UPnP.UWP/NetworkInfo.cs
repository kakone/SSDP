using System.Collections.Generic;
using System.Linq;
using System.Net;
using Windows.Networking;

namespace UPnP
{
    /// <summary>
    /// Retrieve network information
    /// </summary>
    public class NetworkInfo : INetworkInformation
    {
        /// <summary>
        /// Gets a collection of local IP addresses
        /// </summary>
        /// <returns>a collection of local IP addresses</returns>
        public IEnumerable<IPAddress> GetLocalAddresses()
        {
            var cp = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            return Windows.Networking.Connectivity.NetworkInformation.GetHostNames()
                .Where(hn => (hn.Type == HostNameType.Ipv4 || hn.Type == HostNameType.Ipv6) &&
                    hn.IPInformation.NetworkAdapter.NetworkAdapterId == cp.NetworkAdapter.NetworkAdapterId)
                .Select(hn => IPAddress.Parse(hn.CanonicalName));
        }
    }
}