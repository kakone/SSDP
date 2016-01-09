
namespace UPnP
{
    /// <summary>
    /// Utility class for URIs
    /// </summary>
    public static class UriUtility
    {
        /// <summary>
        /// Get the extension of a file
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns>the extension of a file</returns>
        public static string GetExtension(string uri)
        {
            if (uri == null)
                return null;

            var index = FindExtension(uri);
            if (index > -1 && index < uri.Length - 1)
                return uri.Substring(index);
            return string.Empty;
        }

        /// <summary>
        /// Gets the filename of the URI
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns>the filename of the URI</returns>
        public static string GetFileName(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return uri;

            int indexLastSep = uri.LastIndexOf('/');
            if (indexLastSep >= 0)
                return uri.Substring(indexLastSep + 1);

            return uri;
        }

        private static int FindExtension(string uri)
        {
            if (uri != null)
            {
                int indexLastDot = uri.LastIndexOf('.');
                int indexLastSep = uri.LastIndexOf('/');

                if (indexLastDot > indexLastSep)
                    return indexLastDot;
            }
            return -1;
        }
    }
}
