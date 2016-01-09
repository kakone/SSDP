using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using UPnP.Cryptography;

namespace UPnP.RealDebrid
{
    /// <summary>
    /// Client for RealDebrid service
    /// </summary>
    public class RealDebridClient : PremiumLinkGenerator
    {
        private CookieContainer CookieContainer { get; set; }

        private string _username;
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username
        {
            get { return _username; }
            set
            {
                if (_username != value)
                {
                    _username = value;
                    CookieContainer = null;
                }
            }
        }

        private string _password;
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    CookieContainer = null;
                }
            }
        }

        private async Task<Result> GetResult(HttpResponseMessage responseMessage)
        {
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            Result result;
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                result = (Result)new DataContractJsonSerializer(typeof(Result)).ReadObject(ms);
            }
            if (result.ErrorCode > 0)
            {
                if (result.ErrorCode == 4)
                {
                    throw new NotSupportedException(result.ErrorMessage);
                }
                throw new Exception(result.ErrorMessage);
            }
            return result;
        }

        /// <summary>
        /// Unrestrict a link
        /// </summary>
        /// <param name="uri">link to unrestrict</param>
        /// <returns>a premium link</returns>
        public override async Task<IPremiumLink> Unrestrict(string uri)
        {
            var realDebridBaseAddress = new Uri("https://www.real-debrid.com/ajax/");
            var cookieContainer = CookieContainer;
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
                CookieContainer = cookieContainer;
            }

            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            {
                var httpClient = new HttpClient(handler) { BaseAddress = realDebridBaseAddress };
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                var cookie = cookieContainer.GetCookies(realDebridBaseAddress)["auth"];
                if (cookie == null || cookie.Expired)
                {
                    var result = await GetResult(await httpClient.GetAsync(String.Format("login.php?user={0}&pass={1}", Uri.EscapeUriString(Username), MD5.GetHashString(Password).ToLower())));
                    cookie = cookieContainer.GetCookies(realDebridBaseAddress)["auth"];
                }

                if (cookie != null && !cookie.Expired)
                {
                    return await GetResult(await httpClient.GetAsync(String.Format("unrestrict.php?link={0}", Uri.EscapeUriString(uri))));
                }
            }
            return null;
        }
    }
}
