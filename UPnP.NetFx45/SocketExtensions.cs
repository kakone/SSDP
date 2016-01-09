using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace UPnP
{
    static class SocketExtensions
    {
        /// <summary>
        /// Sends data asynchronously to a specific remote host
        /// </summary>
        /// <param name="socket">socket</param>
        /// <param name="buffer">an array of type System.Byte that contains the data to send</param>
        /// <param name="offset">the zero-based position in buffer at which to begin sending data</param>
        /// <param name="size">the number of bytes to send</param>
        /// <param name="flags">a bitwise combination of the System.Net.Sockets.SocketFlags values.</param>
        /// <param name="remoteEP">an System.Net.EndPoint that represents the remote device</param>
        /// <returns>Task</returns>
        public static Task SendToAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags flags, EndPoint remoteEP)
        {
            return Task<int>.Factory.FromAsync(
                (ac, state) => socket.BeginSendTo(buffer, offset, size, flags, remoteEP, ac, state),
                socket.EndSendTo,
                null,
                TaskCreationOptions.None);
        }
    }
}
