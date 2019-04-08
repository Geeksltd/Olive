namespace Olive.Mvc
{
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using System.Linq;

    [HtmlTargetElement(Attributes = PREFIX_ATTRIBUTE_NAME)]
    public class PrefixIdentificationsTagHelper : TagHelper
    {
        const string PREFIX_ATTRIBUTE_NAME = "asp-prefix";

        [HtmlAttributeName(PREFIX_ATTRIBUTE_NAME)]
        public string Prefix { get; set; }

        public override int Order => 0;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            var newName = $"{Prefix.TrimEnd(".")}.{output.Attributes.LastOrDefault(att => att.Name == "name").Value}";
            output.ReplaceIdentificationAttributes(newName);
        }
    }
}