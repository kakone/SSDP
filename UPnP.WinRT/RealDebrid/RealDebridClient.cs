using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Authentication.Web;
using Windows.Security.Credentials;

namespace UPnP.RealDebrid
{
    /// <summary>
    /// Client for RealDebrid service
    /// </summary>
    public class RealDebridClient : PremiumLinkGenerator
    {
        /// <summary>
        /// Gets or sets the client identifier
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret string
        /// </summary>
        public string ClientSecret { get; set; }

        private string _applicationName;
        /// <summary>
        /// Gets or sets the application name
        /// </summary>
        public string ApplicationName
        {
            get { return _applicationName; }
            set
            {
                _applicationName = value;
                AccessToken = null;
                RefreshToken = null;
                try
                {
                    var passwordVault = new PasswordVault();
                    var credentials = passwordVault.FindAllByResource(value);
                    if (credentials != null)
                    {
                        AccessToken = GetToken(credentials, "AccessToken");
                        RefreshToken = GetToken(credentials, "RefreshToken");
                    }
                }
                catch (Exception) { }
            }
        }

        private string AccessToken { get; set; }

        private string RefreshToken { get; set; }

        private string GetToken(IEnumerable<PasswordCredential> credentials, string name)
        {
            var token = credentials.FirstOrDefault(c => c.UserName == name);
            if (token == null)
            {
                return null;
            }
            try { token.RetrievePassword(); } catch (Exception) { }
            return token.Password;
        }

        private async Task CheckAccessToken()
        {
            if (String.IsNullOrWhiteSpace(AccessToken))
            {
                var callBackUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().AbsoluteUri.Replace("ms-app", "http");
                var webAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None,
                    new Uri($"https://api.real-debrid.com/oauth/v2/auth?client_id={ClientId}&redirect_uri={callBackUri}&response_type=code&state=iloverd"),
                    new Uri(callBackUri));
                if (webAuthenticationResult.ResponseStatus != WebAuthenticationStatus.Success)
                {
                    throw new UnauthorizedAccessException();
                }
                var responseData = webAuthenticationResult.ResponseData.Substring(callBackUri.Length + 1).Split('&');
                var keyValuePairs = new Dictionary<string, string>();
                foreach (var str in responseData)
                {
                    var keyValue = str.Split('=');
                    keyValuePairs.Add(keyValue[0], keyValue[1]);
                }
                if (keyValuePairs["action"] != "allow" || keyValuePairs["state"] != "iloverd")
                {
                    throw new UnauthorizedAccessException();
                }

                await UpdateTokens(keyValuePairs["code"], "authorization_code");
            }
        }

        private async Task UpdateTokens(string code, string grantType)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.ExpectContinue = false;
            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("client_id", ClientId));
            parameters.Add(new KeyValuePair<string, string>("client_secret", ClientSecret));
            parameters.Add(new KeyValuePair<string, string>("code", code));
            parameters.Add(new KeyValuePair<string, string>("grant_type", grantType));
            var response = await httpClient.PostAsync("https://api.real-debrid.com/oauth/v2/token", new FormUrlEncodedContent(parameters));
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException();
            }
            response.EnsureSuccessStatusCode();
            var responseObject = JsonValue.Parse(await response.Content.ReadAsStringAsync()).GetObject();
            AccessToken = responseObject.GetNamedString("access_token");
            RefreshToken = responseObject.GetNamedString("refresh_token");
            var passwordVault = new PasswordVault();
            RemoveTokensFromPasswordVault(passwordVault);
            AddTokenToPasswordVault(passwordVault, "AccessToken", AccessToken);
            AddTokenToPasswordVault(passwordVault, "RefreshToken", RefreshToken);
        }

        private void RemoveTokensFromPasswordVault(PasswordVault passwordVault)
        {
            try
            {
                var credentials = passwordVault.FindAllByResource(ApplicationName);
                RemoveTokenFromPasswordVault(passwordVault, credentials, "AccessToken");
                RemoveTokenFromPasswordVault(passwordVault, credentials, "RefreshToken");
            }
            catch (Exception) { }
        }

        private void RemoveTokenFromPasswordVault(PasswordVault passwordVault, IEnumerable<PasswordCredential> credentials, string tokenName)
        {
            try
            {
                var credential = credentials.FirstOrDefault(c => c.UserName == tokenName);
                if (credential != null)
                {
                    passwordVault.Remove(credential);
                }
            }
            catch (Exception) { }
        }

        private void AddTokenToPasswordVault(PasswordVault passwordVault, string tokenName, string token)
        {
            try { passwordVault.Add(new PasswordCredential(ApplicationName, tokenName, token)); }
            catch (Exception) { }
        }

        private async Task<HttpClient> CreateHttpClient()
        {
            await CheckAccessToken();
            var httpClient = new HttpClient() { BaseAddress = new Uri("https://api.real-debrid.com/rest/1.0/") };
            httpClient.DefaultRequestHeaders.ExpectContinue = false;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            return httpClient;
        }

        /// <summary>
        /// Unrestrict a link
        /// </summary>
        /// <param name="uri">link to unrestrict</param>
        /// <returns>a premium link</returns>
        public override async Task<IPremiumLink> Unrestrict(string uri)
        {
            var httpClient = await CreateHttpClient();
            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("link", Uri.EscapeUriString(uri)));
            var responseMessage = await httpClient.PostAsync($"unrestrict/link", new FormUrlEncodedContent(parameters));
            switch (responseMessage.StatusCode)
            {
                case HttpStatusCode.ServiceUnavailable:
                    throw new NotSupportedException(responseMessage.ReasonPhrase);
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    if (!String.IsNullOrWhiteSpace(RefreshToken))
                    {
                        try { await UpdateTokens(RefreshToken, "http://oauth.net/grant_type/device/1.0"); }
                        catch (UnauthorizedAccessException)
                        {
                            AccessToken = null;
                            RefreshToken = null;
                            RemoveTokensFromPasswordVault(new PasswordVault());
                        }
                        return await Unrestrict(uri);
                    }
                    break;
            }
            responseMessage.EnsureSuccessStatusCode();
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(await responseMessage.Content.ReadAsStringAsync())))
            {
                return (IPremiumLink)new DataContractJsonSerializer(typeof(RealDebridLink)).ReadObject(ms);
            }
        }
    }
}