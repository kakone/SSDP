using System.Collections.Generic;
using System.Linq;
using Windows.Networking;
using Windows.Networking.Connectivity;

namespace UPnP
{
    /// <summary>
    /// Simple Service Discovery Protocol implementation
    /// </summary>
    public class Ssdp : SsdpBase<UdpSocket, HostName>
    {
        /// <summary>
        /// Gets a value indicating whether the IP address is an IPv4 ou IPv6 address
        /// </summary>
        /// <param name="address">IP address</param>
        /// <returns>true if the IP address is an IPv4, false otherwise</returns>       
        protected override bool IsIPv4(HostName address)
        {
            return address.Type == HostNameType.Ipv4;
        }

        /// <summary>
        /// Gets a collection of local IP addresses
        /// </summary>
        /// <returns>a collection of local IP addresses</returns>
        protected override IEnumerable<HostName> GetLocalAddresses()
        {
            var cp = NetworkInformation.GetInternetConnectionProfile();
            return NetworkInformation.GetHostNames().Where(hn => (hn.Type == HostNameType.Ipv4 || hn.Type == HostNameType.Ipv6) &&
                hn.IPInformation.NetworkAdapter.NetworkAdapterId == cp.NetworkAdapter.NetworkAdapterId);
        }
    }
}
