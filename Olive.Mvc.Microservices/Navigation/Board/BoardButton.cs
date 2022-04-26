using Newtonsoft.Json;

namespace Olive.Mvc.Microservices
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class BoardButton : BoardBoxContent
    {
        /// <summary>
        /// Icon of the button to be added at the top of Box.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Url to which the user will be redirected after clicking the button.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The text of the button to show next to the icon (optional).
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Tooltip description to show to the user.
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// How you want it to be presented.
        /// </summary>
        public UrlTarget Action { get; set; }
    }
}
