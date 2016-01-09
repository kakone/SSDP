using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace UPnP
{
    /// <summary>
    /// UDP socket
    /// </summary>
    public class UdpSocket : UdpSocketBase<Socket, IPAddress>
    {
        /// <summary>
        /// Bind the socket to a local IP address
        /// </summary>
        /// <param name="address">local address</param>
        /// <returns>the socket</returns>
        public override async Task<Socket> BindAsync(IPAddress address)
        {
            return await Task.Run(() =>
            {
                var socket = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                socket.ExclusiveAddressUse = true;
                socket.Bind(new IPEndPoint(address, 0));
                return socket;
            });       
        }

        /// <summary>
        /// Send data to multicast address and port
        /// </summary>
        /// <param name="multicastAddress">multicastAddress</param>
        /// <param name="port">port</param>
        /// <param name="data">data to send</param>
        /// <returns>Task</returns>
        public override async Task SendToAsync(string multicastAddress, int port, byte[] data)
        {
            await Task.Run(() => Socket.SendTo(data, new IPEndPoint(IPAddress.Parse(multicastAddress), port)));
        }
    }
}