using System;
using System.Threading.Tasks;

namespace UPnP
{
    /// <summary>
    /// Base class for UDP socket
    /// </summary>
    /// <typeparam name="TSocket">underlying socket type</typeparam>
    /// <typeparam name="TAddress">IP address type</typeparam>
    public abstract class UdpSocketBase<TSocket, TAddress> : IUdpSocket<TAddress> where TSocket : IDisposable
    {
        /// <summary>
        /// Raised when a message is received
        /// </summary>
        public event SocketMessageReceivedEventHandler<IUdpSocket<TAddress>, TAddress> MessageReceived;

        /// <summary>
        /// Gets the inner socket
        /// </summary>
        protected TSocket Socket { get; private set; }

        /// <summary>
        /// Bind the socket to a local IP address
        /// </summary>
        /// <param name="address">local address</param>
        /// <returns>the socket</returns>
        public abstract Task<TSocket> BindAsync(TAddress address);

        /// <summary>
        /// Send data to multicast address and port
        /// </summary>
        /// <param name="multicastAddress">multicastAddress</param>
        /// <param name="port">port</param>
        /// <param name="data">data to send</param>
        /// <returns>Task</returns>
        public abstract Task SendToAsync(string multicastAddress, int port, byte[] data);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Socket != null)
            {
                Socket.Dispose();
            }
        }

        /// <summary>
        /// Bind the socket to local IP address
        /// </summary>
        /// <param name="address">address</param>
        /// <returns>Task</returns>
        async Task IUdpSocket<TAddress>.BindAsync(TAddress address)
        {
            Socket = await BindAsync(address);
        }

        /// <summary>
        /// Calls the MessageReceived event
        /// </summary>
        /// <param name="message">received message</param>
        /// <param name="length">length of the message</param>
        protected virtual void OnMessageReceived(byte[] message, int? length = null)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, new SocketMessageReceivedEventArgs() { Message = message, Length = length });
            }
        }
    }
}
