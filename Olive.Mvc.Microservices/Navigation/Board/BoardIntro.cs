using Newtonsoft.Json;

namespace Olive.Mvc.Microservices
{
    /// <summary>
    /// Represents a single widget that is displayed to the user.
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class BoardIntro
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
        /// Url to the image to be shown on top.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Name of the search result. This is mandatory.
        /// </summary>
        public string Description { get; set; }
    }
}