using System.Collections.Generic;

namespace Olive.Mvc.Microservices
{
    /// <summary>
    /// Represents a single item that is displayed to the user.
    /// </summary>
    public class BoardInfo : BoardBox
    {
        public enum UrlTarget
        {
            Redirect,
            Popup,
            NewWindow
        }
        public BoardInfo(Navigation navigation) : base(navigation) => this.Navigation = navigation;

        /// <summary>
        /// Url to which the user will be redirected. This is mandatory.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Name of the search result. This is mandatory.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Body.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// How you want it to be presented.
        /// </summary>
        public UrlTarget Action { get; set; }
    }
}
