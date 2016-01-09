using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

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
        public Task<MediaInfo> RetrieveFirstFeedAsync(string uri)
        {
            return Task.Run(() =>
                {
                    SyndicationFeed feed;
                    using (var xmlReader = XmlReader.Create(uri))
                    {
                        feed = SyndicationFeed.Load(xmlReader);
                    }
                    var item = feed.Items.FirstOrDefault();
                    if (item == null)
                    {
                        return null;
                    }
                    var mediaInfo = new MediaInfo()
                    {
                        Author = item.Authors == null || !item.Authors.Any() ? null : item.Authors.First().Name,
                        Title = item.Title.Text,
                    };

                    if (item.Links != null)
                    {
                        var link = item.Links.FirstOrDefault(l => l.RelationshipType == "enclosure");
                        if (link != null)
                        {
                            mediaInfo.Uri = link.Uri.AbsoluteUri;
                            mediaInfo.Type = link.MediaType;
                        }
                    }
                    return mediaInfo;
                });
        }
    }
}
