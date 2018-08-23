using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;
using System.Linq;

namespace Olive.Mvc
{
    [HtmlTargetElement(Attributes = REPLACE_THIS_ATTRIBUTE_NAME + "," + WITH_THIS_ATTRIBUTE_NAME)]
    public class ReplaceIdentificationsTagHelper : TagHelper
    {
        const string REPLACE_THIS_ATTRIBUTE_NAME = "asp-replace-this";
        const string WITH_THIS_ATTRIBUTE_NAME = "asp-with-this";

        [HtmlAttributeName(REPLACE_THIS_ATTRIBUTE_NAME)]
        public string ReplaceThis { get; set; }

        [HtmlAttributeName(WITH_THIS_ATTRIBUTE_NAME)]
        public string WithThis { get; set; }

        public override int Order => 0;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            var newName = output.Attributes.LastOrDefault(att => att.Name == "name").Value.ToStringOrEmpty().Replace(ReplaceThis, WithThis);
            output.ReplaceIdentificationAttributes(newName);
        }
    }
}
