#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using System.Linq;
using System.Threading.Tasks;
using UPnP.AVTransport;

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
            var mediaRenderers = await controlPoint.GetMediaRenderers();
            if (!mediaRenderers.Any())
            {
                Assert.Inconclusive("No media renderer found");
            }
            await controlPoint.Play(
                mediaRenderers.FirstOrDefault(render => render.FriendlyName.StartsWith("Kodi")) ?? mediaRenderers.First(),
                BIG_BUCK_BUNNY_MOVIE);
        }
    }
}
