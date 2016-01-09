using System;
using System.Threading.Tasks;

namespace UPnP
{
    /// <summary>
    /// Interface for UDP sockets
    /// </summary>
    /// <typeparam name="TAddress">IP address type</typeparam>
    public interface IUdpSocket<TAddress> : IDisposable
    {
        /// <summary>
        /// Raised when a message is received
        /// </summary>
        event SocketMessageReceivedEventHandler<IUdpSocket<TAddress>, TAddress> MessageReceived;

        /// <summary>
        /// Bind the socket to local IP address
        /// </summary>
        /// <param name="address">address</param>
        /// <returns>Task</returns>
        Task BindAsync(TAddress address);

        /// <summary>
        /// Send data to multicast address and port
        /// </summary>
        /// <param name="multicastAddress">multicastAddress</param>
        /// <param name="port">port</param>
        /// <param name="data">data to send</param>
        /// <returns>Task</returns>
        Task SendToAsync(string multicastAddress, int port, byte[] data);
    }
}
