using System;

namespace UPnP.RealDebrid
{
    /// <summary>
    /// Event args for AuthenticateEvent
    /// </summary>
    public class AuthenticateEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of AuthenticateEventArgs class
        /// </summary>
        /// <param name="requestUri">request URI</param>
        /// <param name="callbackUri">callback URI</param>
        public AuthenticateEventArgs(Uri requestUri, Uri callbackUri)
        {
            RequestUri = requestUri;
            CallbackUri = callbackUri;
        }

        /// <summary>
        /// Gets the request URI
        /// </summary>
        public Uri RequestUri { get; private set; }

        /// <summary>
        /// Gets the callback URI
        /// </summary>
        public Uri CallbackUri { get; private set; }
    }
}
