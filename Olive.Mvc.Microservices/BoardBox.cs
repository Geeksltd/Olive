using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc.Microservices
{
    public class BoardBox
    {
        internal Navigation Navigation;

        public BoardBox(Navigation navigation)
        {
            Buttons = new List<BoxButton>();
            Navigation = navigation;
        }
        /// <summary>
        /// Url to which the user will be redirected. This is mandatory.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        internal string Colour { get; set; }
        /// <summary>
        /// Type of the item 
        /// </summary>
        internal string Title { get; set; }
        /// <summary>
        /// Buttons added to the top of the box
        /// </summary>
        internal List<BoxButton> Buttons { get; set; }
        public void AddButton(string icon, string url) => Buttons.Add(new BoxButton { Icon = icon, Url = url });
        public void Add(Action<BoardBox> action) => action(this);
        protected BoardBox AddFrom(string url, string permissions = null)
        {
            var tempBoardBox = new BoardWidget(Navigation)
            {
                Colour = Colour,
                Title = Title,
                Url = url,
                Buttons = Buttons,
                Permissions = permissions
            };
            Navigation.Add(tempBoardBox);
            return tempBoardBox;
        }
        protected BoardBox AddInfo(string name, string description, string icon, string url, BoardInfo.UrlTarget action = BoardInfo.UrlTarget.Redirect)
        {
            var tempBoardBox = new BoardInfo(Navigation)
            {
                Colour = Colour,
                Title = Title,
                Url = url,
                Name = name,
                Icon = icon,
                Description = description,
                Action = action,

            };
            Navigation.Add(tempBoardBox);
            return tempBoardBox;
        }
        protected BoardBox AddHtml(string rawHtml, string permissions = null)
        {
            var tempBoardBox = new BoardHtml(Navigation)
            {
                Colour = Colour,
                Title = Title,
                RawHtml = rawHtml,
                Buttons = Buttons,
                Permissions = permissions
            };
            Navigation.Add(tempBoardBox);
            return tempBoardBox;
        }
    }
}
