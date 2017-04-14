using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UPnP.AVTransport
{
    /// <summary>
    /// AVTransport control point
    /// </summary>
    public class AVTransportControlPoint : ControlPoint
    {
        private const string SERVICE_NAME = "AVTransport";

        /// <summary>
        /// Initializes a new instance of AVTransportControlPoint
        /// </summary>
        /// <param name="ssdp">discovery service</param>
        public AVTransportControlPoint(ISsdp ssdp = null)
        {
            Ssdp = ssdp ?? new Ssdp();
            var xmlSerializer = new XmlSerializer(typeof(MimeTypes));
            using (var stream = GetType().GetTypeInfo().Assembly.GetManifestResourceStream("UPnP.MimeTypes.xml"))
            {
                MimeTypes = ((MimeTypes)xmlSerializer.Deserialize(stream)).Collection;
            }
        }

        private ISsdp Ssdp { get; }
        private IEnumerable<MimeType> MimeTypes { get; }

        /// <summary>
        /// Refresh the collection of media renderers
        /// </summary>
        /// <returns>the new collection of media renderers</returns>
        public async Task<IEnumerable<Device>> GetMediaRenderersAsync()
        {
            return (await Ssdp.SearchUPnPDevicesAsync("MediaRenderer")).Where(r => r.Services.Any(service => service.ServiceName == "AVTransport")).OrderBy(r => r.FriendlyName);
        }

        private async Task SetMimeTypeAsync(HttpClient httpClient, MediaInfo mediaInfo)
        {
            if (!String.IsNullOrWhiteSpace(mediaInfo.Title) && !String.IsNullOrWhiteSpace(mediaInfo.Type))
            {
                return;
            }

            var mediaUri = new Uri(WebUtility.UrlDecode(mediaInfo.Uri).Trim());
            String mediaFilename;
            try { mediaFilename = Path.GetFileName(mediaUri.AbsolutePath); }
            catch (Exception) { mediaFilename = null; }
            if (String.IsNullOrWhiteSpace(mediaInfo.Title))
            {
                mediaInfo.Title = mediaFilename;
            }

            if (String.IsNullOrWhiteSpace(mediaInfo.Type))
            {
                if (!String.IsNullOrWhiteSpace(mediaFilename))
                {
                    var extension = Path.GetExtension(mediaFilename);
                    if (!String.IsNullOrWhiteSpace(extension))
                    {
                        extension = extension.Substring(1).ToLower();
                        var mimeType = MimeTypes.FirstOrDefault(mt => mt.Extensions.Any(ext => extension == ext));
                        if (mimeType != null)
                        {
                            mediaInfo.Type = mimeType.Type;
                            return;
                        }
                    }
                }

                var httpRequest = new HttpRequestMessage() { RequestUri = mediaUri, Method = HttpMethod.Head };
                try
                {
                    var contentType = (await httpClient.GetAsync(mediaUri).TimeoutAfter(5000)).Content.Headers.ContentType;
                    if (contentType != null)
                    {
                        mediaInfo.Type = contentType.MediaType;
                        return;
                    }
                }
                catch (Exception) { }

                mediaInfo.Type = "video/mp4";
            }
        }

        private string GetMimeTypeUPnPClass(string mediaType)
        {
            var splittedString = mediaType.Trim().ToLower().Split('/');
            if (splittedString.Any())
            {
                switch (splittedString[0])
                {
                    case "video": return "videoItem";
                    case "image": return "imageItem";
                }
            }

            return "audioItem.musicTrack";
        }

        /// <summary>
        /// Play a media HTTP link to a media renderer
        /// </summary>
        /// <param name="mediaRenderer">media renderer</param>
        /// <param name="mediaInfo">informations about the media</param>
        /// <returns>Task</returns>
        public async Task PlayAsync(Device mediaRenderer, MediaInfo mediaInfo)
        {
            using (var httpClient = CreateHttpClient())
            {
                await SetMimeTypeAsync(httpClient, mediaInfo);

                var avTransportService = GetService(mediaRenderer, SERVICE_NAME);
                var requestUri = GetControlUri(mediaRenderer, avTransportService);

                var setAVTransportURIAction = new SetAVTransportURIAction()
                {
                    CurrentUri = mediaInfo.Uri,
                    UriMetadata = new DIDL_Lite()
                    {
                        Item = new Item()
                        {
                            Title = mediaInfo.Title,
                            Creator = mediaInfo.Author,
                            Class = $"object.item.{GetMimeTypeUPnPClass(mediaInfo.Type)}",
                            Res = new Resource() { ProtocolInfo = $"http-get:*:{mediaInfo.Type}:*", Uri = mediaInfo.Uri }
                        }
                    }
                };

                var response = await PostActionAsync(httpClient, avTransportService, requestUri, setAVTransportURIAction, 
                    Tuple.Create("transferMode.dlna.org", "Streaming"));
                if (response.IsSuccessStatusCode)
                {
                    await PostActionAsync(httpClient, avTransportService, requestUri, new PlayAction());
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        /// <summary>
        /// Play a media HTTP link to a media renderer
        /// </summary>
        /// <param name="mediaRenderer">media renderer</param>
        /// <param name="uri">uri of the media</param>
        /// <returns>Task</returns>
        public async Task PlayAsync(Device mediaRenderer, string uri)
        {
            await PlayAsync(mediaRenderer, new MediaInfo() { Uri = uri });
        }

        private async Task<HttpResponseMessage> PostActionAsync(Device device, object action)
        {
            return await PostActionAsync(device, SERVICE_NAME, action);
        }

        /// <summary>
        /// Play
        /// </summary>
        /// <param name="mediaRenderer">media renderer</param>
        /// <returns>Task</returns>
        public async Task PlayAsync(Device mediaRenderer)
        {
            await PostActionAsync(mediaRenderer, new PlayAction());
        }

        /// <summary>
        /// Pause
        /// </summary>
        /// <param name="mediaRenderer">media renderer</param>
        /// <returns>Task</returns>
        public async Task PauseAsync(Device mediaRenderer)
        {
            await PostActionAsync(mediaRenderer, new PauseAction());
        }

        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="mediaRenderer">media renderer</param>
        /// <returns>Task</returns>
        public async Task StopAsync(Device mediaRenderer)
        {
            await PostActionAsync(mediaRenderer, new StopAction());
        }
    }
}
