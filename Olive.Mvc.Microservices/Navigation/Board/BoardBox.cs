namespace Olive.Mvc.Microservices
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    public enum UrlTarget
    {
        Redirect,
        Popup,
        NewWindow
    }
    public class BoardBox
    {
        Navigation Navigation;

        internal BoardBox(Navigation navigation) => Navigation = navigation;

        /// <summary>
        /// Url to which the user will be redirected. This is mandatory.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        internal string Colour { get; set; }

        /// <summary>
        /// Type of the item 
        /// </summary>
        internal string Title { get; set; }

        public void Add(Action<BoardBox> action) => action(this);

        BoardBoxContent Add(BoardBoxContent content)
        {
            content.BoxColour = Colour;
            content.BoxTitle = Title;
            Navigation.BoardContents.Add(content);
            return content;
        }

        public void AddFrom(string url, string permissions = null)
        {
            Add(new BoardWidget { Url = url, Permissions = permissions });
        }

        public void AddInfo(string title, string description, string icon, string url, string permissions = null, UrlTarget action = UrlTarget.Redirect)
        {
            var result = new BoardInfo
            {
                Url = url,
                Name = title,
                Icon = icon,
                Description = description,
                Action = action,
                Permissions = permissions
            };

            Add(result);
        }

        public void AddHtml(string rawHtml, string permissions = null)
        {
            Add(new BoardHtml { RawHtml = rawHtml, Permissions = permissions });
        }

        public void AddButton(string icon, string url, string text = null, string tooltip = null, string permissions = null, UrlTarget action = UrlTarget.Redirect)
            => Add(new BoardButton { Icon = icon, Url = url, Text = text, Tooltip = tooltip, Permissions = permissions, Action = action });

    }
}