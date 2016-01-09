namespace UPnP
{
    /// <summary>
    /// Response object to M-SEARCH message
    /// </summary>
    public class DeviceNotification
    {
        /// <summary>
        /// Gets or sets the location of the device
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the unique service name
        /// </summary>
        public string USN { get; set; }
    }
}
