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

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // It should clear the original tag helper effects because it will call the base method again.
            output.PreContent.Clear();
            output.Content.Clear();
            output.PostContent.Clear();

            await base.ProcessAsync(context, output);

            if (For.Model is IEntity ent)
            {
                SetSelected(output.PostContent, ent);
            }
            else if (For.Model is IEnumerable<IEntity> entities)
            {
                SetSelected(output.PostContent, entities);
            }
        }

        void SetSelected(TagHelperContent content, IEntity entity)
        {
            var postContent = content.GetContent();
            postContent = postContent.Replace($"\"{entity.GetId()}\"", $"\"{entity.GetId()}\" selected=\"selected\"");
            content.Clear();
            content.SetHtmlContent(postContent);
        }

        void SetSelected(TagHelperContent content, IEnumerable<IEntity> entities)
        {
            var postContent = content.GetContent();

            foreach (var entity in entities)
                postContent = postContent.Replace($"\"{entity.GetId()}\"", $"\"{entity.GetId()}\" selected=\"selected\"");

            content.Clear();
            content.SetHtmlContent(postContent);
        }
    }
}