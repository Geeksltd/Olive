namespace Olive.Mvc.Microservices
{
    /// <summary>
    /// Represents a single item that is displayed to the user.
    /// </summary>
    public class BoardInfo
    {
        /// <summary>
        /// Url to which the user will be redirected for adding a new item.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string AddUrl { get; set; }

        /// <summary>
        /// Url to which the user will be redirected to manage these objects.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string ManageUrl { get; set; }

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
