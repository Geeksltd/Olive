namespace Olive.Mvc.Microservices
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    public class BoardBoxContent
    {
        /// <summary>
        /// Url to which the user will be redirected. This is mandatory.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        internal string BoxColour { get; set; }

        /// <summary>
        /// Type of the item 
        /// </summary>
        internal string BoxTitle { get; set; }

        /// <summary>
        /// Permissions for acceess management
        /// </summary>
        public string Permissions { get; set; }
    } 
}