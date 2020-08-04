using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using Microsoft.AspNetCore.Localization;

namespace Olive.Mvc
{
    public class RecaptchaScriptTagHelper : TagHelper
    {
        private readonly IRecaptchaConfigurationService Service;
        private readonly IHttpContextAccessor ContextAccessor;

        private const string ScriptSnippet = @"
function recaptchaInitialScript() {{
    $.validator.setDefaults({{submitHandler:function(){{var e=this,r=''!==grecaptcha.getResponse(),a='{1}',t=$('#{0}', e.currentForm);if(t.length===0)return !0;return a&&(r?t.length&&t.hide():(e.errorList.push({{message:a}}),$(e.currentForm).triggerHandler('invalid-form',[e]),t.length&&(t.html(a),t.show()))),r}}}});
    $(page).off('initialized').on('initialized', function () {{
        var captchas = $('.g-recaptcha');
        captchas.each((index) => {{
            grecaptcha.render(captchas[index], {{
                'callback': function () {{
                    var r=$('#{0}');
                    r.length && r.hide();
                }}
            }});
        }});
    }});
}};
window.addEventListener('load', recaptchaInitialScript, false );
";

        private const string JqueryValidationAttributeName = "jquery-validation";
        private const string ValidationMessageElementIdAttributeName = "validation-message-element-id";

        public RecaptchaScriptTagHelper(IRecaptchaConfigurationService service, IHttpContextAccessor contextAccessor)
        {
            Service = service;
            ContextAccessor = contextAccessor;
        }

        [HtmlAttributeName(JqueryValidationAttributeName)]
        public bool? JqueryValidation { get; set; }

        [HtmlAttributeName(ValidationMessageElementIdAttributeName)]
        public string ValidationMessageElementId { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!Service.Enabled)
            {
                output.TagName = null;
                return;
            }

            var requestCulture = ContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>();
            var language = requestCulture?.RequestCulture?.UICulture?.Name ?? Service.LanguageCode;

            var javaScriptUrl = Service.JavaScriptUrl;

            if (language.HasValue())
                javaScriptUrl = $"{javaScriptUrl}?hl={language}&render=explicit";

            output.TagName = "script";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("src", javaScriptUrl);
            output.Attributes.Add("async", string.Empty);
            output.Attributes.Add("defer", string.Empty);

            if (JqueryValidation == false) return;

            var script = new TagBuilder("script")
            {
                TagRenderMode = TagRenderMode.Normal
            };

            script.InnerHtml.AppendHtml(
                ScriptSnippet.FormatWith(
                    ValidationMessageElementId, 
                    Service.ValidationMessage
                ));

            output.PostElement.AppendHtml(script);
        }
    }
}