namespace Domain
{
    using Olive;
    using System.Text;

    partial class Widget
    {
        public string Render(string id) => RenderHeader(id) + RenderBody(id);

        string RenderHeader(string id)
        {
            var r = new StringBuilder();
            r.Append("<header ");
            r.Append(Colour.WithWrappers("style='background-color:", "' "));
            r.AppendLine(">");
            r.AppendLine(Title);

            if (Settings != null && Context.Current.User().CanSee(Settings))
            {
                r.AppendLine($@"<a class=""pull-right mr-2"" href=""/{Settings.GetHubUrl().InjectId(id).ToLower()}"">");
                r.AppendLine(@"<i class=""fa fa-cog"" aria-hidden=""true""></i>");
                r.AppendLine("</a>");
            }

            r.AppendLine("</header>");

            return r.ToString();
        }

        string RenderBody(string id)
        {
            var url = Feature.GetAbsoluteImplementationUrl().InjectId(id);

            if (Feature.UseIframe)
                return $"<iframe src='{url}' sandbox=\"allow-forms allow-scripts allow-same-origin	allow-popups allow-top-navigation\"></iframe>";
            else
                return $"<service of='{Feature.Service.Name.ToLower()}'><Widget src='{url}'/></service>";
        }
    }
}
