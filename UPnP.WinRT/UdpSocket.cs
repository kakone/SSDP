using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace UPnP
{
    /// <summary>
    /// UDP socket
    /// </summary>
    public class UdpSocket : UdpSocketBase<DatagramSocket, HostName>
    {
        /// <summary>
        /// Bind the socket to a local IP address
        /// </summary>
        /// <param name="address">local address</param>
        /// <returns>the socket</returns>
        public override async Task<DatagramSocket> BindAsync(HostName address)
        {
            var socket = new DatagramSocket();
            socket.MessageReceived += (sender, args) =>
            {
                using (var dataReader = args.GetDataReader())
                {
                    var buffer = new byte[dataReader.UnconsumedBufferLength];
                    dataReader.ReadBytes(buffer);
                    OnMessageReceived(buffer);
                }
            };
            await socket.BindEndpointAsync(address, String.Empty);
            return socket;
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
            using (var stream = await Socket.GetOutputStreamAsync(new HostName(multicastAddress), port.ToString()))
            {
                await stream.WriteAsync(data.AsBuffer());
                await stream.FlushAsync();
            }
        }
    }
}
