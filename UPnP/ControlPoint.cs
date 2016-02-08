using System;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace UPnP
{
    /// <summary>
    /// Base class for a control point
    /// </summary>
    public abstract class ControlPoint
    {
        /// <summary>
        /// XML SOAP envelope
        /// </summary>
        private const string SOAP_ENVELOPE = @"<?xml version=""1.0"" encoding=""utf-8""?>
<s:Envelope encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
    <s:Body>
        {0}
    </s:Body>
</s:Envelope>";

        /// <summary>
        /// Create a new HttpClient (disabling Expect:100-Continue header)
        /// </summary>
        /// <returns></returns>
        protected HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.ExpectContinue = false;
            return httpClient;
        }

        /// <summary>
        /// Gets the absolute URL for a service
        /// </summary>
        /// <param name="device">UPnP device to control</param>
        /// <param name="service">UPnP service</param>
        /// <returns>the absolute URL for the service</returns>
        protected Uri GetControlUri(Device device, Service service)
        {
            var requestUri = new Uri(service.ControlURL, UriKind.RelativeOrAbsolute);
            if (!requestUri.IsAbsoluteUri || requestUri.IsFile) // In Mono.Android, requestUri is not a relative uri but a file
            {
                requestUri = new Uri(new Uri(device.URLBase), requestUri);
            }
            return requestUri;
        }

        /// <summary>
        /// Action call
        /// </summary>
        /// <param name="httpClient">HTTPClient</param>
        /// <param name="service">UPnP service</param>
        /// <param name="controlURL">control URL</param>
        /// <param name="action">action</param>
        /// <param name="headers">optional HTTP headers</param>
        /// <returns>HTTP response message</returns>
        protected async Task<HttpResponseMessage> PostActionAsync(HttpClient httpClient, Service service, Uri controlURL, object action,
            params Tuple<string, string>[] headers)
        {
            var xmlRootAttribute = action.GetType().GetTypeInfo().GetCustomAttribute<XmlRootAttribute>();
            xmlRootAttribute.Namespace = service.ServiceType;
            var request = new StringContent(String.Format(SOAP_ENVELOPE, XmlSerializerUtility.Serialize(action, xmlRootAttribute,
                  new XmlQualifiedName("u", service.ServiceType))), Encoding.UTF8, "text/xml");
            request.Headers.Add("SOAPAction", $"\"{service.ServiceType}#{xmlRootAttribute.ElementName}\"");
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Item1, header.Item2);
                }
            }

            return await httpClient.PostAsync(controlURL, request);
        }
    }
}
