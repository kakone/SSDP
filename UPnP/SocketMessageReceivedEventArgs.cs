using System;

namespace UPnP
{
    /// <summary>
    /// SocketMessageReceived event args
    /// </summary>
    public class SocketMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the received message
        /// </summary>
        public byte[] Message { get; set; }

        /// <summary>
        /// Gets or sets the length of the message
        /// </summary>
        public int? Length { get; set; }
    }
}
