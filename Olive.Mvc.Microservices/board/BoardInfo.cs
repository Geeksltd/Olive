namespace Olive.Mvc.Microservices
{
    /// <summary>
    /// Represents a single item that is displayed to the user.
    /// </summary>
    public class BoardInfo
    {
        /// <summary>
        /// Url For the Widget to be shown.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string FeatureUrl { get; set; }

        /// <summary>
        /// Url to which the user will be redirected to manage this widget.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string SettingsUrl { get; set; }

        /// <summary>
        /// Name of the search result. This is mandatory.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string IconUrl { get; set; }
    }
}
