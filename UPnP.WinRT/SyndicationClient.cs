using System;
using System.Linq;
using System.Threading.Tasks;

namespace UPnP
{
    /// <summary>
    /// SyndicationClient to retrieve feeds from a URI asynchronously
    /// </summary>
    public class SyndicationClient : ISyndicationClient
    {
        /// <summary>
        /// Retrieve first feed from a URI asynchronously
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns>MediaInfo object that contains the informations of the first feed</returns>
        public async Task<MediaInfo> RetrieveFirstFeedAsync(string uri)
        {
            var syndicationFeed = await new Windows.Web.Syndication.SyndicationClient().RetrieveFeedAsync(new Uri(uri));
            var item = syndicationFeed.Items.FirstOrDefault();
            if (item == null)
            {
                return null;
            }
            var mediaInfo = new MediaInfo()
            {
                Author = item.Authors == null || !item.Authors.Any() ? null : item.Authors.First().Name,
                Title = item.Title.Text,
                Uri = item.ItemUri == null ? null : item.ItemUri.AbsoluteUri
            };

            if (item.Links != null)
            {
                var link = item.Links.FirstOrDefault(l => l.Relationship == "enclosure");
                if (link != null)
                {
                    mediaInfo.Uri = link.Uri.AbsoluteUri;
                    mediaInfo.Type = link.MediaType;
                }
            }
            return mediaInfo;
        }
    }
}
