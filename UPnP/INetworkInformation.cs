using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace UPnP
{
    /// <summary>
    /// To retrieve network information
    /// </summary>
    public interface INetworkInformation
    {
        /// <summary>
        /// Gets a collection of local IP addresses
        /// </summary>
        /// <returns>a collection of local IP addresses</returns>
        IEnumerable<IPAddress> GetLocalAddresses();
    }
}
