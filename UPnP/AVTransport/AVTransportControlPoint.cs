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
        /// <summary>
        /// Initializes a new instance of AVTransportControlPoint
        /// </summary>
        /// <param name="ssdp">discovery service</param>
        /// <param name="mediaInfoFetchers">media info fetchers</param>
        public AVTransportControlPoint(ISsdp ssdp, params IMediaInfoFetcher[] mediaInfoFetchers)
        {
            Ssdp = ssdp;
            MediaInfoFetchers = mediaInfoFetchers;
            var xmlSerializer = new XmlSerializer(typeof(MimeTypes));
            using (var stream = GetType().GetTypeInfo().Assembly.GetManifestResourceStream("UPnP.MimeTypes.xml"))
            {
                MimeTypes = ((MimeTypes)xmlSerializer.Deserialize(stream)).Collection;
            }
        }

        private ISsdp Ssdp { get; set; }
        private IEnumerable<IMediaInfoFetcher> MediaInfoFetchers { get; set; }
        private IEnumerable<MimeType> MimeTypes { get; set; }

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

        private async Task<MediaInfo> CheckUriAsync(HttpClient httpClient, MediaInfo mediaInfo)
        {
            var mediaInfoFetchers = MediaInfoFetchers;
            if (mediaInfoFetchers != null)
            {
                foreach (var mediaInfoFetcher in mediaInfoFetchers)
                {
                    if (await mediaInfoFetcher.RetrieveMediaInfoAsync(mediaInfo))
                    {
                        break;
                    }
                }
            }

            await SetMimeTypeAsync(httpClient, mediaInfo);
            return mediaInfo;
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
                mediaInfo = await CheckUriAsync(httpClient, mediaInfo);
                if (mediaInfo == null)
                {
                    return;
                }

                var avTransportService = mediaRenderer.Services.First(service => service.ServiceName == "AVTransport");
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
    }
}
