using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Olive.Entities;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    [HtmlTargetElement("input", Attributes = "[asp-for],[type=hidden]")]
    public class HiddenTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (For?.Model is IEntity ent)
                output.Attributes.SetAttribute("value", ent.GetId().ToString());

            return Task.CompletedTask;
        }
    }
}