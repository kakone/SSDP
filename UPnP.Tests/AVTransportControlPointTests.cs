using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using UPnP.AVTransport;
using System.Linq;

namespace UPnP.Tests
{
    /// <summary>
    /// Test class for AVTransport service control point
    /// </summary>
    [TestClass]
    public class AVTransportControlPointTests
    {
        private const string BIG_BUCK_BUNNY_MOVIE = "http://download.blender.org/peach/bigbuckbunny_movies/big_buck_bunny_480p_surround-fix.avi";

        /// <summary>
        /// Play a video to a media renderer
        /// </summary>
        [TestMethod]
        public async Task Play()
        {
            var controlPoint = new AVTransportControlPoint(new Ssdp());
            var mediaRenderers = await controlPoint.GetMediaRenderersAsync();
            if (!mediaRenderers.Any())
            {
                Assert.Inconclusive("No media renderer found");
            }
            await controlPoint.PlayAsync(
                mediaRenderers.FirstOrDefault(render => render.FriendlyName.StartsWith("Kodi")) ?? mediaRenderers.First(),
                BIG_BUCK_BUNNY_MOVIE);
        }
    }
}
