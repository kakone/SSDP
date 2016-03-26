using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace UPnP
{
    /// <summary>
    /// Simple Service Discovery Protocol implementation
    /// </summary>
    public class Ssdp : SsdpBase<UdpSocket, IPAddress>
    {
        /// <summary>
        /// Gets the type of the IP address
        /// </summary>
        /// <param name="address">IP address</param>
        /// <returns>IP adress type</returns>       
        protected override AddressType GetAddressType(IPAddress address)
        {
            return address.AddressFamily == AddressFamily.InterNetwork ? AddressType.IPv4 :
                address.IsIPv6LinkLocal ? AddressType.IPv6LinkLocal :
                address.IsIPv6SiteLocal ? AddressType.IPv6SiteLocal : AddressType.Unknown;
        }

        /// <summary>
        /// Gets a collection of local IP addresses
        /// </summary>
        /// <returns>a collection of local IP addresses</returns>
        protected override IEnumerable<IPAddress> GetLocalAddresses()
        {
            return NetworkInterface.GetAllNetworkInterfaces().Select(ni => ni.GetIPProperties()).Where(p => p.GatewayAddresses != null && p.GatewayAddresses.Any()).SelectMany(p => p.UnicastAddresses).Select(
             aInfo => aInfo.Address).Where(a => a.AddressFamily == AddressFamily.InterNetwork || a.AddressFamily == AddressFamily.InterNetworkV6);
        }
    }
}
