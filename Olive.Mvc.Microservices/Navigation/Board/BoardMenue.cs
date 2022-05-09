using Newtonsoft.Json;

namespace Olive.Mvc.Microservices
{
    /// <summary>
    /// Represents a single item that is displayed to the user.
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class BoardMenu
    {
        /// <summary>
        /// Url to which the user will be redirected to manage these objects.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string Url { get; set; }

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
        public string Icon { get; set; }

        /// <summary>
        /// Permissions for acceess management
        /// </summary>
        public string Permissions { get; set; }
    }
}
