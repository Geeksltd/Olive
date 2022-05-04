namespace Olive.Mvc.Microservices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Olive.Entities;

    public abstract class Navigation
    {
        protected readonly List<Feature> Features = new();
        protected readonly List<BoardMenue> BoardMenues = new();
        protected readonly List<BoardIntro> BoardIntros = new();
        protected internal readonly List<BoardBoxContent> BoardContents = new();
        public IDatabase Database => Context.Current.Database();
        internal List<Feature> GetFeatures() => Features;

        internal IEnumerable<BoardWidget> GetBoardwidgets() => BoardContents.OfType<BoardWidget>();

        internal IEnumerable<BoardInfo> GetBoardInfos() => BoardContents.OfType<BoardInfo>();

        internal IEnumerable<BoardHtml> GetBoardHtmls() => BoardContents.OfType<BoardHtml>();

        internal IEnumerable<BoardButton> GetBoardButtons() => BoardContents.OfType<BoardButton>();

        internal IEnumerable<BoardMenue> GetBoardMenues() => BoardMenues;

        internal IEnumerable<BoardIntro> GetBoardIntros() => BoardIntros;

        public abstract void Define();

        public virtual async Task<GuidEntity> GetBoardObjectFromText(Type type, string id) => null;

        protected void Add<TController>(string fullPath = null, string icon = null, string url = null, string desc = null, string @ref = null, string badgeUrl = null, bool showOnRight = false, bool iframe = false, string permissions = null, int? order = null) where TController : Controller
        {
            var controller = (TController)Activator.CreateInstance(typeof(TController));

            if (url.IsEmpty()) url = controller.Url.Index<TController>();
            if (fullPath.IsEmpty()) fullPath = typeof(TController).Name.TrimEnd("Controller");
            if (permissions.IsEmpty()) permissions = typeof(TController).GetCustomAttribute<AuthorizeAttribute>()?.Roles;

            Add(fullPath, url, permissions, icon, desc, @ref, badgeUrl, order, showOnRight, iframe);
        }

        protected void Add(string fullPath, string url, string permissions, string icon, string desc, string @ref, string badgeUrl, int? order, bool showOnRigh, bool iframe)
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
            var controller = (TController)Activator.CreateInstance(typeof(TController));
            if (url.IsEmpty()) url = controller.Url.Index<TController>();
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

        protected void AddIntro(string name, string url, string imageUrl = null, string desc = null)
        {
            Add(new BoardIntro
            {
                Name = name,
                Url = url,
                ImageUrl = imageUrl,
                Description = desc
            });
        }

        protected BoardBox ForBox(string boxTitle, string colour) => new BoardBox(this) { Title = boxTitle, Colour = colour };

        protected void Add(Feature feature) => Features.Add(feature);

        protected void Add(BoardMenue boardMenue) => BoardMenues.Add(boardMenue);

        protected void Add(BoardIntro boardIntro) => BoardIntros.Add(boardIntro);
    }
}