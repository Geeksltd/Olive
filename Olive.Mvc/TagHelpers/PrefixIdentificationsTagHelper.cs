using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Olive.Mvc
{
    [HtmlTargetElement(Attributes = PREFIX_ATTRIBUTE_NAME)]
    public class PrefixIdentificationsTagHelper : TagHelper
    {
        const string PREFIX_ATTRIBUTE_NAME = "asp-prefix";

        [HtmlAttributeName(PREFIX_ATTRIBUTE_NAME)]
        public string Prefix { get; set; }

        public override int Order => 0;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            var newName = $"{Prefix}.{output.Attributes.FirstOrDefault(att => att.Name == "name").Value}";

            output.ReplaceIdentificationAttributes(newName);
        }
    }
}
