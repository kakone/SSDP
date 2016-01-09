
namespace UPnP
{
    /// <summary>
    /// SocketMessageReceived event handler
    /// </summary>
    /// <typeparam name="TUdpSocket">UDP socket type</typeparam>
    /// <typeparam name="TAddress">IP address type</typeparam>
    /// <param name="sender">UDP socket</param>
    /// <param name="eventArgs">event args</param>
    public delegate void SocketMessageReceivedEventHandler<TUdpSocket, TAddress>(TUdpSocket sender, SocketMessageReceivedEventArgs eventArgs) where TUdpSocket : IUdpSocket<TAddress>;
}
