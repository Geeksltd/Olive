using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc.Microservices
{

    /// <summary>
    /// Represents a single widget that is displayed to the user.
    /// </summary>
    public class BoardHtml : BoardBox
    {
        public BoardHtml(Navigation navigation) : base(navigation) => this.Navigation = navigation;


        /// <summary>
        /// Url to which the user will be redirected for adding a new item.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string RawHtml;
        /// <summary>
        /// Permissions for acceess management
        /// </summary>
        public string Permissions;


    }
}
