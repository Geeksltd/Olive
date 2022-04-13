using Newtonsoft.Json;

namespace Olive.Mvc.Microservices
{
    /// <summary>
    /// Represents a single widget that is displayed to the user.
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]

    public class BoardHtml : BoardBoxContent
    {
        /// <summary>
        /// Url to which the user will be redirected for adding a new item.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string RawHtml { get; set; }
    }
}