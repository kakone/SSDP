using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace UPnP
{
    /// <summary>
    /// Base class for Simple Service Discovery Protocol implementation
    /// </summary>
    public class Ssdp : ISsdp
    {
        private const int SIO_UDP_CONNRESET = -1744830452;
        private const int UPNP_MULTICAST_PORT = 1900;
        private const int RECEIVE_TIMEOUT = 3000;

        /// <summary>
        /// Initializes a new instance of Ssdp class
        /// </summary>
        /// <param name="networkInformation">network information retriever</param>
        public Ssdp(INetworkInformation networkInformation = null)
        {
            NetworkInformation = networkInformation ?? new NetworkInfo();
        }

        private INetworkInformation NetworkInformation { get; }

        private IDictionary<AddressType, string> MulticastAddresses { get; } =
            new Dictionary<AddressType, string>
            {
                [AddressType.IPv4] = "239.255.255.250",
                [AddressType.IPv6LinkLocal] = "FF02::C",
                [AddressType.IPv6SiteLocal] = "FF05::C",
            };

        private async Task<IEnumerable<string>> GetDevicesAsync(string deviceType)
        {
            var tasks = new List<Task<IEnumerable<string>>>();
            foreach (var localAddress in NetworkInformation.GetLocalAddresses())
            {
                tasks.Add(SearchDevicesAsync(localAddress, deviceType));
            }
            var results = await Task.WhenAll(tasks);
            return results.SelectMany(result => result);
        }      

        private async Task ReceiveAsync(Socket socket, ArraySegment<byte> buffer, ICollection<string> responses)
        {
            while (true)
            {
                var i = await socket.ReceiveAsync(buffer, SocketFlags.None);
                if (i > 0)
                {
                    responses.Add(Encoding.UTF8.GetString(buffer.Take(i).ToArray()));
                }
            }
        }

        private async Task<IEnumerable<string>> SearchDevicesAsync(IPAddress localAddress, string deviceType)
        {
            var responses = new List<string>();

            var addressType = (localAddress.AddressFamily == AddressFamily.InterNetwork ? AddressType.IPv4 :
                localAddress.IsIPv6LinkLocal ? AddressType.IPv6LinkLocal :
                localAddress.IsIPv6SiteLocal ? AddressType.IPv6SiteLocal : AddressType.Unknown);
            if (addressType != AddressType.Unknown)
            {
                try
                {
                    using (var socket = new Socket(localAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
                    {
                        socket.ExclusiveAddressUse = true;
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            socket.IOControl(SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
                        }
                        socket.Bind(new IPEndPoint(localAddress, 0));

                        var multicastEndPoint = new IPEndPoint(IPAddress.Parse(MulticastAddresses[addressType]), UPNP_MULTICAST_PORT);
                        var req = "M-SEARCH * HTTP/1.1\r\n" +
                            $"HOST: {multicastEndPoint}\r\n" +
                            $"ST: {deviceType}\r\n" +
                            "MAN: \"ssdp:discover\"\r\n" +
                            "MX: 3\r\n\r\n";
                        var data = new ArraySegment<byte>(Encoding.UTF8.GetBytes(req));
                        for (int i = 0; i < 3; i++)
                        {
                            await socket.SendToAsync(data, SocketFlags.None, multicastEndPoint);
                        }
                        await ReceiveAsync(socket, new ArraySegment<byte>(new byte[4096]), responses).TimeoutAfter(RECEIVE_TIMEOUT);
                    }
                }
                catch (TimeoutException) { }
                catch (ObjectDisposedException) { }
            }

            return responses;
        }

        private Dictionary<string, string> GetProperties(string response)
        {
            var properties = new Dictionary<string, string>();
            foreach (var x in response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                if (x.Contains(":"))
                {
                    var strings = x.Split(':');
                    properties.Add(strings[0].ToLower(), x.Remove(0, strings[0].Length + 1));
                }
            }
            return properties;
        }

        /// <summary>
        /// Search devices
        /// </summary>
        /// <param name="deviceType">device type</param>
        /// <returns>a collection of notifications</returns>
        public async Task<IEnumerable<DeviceNotification>> SearchAsync(string deviceType)
        {
            var responses = await GetDevicesAsync(deviceType);

            var devices = new List<DeviceNotification>();
            DeviceNotification deviceNotification;
            var xmlSerializer = new XmlSerializer(typeof(DeviceNotification));
            var deviceNotificationProperties = typeof(DeviceNotification).GetRuntimeProperties();
            foreach (var response in responses)
            {
                var properties = GetProperties(response);
                deviceNotification = new DeviceNotification();
                foreach (var property in deviceNotificationProperties)
                {
                    property.SetValue(deviceNotification, properties[property.Name.ToLower()], null);
                }
                if (!devices.Any(dn => dn.USN == deviceNotification.USN))
                {
                    devices.Add(deviceNotification);
                }
            }
            return devices;
        }

        /// <summary>
        /// Search devices
        /// </summary>
        /// <param name="deviceType">device type</param>
        /// <returns>a collection of found devices</returns>
        public async Task<IEnumerable<Device>> SearchDevicesAsync(string deviceType)
        {
            var devices = new List<Device>();
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.ExpectContinue = false;
            var xmlSerializer = new XmlSerializer(typeof(Device));
            foreach (var deviceNotification in await SearchAsync(deviceType))
            {
                try
                {
                    var response = await httpClient.GetByteArrayAsync(deviceNotification.Location);
                    using (var textReader = new StringReader(Encoding.UTF8.GetString(response, 0, response.Length)))
                    {
                        using (var xmlReader = XmlReader.Create(textReader))
                        {
                            string urlBase = null;
                            while (xmlReader.Read())
                            {
                                if (xmlReader.Name == "URLBase")
                                {
                                    urlBase = xmlReader.ReadElementContentAsString();
                                }
                                if (xmlReader.Name == "device")
                                {
                                    var device = (Device)xmlSerializer.Deserialize(xmlReader.ReadSubtree());
                                    device.URLBase = urlBase ?? deviceNotification.Location;
                                    devices.Add(device);
                                }
                            }
                        }
                    }
                }
                catch (Exception) { }
            }
            return devices;
        }

        /// <summary>
        /// Search UPnP devices
        /// </summary>
        /// <param name="deviceType">device type</param>
        /// <param name="deviceVersion">device version</param>
        /// <returns>a collection of found devices</returns>
        public async Task<IEnumerable<Device>> SearchUPnPDevicesAsync(string deviceType, int deviceVersion = 1)
        {
            return await SearchDevicesAsync($"urn:schemas-upnp-org:device:{deviceType}:{deviceVersion}");
        }
    }
}
