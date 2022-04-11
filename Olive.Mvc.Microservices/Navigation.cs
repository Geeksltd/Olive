using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Olive.Entities;

namespace Olive.Mvc.Microservices
{
    public abstract class Navigation
    {
        protected List<Feature> Features = new List<Feature>();
        protected List<BoardWidget> Boardwidgets = new List<BoardWidget>();
        protected List<BoardInfo> BoardInfos = new List<BoardInfo>();
        protected List<BoardHtml> BoardHtmls = new List<BoardHtml>();
        protected List<BoardMenue> BoardMenues = new List<BoardMenue>();

        internal List<Feature> GetFeatures() => Features;
        internal List<BoardWidget> GetBoardwidgets() => Boardwidgets;
        internal List<BoardInfo> GetBoardInfos() => BoardInfos;
        internal List<BoardHtml> GetBoardHtmls() => BoardHtmls;
        internal List<BoardMenue> GetBoardMenues() => BoardMenues;

        protected static IUrlHelper Url => new UrlHelper(new ActionContext(Context.Current.Http(), new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()));

        public abstract void Define();
        public abstract void DefineDynamic(ClaimsPrincipal user, GuidEntity board);
        protected void Add<TController>(string fullPath = null, string icon = null, string url = null, string desc = null, string @ref = null, string badgeUrl = null, bool showOnRight = false, bool iframe = false, string permissions = null, int? order = null) where TController : Controller
        {
            if (url.IsEmpty()) url = Url.Index<TController>();
            if (fullPath.IsEmpty())
                fullPath = typeof(TController).Name.TrimEnd("Controller");

            if (permissions.IsEmpty()) permissions = typeof(TController).GetCustomAttribute<AuthorizeAttribute>()?.Roles;

            Add(fullPath, url, permissions, icon, desc, @ref, badgeUrl, showOnRight, iframe, order);
        }
        protected void Add(string fullPath, string url, string permissions, string icon, string desc, string @ref, string badgeUrl, bool showOnRigh, bool iframe, int? order)
        {
            Add(new Feature
            {
                FullPath = fullPath,
                RelativeUrl = url,
                Permissions = permissions,
                Icon = icon,
                Description = desc,
                Refrance = @ref,
                BadgeUrl = badgeUrl,
                ShowOnRight = showOnRigh,
                Iframe = iframe,
                Order = order
            });
        }

        protected void AddMenu<TController>(string name, string icon = null, string url = null, string desc = null, string permissions = null) where TController : Controller
        {
            if (url.IsEmpty()) url = Url.Index<TController>();
            if (permissions.IsEmpty()) permissions = typeof(TController).GetCustomAttribute<AuthorizeAttribute>()?.Roles;

            Add(new BoardMenue
            {
                Name = name,
                Icon = icon,
                Url = url,
                Body = desc,
                Permissions = permissions
            });
        }



        protected BoardBox ForBox(string boxTitle, string colour)
        {
            return new BoardBox(this)
            {
                Title = boxTitle,
                Colour = colour
            };
        }
        protected void Add(Feature feature) => Features.Add(feature);
        internal void Add(BoardInfo boardInfo) => BoardInfos.Add(boardInfo);
        internal void Add(BoardWidget boardwidget) => Boardwidgets.Add(boardwidget);
        internal void Add(BoardHtml boardHtml) => BoardHtmls.Add(boardHtml);
        protected void Add(BoardMenue boardMenue) => BoardMenues.Add(boardMenue);
    }
}