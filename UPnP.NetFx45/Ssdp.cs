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
        /// Gets a value indicating whether the IP address is an IPv4 ou IPv6 address
        /// </summary>
        /// <param name="address">IP address</param>
        /// <returns>true if the IP address is an IPv4, false otherwise</returns>
        protected override bool IsIPv4(IPAddress address)
        {
            return address.AddressFamily == AddressFamily.InterNetwork;
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
