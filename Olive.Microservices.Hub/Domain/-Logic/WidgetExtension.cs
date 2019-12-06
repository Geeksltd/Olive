namespace Domain
{
    public static class WidgetExtension
    {
        public static string RenderRightSide(this Widget @this, string id)
        {
            var url = $"{@this.Feature.GetAbsoluteImplementationUrl().InjectId(id)}/{id}";

            return "<div class=\"d-none d-lg-flex\" style=\"width: 250px;\"></div>" +
                "<div class=\"d-none d-lg-flex right-side\">" +
                $"    <iframe src='{url}' sandbox=\"allow-forms allow-scripts allow-same-origin	allow-popups allow-top-navigation\"></iframe>" +
                "</div>";
        }
    }
}
