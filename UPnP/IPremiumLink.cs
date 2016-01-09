namespace UPnP
{
    /// <summary>
    /// Interface for premium links
    /// </summary>
    public interface IPremiumLink
    {
        /// <summary>
        /// Gets the title of the media
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the main link
        /// </summary>
        string MainLink { get; }
    }
}
