namespace Olive.Mvc.Microservices
{
    /// <summary>
    /// Represents a single item that is displayed to the user.
    /// </summary>
    public class BoardComponentsResult
    {
        /// <summary>
        /// Url to which the user will be redirected. This is mandatory.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Type of the item 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Name of the search result. This is mandatory.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Background colour of the rendered component.
        /// </summary>
        public string Colour { get; set; }
    }
}
