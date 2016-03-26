using System;
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
        private bool CheckIPv6Address(HostName address, Func<ushort, bool> check)
        {
            if (address.Type != HostNameType.Ipv6)
            {
                return false;
            }

            var canonicalName = address.CanonicalName;
            return check((ushort)(canonicalName[0] * 256 + canonicalName[1]));
        }

        private bool IsIPv6LinkLocal(HostName address)
        {
            return CheckIPv6Address(address, n => (n & 0xFFC0) == 0xFE80);
        }

        private bool IsIPv6SiteLocal(HostName address)
        {
            return CheckIPv6Address(address, n => (n & 0xFFC0) == 0xFEC0);
        }

        /// <summary>
        /// Gets the type of the IP address
        /// </summary>
        /// <param name="address">IP address</param>
        /// <returns>IP adress type</returns>       
        protected override AddressType GetAddressType(HostName address)
        {
            return address.Type == HostNameType.Ipv4 ? AddressType.IPv4 :
                IsIPv6LinkLocal(address) ? AddressType.IPv6LinkLocal :
                IsIPv6SiteLocal(address) ? AddressType.IPv6SiteLocal : AddressType.Unknown;
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
