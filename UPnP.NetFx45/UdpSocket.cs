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
        private class StateObject
        {
            public Socket Socket { get; set; }
            public byte[] Buffer { get; set; }
        }

        private readonly int SIO_UDP_CONNRESET = -1744830452;

        /// <summary>
        /// Bind the socket to a local IP address
        /// </summary>
        /// <param name="address">local address</param>
        /// <returns>the socket</returns>
        public override Task<Socket> BindAsync(IPAddress address)
        {
            return Task.Run(() =>
            {
                var socket = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                socket.ExclusiveAddressUse = true;
                socket.IOControl(SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
                socket.Bind(new IPEndPoint(address, 0));
                BeginReceive(socket);
                return socket;
            });
        }

        private void BeginReceive(Socket socket)
        {
            try
            {
                var buffer = new byte[4096];
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, EndReceive, new StateObject() { Socket = socket, Buffer = buffer });
            }
            catch (ObjectDisposedException) { }
        }

        private void EndReceive(IAsyncResult result)
        {
            try
            {
                var stateObject = (StateObject)result.AsyncState;
                var length = stateObject.Socket.EndReceive(result);
                OnMessageReceived(stateObject.Buffer, length);
                BeginReceive(stateObject.Socket);
            }
            catch (ObjectDisposedException) { }
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
            await Socket.SendToAsync(data, 0, data.Length, SocketFlags.None, new IPEndPoint(IPAddress.Parse(multicastAddress), port));
        }
    }
}
