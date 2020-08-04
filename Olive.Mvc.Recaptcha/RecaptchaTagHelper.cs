using System;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Olive.Mvc
{
    public class RecaptchaTagHelper : TagHelper
    {
        private readonly IRecaptchaConfigurationService Service;

        private const string ThemeAttributeName = "theme";
        private const string TypeAttributeName = "type";
        private const string SizeAttributeName = "size";
        private const string TabindexAttributeName = "tabindex";

        [HtmlAttributeName(ThemeAttributeName)]
        public RecaptchaTheme? Theme { get; set; }

        [HtmlAttributeName(TypeAttributeName)]
        public RecaptchaType? Type { get; set; }

        [HtmlAttributeName(SizeAttributeName)]
        public RecaptchaSize? Size { get; set; }

        [HtmlAttributeName(TabindexAttributeName)]
        public int TabIndex { get; set; }

        public RecaptchaTagHelper(IRecaptchaConfigurationService service)
        {
            Service = service;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!Service.Enabled)
            {
                output.TagName = null;
                return;
            }

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("class", "g-recaptcha");
            output.Attributes.Add("data-sitekey", Service.SiteKey);

            var controlSettings = Service.ControlSettings;

            if ((Theme ?? controlSettings.Theme) == RecaptchaTheme.Dark)
            {
                output.Attributes.Add("data-theme", "dark");
            }

            if ((Type ?? controlSettings.Type) == RecaptchaType.Audio)
            {
                output.Attributes.Add("data-type", "audio");
            }

            if ((Size ?? controlSettings.Size) == RecaptchaSize.Compact)
            {
                output.Attributes.Add("data-size", "compact");
            }

            if (TabIndex != 0)
            {
                output.Attributes.Add("data-tabindex", TabIndex);
            }
        }
    }
}