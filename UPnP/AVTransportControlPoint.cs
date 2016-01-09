using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UPnP
{
    /// <summary>
    /// AVTransport control point
    /// </summary>
    public class AVTransportControlPoint
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
            return (await Ssdp.SearchUPnPDevices()).Where(r => r.Services.Any(service => service.ServiceType == "urn:schemas-upnp-org:service:AVTransport:1")).OrderBy(r => r.FriendlyName);
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

        private StringContent CreateContent(string action, string xml)
        {
            var content = new StringContent(xml, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", String.Format("\"urn:schemas-upnp-org:service:AVTransport:1#{0}\"", action));
            return content;
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

            mediaInfo.XmlEscape();
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
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.ExpectContinue = false;

                mediaInfo = await CheckUri(httpClient, mediaInfo);

                var requestUri = new Uri(mediaRenderer.Services.First(service => service.ServiceType == "urn:schemas-upnp-org:service:AVTransport:1").ControlURL, UriKind.RelativeOrAbsolute);
                if (!requestUri.IsAbsoluteUri || requestUri.IsFile) // In Mono.Android, requestUri will not a relative uri but a file
                {
                    requestUri = new Uri(new Uri(mediaRenderer.URLBase), requestUri);
                }

                var xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<s:Envelope s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                        "<s:Body>" +
                            "{0}" +
                        "</s:Body>" +
                    "</s:Envelope>";

                var xmlContent = String.Format(xml, String.Format(
                    "<u:SetAVTransportURI xmlns:u=\"urn:schemas-upnp-org:service:AVTransport:1\">" +
                        "<InstanceID>0</InstanceID>" +
                        "<CurrentURI>{0}</CurrentURI>" +
                        "<CurrentURIMetaData>{1}</CurrentURIMetaData>" +
                    "</u:SetAVTransportURI>", mediaInfo.Uri,
                    String.Format(WebUtility.HtmlEncode(
                    "<DIDL-Lite xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:sec=\"http://www.sec.co.kr/\">" +
                        "<item id=\"f-0\" parentID=\"0\" restricted=\"1\">" +
                        (mediaInfo.Title == null ? String.Empty : "<dc:title>{3}</dc:title>") +
                        (mediaInfo.Author == null ? String.Empty : "<dc:creator>{4}</dc:creator>") +
                        "<upnp:class>object.item.{2}</upnp:class>" +
                        "<res protocolInfo=\"http-get:*:{1}:DLNA.ORG_OP=01;DLNA.ORG_CI=0;DLNA.ORG_FLAGS=01700000000000000000000000000000\">{0}</res>" +
                        "</item>" +
                        "</DIDL-Lite>"), mediaInfo.Uri, mediaInfo.Type, GetMimeTypeUPnPClass(mediaInfo.Type), mediaInfo.Title, mediaInfo.Author)));

                var request = CreateContent("SetAVTransportURI", xmlContent);
                request.Headers.Add("transferMode.dlna.org", "Streaming");
                request.Headers.Add("contentFeatures.dlna.org", "DLNA.ORG_OP=01;DLNA.ORG_CI=0;DLNA.ORG_FLAGS=01700000000000000000000000000000");
                var response = await httpClient.PostAsync(requestUri, request);
                if (response.IsSuccessStatusCode)
                {
                    xmlContent = String.Format(xml,
                        "<u:Play xmlns:u=\"urn:schemas-upnp-org:service:AVTransport:1\">" +
                            "<InstanceID>0</InstanceID>" +
                            "<Speed>1</Speed>" +
                        "</u:Play>");

                    response = await httpClient.PostAsync(requestUri, CreateContent("Play", xmlContent));
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
