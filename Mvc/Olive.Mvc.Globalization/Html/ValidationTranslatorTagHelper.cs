// ******************************************
// TODO: Check this and turn into its own package.
// ******************************************

// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Razor.TagHelpers;
// using Olive.Globalization;

// namespace Olive.Mvc
// {
//    [HtmlTargetElement("select")]
//    [HtmlTargetElement("input")]
//    public class ValidationTranslatorTagHelper : TagHelper
//    {
//        static bool TranslateValidators = Config.Get("Globalization:TranslateValidators", defaultValue: false);
//        static string[] ValidationTextAttributes = new[] { "data-val-length", "data-val-required", "data-val-email" };

//        public override int Order => int.MaxValue;

//        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
//        {
//            if (TranslateValidators)
//            {
//                var temp = output.Attributes
//                    .Where(att => att.Name.IsAnyOf(ValidationTextAttributes))
//                    .Select(att => new { att.Name, Value = att.Value.ToString() }).ToArray();

//                foreach (var att in temp)
//                    output.Attributes.SetAttribute(att.Name, Translator.Translate(att.Value));
//            }

//            await base.ProcessAsync(context, output);
//        }
//    }
// }