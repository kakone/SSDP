using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Linq;
using System.Net.Sockets;

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
            return NetworkInterface.GetAllNetworkInterfaces()
                .Select(ni => ni.GetIPProperties())
                .Where(p => p.GatewayAddresses != null && p.GatewayAddresses.Any())
                .SelectMany(p => p.UnicastAddresses)
                .Select(aInfo => aInfo.Address)
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork || a.AddressFamily == AddressFamily.InterNetworkV6);
        }
    }
}
