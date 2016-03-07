using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <param name="syndicationClient">syndication client</param>
        /// <param name="premiumLinkGenerators">premium link generators</param>
        public AVTransportControlPoint(ISsdp ssdp, ISyndicationClient syndicationClient = null, params IPremiumLinkGenerator[] premiumLinkGenerators)
        {
            Ssdp = ssdp;
            SyndicationClient = syndicationClient;
            PremiumLinkGenerators = premiumLinkGenerators;
            var xmlSerializer = new XmlSerializer(typeof(MimeTypes));
            using (var stream = GetType().GetTypeInfo().Assembly.GetManifestResourceStream("UPnP.MimeTypes.xml"))
            {
                MimeTypes = ((MimeTypes)xmlSerializer.Deserialize(stream)).Collection;
            }
        }

        private ISsdp Ssdp { get; set; }
        private ISyndicationClient SyndicationClient { get; set; }
        private IEnumerable<MimeType> MimeTypes { get; set; }
        private IEnumerable<IPremiumLinkGenerator> PremiumLinkGenerators { get; set; }

        /// <summary>
        /// Refresh the collection of media renderers
        /// </summary>
        /// <returns>the new collection of media renderers</returns>
        public async Task<IEnumerable<Device>> GetMediaRenderers()
        {
            return (await Ssdp.SearchUPnPDevices("MediaRenderer")).Where(r => r.Services.Any(service => service.ServiceName == "AVTransport")).OrderBy(r => r.FriendlyName);
        }

        private async Task SetMimeType(HttpClient httpClient, MediaInfo mediaInfo)
        {
            if (!String.IsNullOrWhiteSpace(mediaInfo.Title) && !String.IsNullOrWhiteSpace(mediaInfo.Type))
            {
                return;
            }

            var mediaUri = new Uri(WebUtility.UrlDecode(mediaInfo.Uri).Trim());
            String mediaFilename;
            try { mediaFilename = UriUtility.GetFileName(mediaUri.AbsolutePath); }
            catch (Exception) { mediaFilename = null; }
            if (String.IsNullOrWhiteSpace(mediaInfo.Title))
            {
                mediaInfo.Title = mediaFilename;
            }

            if (String.IsNullOrWhiteSpace(mediaInfo.Type))
            {
                if (!String.IsNullOrWhiteSpace(mediaFilename))
                {
                    var extension = UriUtility.GetExtension(mediaFilename);
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

        private async Task<MediaInfo> CheckUri(HttpClient httpClient, MediaInfo mediaInfo)
        {
            var premiumLinkGenerators = PremiumLinkGenerators;
            if (premiumLinkGenerators != null)
            {
                string url = null;
                foreach (var premiumLinkGenerator in premiumLinkGenerators)
                {
                    if (premiumLinkGenerator.Enabled)
                    {
                        try
                        {
                            var premiumLink = await premiumLinkGenerator.Unrestrict(url ?? (url = WebUtility.UrlDecode(mediaInfo.Uri).Trim()));
                            if (premiumLink != null)
                            {
                                if (String.IsNullOrWhiteSpace(mediaInfo.Title))
                                {
                                    mediaInfo.Title = premiumLink.Title;
                                }
                                mediaInfo.Uri = premiumLink.MainLink;
                                break;
                            }
                        }
                        catch (OperationCanceledException) { return null; }
                        catch (NotSupportedException) { }
                    }
                }
            }

            await SetMimeType(httpClient, mediaInfo);

            var syndicationClient = SyndicationClient;
            if (syndicationClient != null)
            {
                var mediaType = mediaInfo.Type;
                if (mediaType == "application/rss+xml" || mediaType == "application/xml" || mediaType == "text/xml" || mediaType == "application/rdf+xml" ||
                    mediaType == "application/atom+xml" || mediaType == "application/xml")
                {
                    // Podcast
                    var item = await syndicationClient.RetrieveFirstFeedAsync(mediaInfo.Uri);
                    if (item != null)
                    {
                        mediaInfo = item;
                        await SetMimeType(httpClient, mediaInfo);
                    }
                }
            }

            return mediaInfo;
        }

        /// <summary>
        /// Play a media HTTP link to a media renderer
        /// </summary>
        /// <param name="mediaRenderer">media renderer</param>
        /// <param name="mediaInfo">informations about the media</param>
        /// <returns>Task</returns>
        public async Task Play(Device mediaRenderer, MediaInfo mediaInfo)
        {
            using (var httpClient = CreateHttpClient())
            {
                mediaInfo = await CheckUri(httpClient, mediaInfo);
                if (mediaInfo == null)
                {
                    return;
                }

                var avTransportService = mediaRenderer.Services.First(service => service.ServiceName == "AVTransport");
                var requestUri = GetControlUri(mediaRenderer, avTransportService);

                var setAVTransportURIAction = new SetAVTransportURIAction();
                setAVTransportURIAction.CurrentUri = mediaInfo.Uri;
                setAVTransportURIAction.UriMetadata.Item = new Item()
                {
                    Title = mediaInfo.Title,
                    Creator = mediaInfo.Author,
                    Class = $"object.item.{GetMimeTypeUPnPClass(mediaInfo.Type)}",
                    Res = new Resource() { ProtocolInfo = $"http-get:*:{mediaInfo.Type}:*", Uri = mediaInfo.Uri }
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
        public async Task Play(Device mediaRenderer, string uri)
        {
            await Play(mediaRenderer, new MediaInfo() { Uri = uri });
        }
    }
}
