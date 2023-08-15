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

        public void AddFrom(string url)
        {
            Add(new BoardWidget { Url = url });
        }

        public void AddInfo(string title, string description, string icon, string url, UrlTarget action = UrlTarget.Redirect)
        {
            var result = new BoardInfo
            {
                Url = url,
                Name = title,
                Icon = icon,
                Description = description,
                Action = action,
            };

            Add(result);
        }

        public void AddHtml(string rawHtml)
        {
            Add(new BoardHtml { RawHtml = rawHtml });
        }

        public void AddButton(string icon, string url, string text = null, string tooltip = null, UrlTarget action = UrlTarget.Redirect)
            => Add(new BoardButton { Icon = icon, Url = url, Text = text, Tooltip = tooltip, Action = action });

    }
}