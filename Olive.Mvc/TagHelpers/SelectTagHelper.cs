using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Olive.Entities;

namespace Olive.Mvc
{
    [HtmlTargetElement("select", Attributes = "asp-for")]
    [HtmlTargetElement("select", Attributes = "asp-items")]
    public class SelectTagHelper : Microsoft.AspNetCore.Mvc.TagHelpers.SelectTagHelper
    {
        public SelectTagHelper(IHtmlGenerator generator) : base(generator) { }

        public override int Order => 0;

        Type ModelType => For?.Model?.GetType();

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // It should clear the original tag helper effects because it will call the base method again.
            output.PreContent.Clear();
            output.Content.Clear();
            output.PostContent.Clear();

            await base.ProcessAsync(context, output);

            if (ModelType?.IsA<IEntity>() ?? false)
            {
                var postContent = output.PostContent.GetContent();
                var stringId = ((IEntity)For.Model).GetId().ToString();
                postContent = postContent.Replace($"\"{stringId}\"", $"\"{stringId}\" selected=\"selected\"");
                output.PostContent.Clear();
                output.PostContent.SetHtmlContent(postContent);
            }
            else if (ModelType?.IsGenericOf(typeof(IEnumerable<>), typeof(IEntity)) ?? false)
            {
            }
        }
    }
}