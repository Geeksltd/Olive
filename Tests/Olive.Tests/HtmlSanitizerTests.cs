using Ganss.Xss;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;
using Olive.Mvc;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Olive.Tests
{
    /// <summary>
    /// Tests for the string.Raw() extension and the policy it uses (HtmlSanitizerFactory):
    /// the permissive library Default, the strict CSP-style policy an app supplies via config,
    /// and the ShowRemoved / ShowRemovedWhen markers.
    /// </summary>
    [TestFixture]
    public class HtmlSanitizerTests
    {
        static string GetHtml(HtmlString html)
        {
            using var writer = new StringWriter();
            html.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }

        // The strict, CSP-style policy that ags.knowledge supplies via the "Html:Sanitizer" config.
        // .Raw() uses the permissive library Default (which keeps style, no video/iframe/data/aria);
        // the tests below that need the strict behaviour build it explicitly via Create().
        static readonly HtmlSanitizer Strict = HtmlSanitizerFactory.Create(new HtmlSanitizerSettings
        {
            AllowedSchemes = new[] { "http", "https", "mailto", "tel" },
            AllowTags = new[] { "video", "source", "iframe" },
            AllowAttributes = new[]
            {
                "id", "class", "role",
                "controls", "autoplay", "loop", "muted", "preload", "poster",
                "frameborder", "allow", "allowfullscreen"
            },
            RemoveAttributes = new[] { "style" },
            UriAttributes = new[] { "poster" },
            AllowedFrameDomains = new[] { "youtube.com", "youtube-nocookie.com", "vimeo.com", "lsi.ac.uk" },
            AllowDataAttributes = true,
            AllowAriaAttributes = true,
            KeepChildNodes = true
        });

        [Test]
        public void Raw_DefaultSanitize_RemovesScriptTags()
        {
            var input = "<p>Hello</p><script>alert('xss')</script>";

            var result = GetHtml(input.Raw());

            Assert.That(result, Does.Not.Contain("<script"));
            Assert.That(result, Does.Contain("<p>Hello</p>"));
        }

        [Test]
        public void Raw_WithSanitizeTrue_RemovesEventHandlers()
        {
            var input = "<p id=\"p1\" class=\"text\" onclick=\"evil()\">Click</p>";

            var result = GetHtml(input.Raw(sanitize: true));

            result.ShouldEqual("<p id=\"p1\" class=\"text\">Click</p>");
        }

        [Test]
        public void Raw_WithSanitizeTrue_RemovesJavascriptUrls()
        {
            var input = "<a href=\"javascript:alert(1)\">link</a>";

            var result = GetHtml(input.Raw(sanitize: true));

            result.ShouldEqual("<a>link</a>");
        }

        [Test]
        public void Raw_WithSanitizeTrue_KeepsAllowedLinkSchemes()
        {
            var mailto = GetHtml("<a href=\"mailto:test@example.com\">email</a>".Raw());
            var tel = GetHtml("<a href=\"tel:+1234567890\">phone</a>".Raw());
            var http = GetHtml("<a href=\"http://example.com\">http</a>".Raw());
            var https = GetHtml("<a href=\"https://example.com\">https</a>".Raw());

            mailto.ShouldEqual("<a href=\"mailto:test@example.com\">email</a>");
            tel.ShouldEqual("<a href=\"tel:+1234567890\">phone</a>");
            http.ShouldEqual("<a href=\"http://example.com\">http</a>");
            https.ShouldEqual("<a href=\"https://example.com\">https</a>");
        }

        [Test]
        public void Raw_WithSanitizeTrue_KeepsStyleAttribute()
        {
            // The library Default (commit 1ad7ed4 baseline) keeps inline style.
            var input = "<span style=\"color:red\">styled</span>";

            var result = GetHtml(input.Raw(sanitize: true));

            Assert.That(result, Does.Contain("style="));
            Assert.That(result, Does.Contain("styled"));
            Assert.That(result, Does.Not.Contain("javascript:"));
        }

        [Test]
        public void Create_StrictPolicy_RemovesStyleAttribute()
        {
            // The strict config drops style; the element and its class survive.
            var result = Strict.Sanitize("<span style=\"color:red\" class=\"c\">styled</span>");

            result.ShouldEqual("<span class=\"c\">styled</span>");
        }

        [Test]
        public void Raw_WithSanitizeTrue_RemovesStyleElement()
        {
            var result = GetHtml("<p>Hi</p><style>.a{color:red}</style>".Raw());

            Assert.That(result, Does.Not.Contain("<style"));
            Assert.That(result, Does.Contain("<p>Hi</p>"));
        }

        [Test]
        public void Create_StrictPolicy_KeepsDataAttributes()
        {
            var result = Strict.Sanitize("<div data-status=\"ready\" data-item-id=\"42\">hi</div>");

            Assert.That(result, Does.Contain("data-status=\"ready\""));
            Assert.That(result, Does.Contain("data-item-id=\"42\""));
        }

        [Test]
        public void Create_StrictPolicy_KeepsAriaAttributesAndRole()
        {
            var result = Strict.Sanitize("<button role=\"button\" aria-label=\"Close\" aria-pressed=\"false\">X</button>");

            Assert.That(result, Does.Contain("role=\"button\""));
            Assert.That(result, Does.Contain("aria-label=\"Close\""));
            Assert.That(result, Does.Contain("aria-pressed=\"false\""));
        }

        [Test]
        public void Create_StrictPolicy_KeepsVideoAndSource()
        {
            var input = "<video controls poster=\"https://ex.com/p.jpg\">" +
                        "<source src=\"https://ex.com/v.mp4\" type=\"video/mp4\"></video>";

            var result = Strict.Sanitize(input);

            Assert.That(result, Does.Contain("<video"));
            Assert.That(result, Does.Contain("controls"));
            Assert.That(result, Does.Contain("poster=\"https://ex.com/p.jpg\""));
            Assert.That(result, Does.Contain("<source"));
            Assert.That(result, Does.Contain("src=\"https://ex.com/v.mp4\""));
            Assert.That(result, Does.Contain("type=\"video/mp4\""));
        }

        [Test]
        public void Create_StrictPolicy_RemovesVideoEventHandlerAndUnsafeSourceScheme()
        {
            var input = "<video onerror=\"evil()\">" +
                        "<source src=\"javascript:alert(1)\" type=\"video/mp4\"></video>";

            var result = Strict.Sanitize(input);

            Assert.That(result, Does.Contain("<video"));
            Assert.That(result, Does.Not.Contain("onerror"));
            Assert.That(result, Does.Not.Contain("javascript:"));
        }

        [Test]
        public void Raw_WithSanitizeTrue_RemovesDataUriScheme()
        {
            var input = "<img src=\"data:image/png;base64,AAAA\">";

            var result = GetHtml(input.Raw(sanitize: true));

            Assert.That(result, Does.Not.Contain("data:"));
        }

        [Test]
        public void Create_StrictPolicy_RemovesUnsafePosterScheme()
        {
            var input = "<video poster=\"data:image/png;base64,AAAA\">" +
                        "<source src=\"https://ex.com/v.mp4\" type=\"video/mp4\"></video>";

            var result = Strict.Sanitize(input);

            Assert.That(result, Does.Contain("<video"));
            Assert.That(result, Does.Contain("<source"));
            Assert.That(result, Does.Not.Contain("poster"));
            Assert.That(result, Does.Not.Contain("data:"));
        }

        [Test]
        public void Raw_WithSanitizeTrue_RemovesNonWhitelistedSchemes()
        {
            var ftp = GetHtml("<a href=\"ftp://ex.com/file\">ftp</a>".Raw());
            var file = GetHtml("<a href=\"file:///etc/passwd\">file</a>".Raw());

            ftp.ShouldEqual("<a>ftp</a>");
            file.ShouldEqual("<a>file</a>");
        }

        [Test]
        public void Create_StrictPolicy_KeepsMultipleAriaAttributes()
        {
            var input = "<div aria-hidden=\"true\" aria-live=\"polite\" " +
                        "aria-atomic=\"true\" aria-expanded=\"false\" aria-controls=\"panel\">c</div>";

            var result = Strict.Sanitize(input);

            Assert.That(result, Does.Contain("aria-hidden=\"true\""));
            Assert.That(result, Does.Contain("aria-live=\"polite\""));
            Assert.That(result, Does.Contain("aria-atomic=\"true\""));
            Assert.That(result, Does.Contain("aria-expanded=\"false\""));
            Assert.That(result, Does.Contain("aria-controls=\"panel\""));
        }

        [Test]
        public void Raw_WithSanitizeTrue_RemovesObject()
        {
            var result = GetHtml("<object data=\"https://ex.com/x.swf\">obj</object>".Raw());

            Assert.That(result, Does.Not.Contain("<object"));
        }

        [Test]
        public void Create_StrictPolicy_KeepsIframeFromTrustedHost()
        {
            var input = "<iframe width=\"560\" height=\"315\" src=\"https://www.youtube.com/embed/abc123\" " +
                        "frameborder=\"0\" allow=\"autoplay; encrypted-media\" allowfullscreen></iframe>";

            var result = Strict.Sanitize(input);

            Assert.That(result, Does.Contain("<iframe"));
            Assert.That(result, Does.Contain("src=\"https://www.youtube.com/embed/abc123\""));
            Assert.That(result, Does.Contain("allowfullscreen"));
            Assert.That(result, Does.Contain("frameborder=\"0\""));
            Assert.That(result, Does.Contain("width=\"560\""));
        }

        [Test]
        public void Create_StrictPolicy_KeepsIframeFromOtherTrustedDomains()
        {
            var vimeo = Strict.Sanitize("<iframe src=\"https://player.vimeo.com/video/1\"></iframe>");
            var subdomain = Strict.Sanitize("<iframe src=\"https://learn.lsi.ac.uk/x\"></iframe>");
            var apex = Strict.Sanitize("<iframe src=\"https://lsi.ac.uk/x\"></iframe>");

            Assert.That(vimeo, Does.Contain("src=\"https://player.vimeo.com/video/1\""));
            Assert.That(subdomain, Does.Contain("src=\"https://learn.lsi.ac.uk/x\""));
            Assert.That(apex, Does.Contain("src=\"https://lsi.ac.uk/x\""));
        }

        [Test]
        public void Create_StrictPolicy_DropsIframeSrcFromUntrustedHost()
        {
            // Each of these must lose its src, so the frame loads nothing.
            var evil = Strict.Sanitize("<iframe src=\"https://evil.com/x\"></iframe>");
            var lookalike = Strict.Sanitize("<iframe src=\"https://evil-youtube.com/x\"></iframe>");
            var notHttps = Strict.Sanitize("<iframe src=\"http://www.youtube.com/embed/x\"></iframe>");
            var protocolRelative = Strict.Sanitize("<iframe src=\"//evil.com/x\"></iframe>");
            var javascriptUrl = Strict.Sanitize("<iframe src=\"javascript:alert(1)\"></iframe>");

            Assert.That(evil, Does.Not.Contain("evil.com"));
            Assert.That(lookalike, Does.Not.Contain("evil-youtube.com"));
            Assert.That(notHttps, Does.Not.Contain("http://"));
            Assert.That(protocolRelative, Does.Not.Contain("evil.com"));
            Assert.That(javascriptUrl, Does.Not.Contain("javascript:"));
        }

        [Test]
        public void Create_StrictPolicy_IframeHostFilterDoesNotAffectLinksOrImages()
        {
            // Only iframes are host-restricted; ordinary links/images keep the scheme-only rules.
            var link = Strict.Sanitize("<a href=\"https://evil.com/x\">l</a>");
            var image = Strict.Sanitize("<img src=\"https://evil.com/i.png\">");

            Assert.That(link, Does.Contain("href=\"https://evil.com/x\""));
            Assert.That(image, Does.Contain("src=\"https://evil.com/i.png\""));
        }

        [Test]
        public void Create_StrictPolicy_KeepsDataAriaAndRoleTogether()
        {
            var result = Strict.Sanitize("<div data-x=\"1\" aria-label=\"label\" role=\"note\">t</div>");

            Assert.That(result, Does.Contain("data-x=\"1\""));
            Assert.That(result, Does.Contain("aria-label=\"label\""));
            Assert.That(result, Does.Contain("role=\"note\""));
        }

        [Test]
        public void Raw_WithSanitizeTrue_RemovesScriptElementButKeepsChildText()
        {
            var result = GetHtml("<p>Hi</p><script>removed()</script>".Raw());

            result.ShouldEqual("<p>Hi</p>removed()");
        }

        [Test]
        public void Raw_WithSanitizeFalse_PreservesUnsafeHtml()
        {
            var input = "<p onclick=\"evil()\">unsafe</p><script>alert(1)</script>";

            var result = GetHtml(input.Raw(sanitize: false));

            result.ShouldEqual(input);
        }

        [Test]
        public void Raw_NullOrEmpty_ReturnsEmptyHtmlString()
        {
            GetHtml(((string)null).Raw()).ShouldEqual(string.Empty);
            GetHtml(string.Empty.Raw()).ShouldEqual(string.Empty);
        }

        [Test]
        public async Task Raw_Task_DefaultSanitize_RemovesScriptTags()
        {
            var result = GetHtml(await Task.FromResult("<b>ok</b><script>x</script>").Raw());

            Assert.That(result, Does.Not.Contain("<script"));
            Assert.That(result, Does.Contain("<b>ok</b>"));
        }

        [Test]
        public async Task Raw_Task_WithSanitizeFalse_PreservesUnsafeHtml()
        {
            var input = "<img onerror=\"evil()\" src=\"x\">";

            var result = GetHtml(await Task.FromResult(input).Raw(sanitize: false));

            result.ShouldEqual(input);
        }

        // ---- TrustedRaw: the named way to say "this HTML is trusted, do not clean it" ----

        [Test]
        public void TrustedRaw_PreservesUnsafeHtml()
        {
            var input = "<p onclick=\"evil()\">unsafe</p><script>alert(1)</script>";

            GetHtml(input.TrustedRaw()).ShouldEqual(input);
        }

        [Test]
        public void TrustedRaw_NullOrEmpty_ReturnsEmptyHtmlString()
        {
            GetHtml(((string)null).TrustedRaw()).ShouldEqual(string.Empty);
            GetHtml(string.Empty.TrustedRaw()).ShouldEqual(string.Empty);
        }

        [Test]
        public async Task TrustedRaw_Task_PreservesUnsafeHtml()
        {
            var input = "<img onerror=\"evil()\" src=\"x\">";

            GetHtml(await Task.FromResult(input).TrustedRaw()).ShouldEqual(input);
        }

        // ---- HtmlSanitizerFactory: config-driven policy + ShowRemoved markers ----
        // These build a sanitizer directly (Create) so they test the policy mapping without
        // touching the cached shared instance or needing an initialized Context.

        [Test]
        public void HtmlSanitizerFactory_Create_AppliesConfiguredPolicy()
        {
            // Only vimeo is a trusted frame host; "class" is not allowed here.
            var sanitizer = HtmlSanitizerFactory.Create(new HtmlSanitizerSettings
            {
                AllowedSchemes = new[] { "http", "https" },
                AllowTags = new[] { "iframe" },
                AllowAttributes = new[] { "id" },
                AllowedFrameDomains = new[] { "vimeo.com" },
                KeepChildNodes = true
            });

            var youtube = sanitizer.Sanitize("<iframe src=\"https://www.youtube.com/embed/x\"></iframe>");
            var vimeo = sanitizer.Sanitize("<iframe src=\"https://player.vimeo.com/video/1\"></iframe>");
            var classed = sanitizer.Sanitize("<p class=\"c\" id=\"p1\">x</p>");

            Assert.That(youtube, Does.Not.Contain("youtube.com")); // src dropped: not a trusted host
            Assert.That(vimeo, Does.Contain("player.vimeo.com"));   // src kept: trusted host
            Assert.That(classed, Does.Not.Contain("class="));       // class not in the configured list
            Assert.That(classed, Does.Contain("id=\"p1\""));        // id is
        }

        [Test]
        public void HtmlSanitizerFactory_ShowRemoved_ReplacesRemovedTagWithRedSpan()
        {
            var settings = HtmlSanitizerFactory.Default;
            settings.ShowRemoved = true;
            settings.ShowRemovedWhen = () => true; // markers are off unless this allows them
            var sanitizer = HtmlSanitizerFactory.Create(settings);

            var result = sanitizer.Sanitize("<div>Hi <script>alert(1)</script> Bye</div>");

            Assert.That(result, Does.Contain("<span class=\"removed-tag\">[REMOVED: SCRIPT]</span>"));
            Assert.That(result, Does.Not.Contain("alert(1)")); // inner content dropped
        }

        [Test]
        public void HtmlSanitizerFactory_ShowRemoved_KeepsAttributeValueButRemovesLiveHandler()
        {
            var settings = HtmlSanitizerFactory.Default;
            settings.ShowRemoved = true;
            settings.ShowRemovedWhen = () => true;
            var sanitizer = HtmlSanitizerFactory.Create(settings);

            var result = sanitizer.Sanitize("<img src=\"a.jpg\" onerror=\"alert(1)\">");

            // The value survives only as an inert marker...
            Assert.That(result, Does.Contain("data-removed-onerror=\"alert(1)\""));
            Assert.That(result, Does.Contain("class=\"removed-attr\""));
            // ...and there is NO live onerror handler (guards the Cancel-keeps-the-attribute trap).
            // A live attribute looks like  onerror="...  (space + quote); the marker/badge do not.
            Assert.That(result, Does.Not.Contain(" onerror=\""));
        }

        [Test]
        public void HtmlSanitizerFactory_ShowRemoved_ShowsRemovedValueOnImageViaTitle()
        {
            // <img> is a replaced element: CSS ::after cannot render on it, so the sanitizer sets a
            // `title` tooltip carrying the removed value. No extra DOM node -> re-running Raw() is safe.
            var sanitizer = HtmlSanitizerFactory.Create(new HtmlSanitizerSettings
            {
                AllowedSchemes = new[] { "http", "https" },
                AllowAttributes = new[] { "id", "class" },
                RemoveAttributes = new[] { "style" }, // style is default-allowed, so drop it explicitly
                KeepChildNodes = true,
                ShowRemoved = true,
                ShowRemovedWhen = () => true
            });

            var result = sanitizer.Sanitize("<img src=\"a.webp\" style=\"max-width:100%\">");

            Assert.That(result, Does.Contain("title=\"⚠ removed: style=max-width:100%\""));
            Assert.That(result, Does.Contain("data-removed-style=\"max-width:100%\""));
            Assert.That(result, Does.Contain("class=\"removed-attr\""));
            Assert.That(result, Does.Not.Contain(" style=\"")); // no live style attribute left
        }

        [Test]
        public void HtmlSanitizerFactory_ShowRemoved_IsIdempotentAcrossTwoPasses()
        {
            // Mirrors ags.knowledge: style dropped, data-* allowed (so the markers survive a re-pass).
            var sanitizer = HtmlSanitizerFactory.Create(new HtmlSanitizerSettings
            {
                AllowedSchemes = new[] { "http", "https" },
                AllowAttributes = new[] { "id", "class" }, // don't re-allow style
                RemoveAttributes = new[] { "style" },
                AllowDataAttributes = true,
                ShowRemoved = true,
                ShowRemovedWhen = () => true
            });

            var once = sanitizer.Sanitize("<img src=\"a.webp\" style=\"max-width:100%\">");
            var twice = sanitizer.Sanitize(once);

            // Running it again must not add or duplicate any marker.
            once.ShouldEqual(twice);
        }

        [Test]
        public void HtmlSanitizerFactory_ShowRemovedOff_EmitsNoMarkers()
        {
            // Default is ShowRemoved = false -> plain removal, no markers.
            var sanitizer = HtmlSanitizerFactory.Create(HtmlSanitizerFactory.Default);

            var result = sanitizer.Sanitize("<div><script>x</script><b onclick=\"e()\">t</b></div>");

            Assert.That(result, Does.Not.Contain("removed-tag"));
            Assert.That(result, Does.Not.Contain("data-removed"));
        }

        // ---- ShowRemovedWhen: the per-request check on top of the ShowRemoved config bool ----
        // There is no HttpContext in these tests, so ShouldShow takes the uncached path and the
        // delegate is asked directly.

        static HtmlSanitizer BuildMarkerSanitizer(bool showRemoved, Func<bool?> showRemovedWhen) =>
            HtmlSanitizerFactory.Create(new HtmlSanitizerSettings
            {
                AllowedSchemes = new[] { "http", "https" },
                AllowAttributes = new[] { "id", "class" },
                KeepChildNodes = true,
                ShowRemoved = showRemoved,
                ShowRemovedWhen = showRemovedWhen
            });

        [Test]
        public void ShowRemovedWhen_ReturnsTrue_ShowsMarkers()
        {
            var result = BuildMarkerSanitizer(true, () => true)
                .Sanitize("<img src=\"a.jpg\" onerror=\"alert(1)\">");

            Assert.That(result, Does.Contain("data-removed-onerror=\"alert(1)\""));
            Assert.That(result, Does.Contain("class=\"removed-attr\""));
            Assert.That(result, Does.Contain("title=\"⚠ removed: onerror=alert(1)\""));
        }

        [Test]
        public void ShowRemovedWhen_ReturnsFalse_HidesMarkers()
        {
            var result = BuildMarkerSanitizer(true, () => false)
                .Sanitize("<img src=\"a.jpg\" onerror=\"alert(1)\">");

            Assert.That(result, Does.Not.Contain("data-removed"));
            Assert.That(result, Does.Not.Contain("removed-attr"));
            Assert.That(result, Does.Not.Contain(" onerror=\"")); // still removed, just not advertised
        }

        [Test]
        public void ShowRemovedWhen_ReturnsNull_HidesMarkers()
        {
            // The anonymous-visitor case: Context.Current.User().IsTester() is null when nobody is
            // signed in. A stranger must never be shown the payload we just stripped.
            var result = BuildMarkerSanitizer(true, () => (bool?)null)
                .Sanitize("<img src=\"a.jpg\" onerror=\"alert(1)\">");

            Assert.That(result, Does.Not.Contain("data-removed"));
            Assert.That(result, Does.Not.Contain("alert(1)"));
        }

        [Test]
        public void ShowRemovedWhen_NotSet_HidesMarkers()
        {
            // Safe by default: an app that never assigns the delegate gets no markers.
            var result = BuildMarkerSanitizer(true, null)
                .Sanitize("<img src=\"a.jpg\" onerror=\"alert(1)\">");

            Assert.That(result, Does.Not.Contain("data-removed"));
        }

        [Test]
        public void ShowRemoved_False_BeatsShowRemovedWhen()
        {
            // The config bool is the master switch: turning it off wins over any delegate.
            var result = BuildMarkerSanitizer(false, () => true)
                .Sanitize("<img src=\"a.jpg\" onerror=\"alert(1)\">");

            Assert.That(result, Does.Not.Contain("data-removed"));
        }

        [Test]
        public void ShowRemovedWhen_IsNotCalledWhenShowRemovedIsFalse()
        {
            var calls = 0;
            BuildMarkerSanitizer(false, () => { calls++; return true; })
                .Sanitize("<img src=\"a.jpg\" onerror=\"alert(1)\">");

            calls.ShouldEqual(0); // the early return really does skip the delegate
        }

        // ---- Marker placement: title only where CSS ::after cannot render, and nothing overwritten ----

        static HtmlSanitizer BuildStyleStripper() =>
            HtmlSanitizerFactory.Create(new HtmlSanitizerSettings
            {
                AllowedSchemes = new[] { "http", "https" },
                AllowAttributes = new[] { "id", "class" },
                RemoveAttributes = new[] { "style" }, // style is default-allowed, so drop it explicitly
                AllowDataAttributes = true,
                KeepChildNodes = true,
                ShowRemoved = true,
                ShowRemovedWhen = () => true
            });

        [Test]
        public void ShowRemoved_SetsTitleOnlyOnReplacedElements()
        {
            var sanitizer = BuildStyleStripper();

            var paragraph = sanitizer.Sanitize("<p style=\"color:red\">hi</p>");
            var image = sanitizer.Sanitize("<img src=\"a.webp\" style=\"color:red\">");

            // <p> shows the message through the ::after badge, so its title must be left alone.
            Assert.That(paragraph, Does.Contain("data-removed=\"style=color:red\""));
            Assert.That(paragraph, Does.Contain("removed-attr"));
            Assert.That(paragraph, Does.Not.Contain("title="));

            // <img> is a replaced element: ::after cannot render, so it gets the tooltip.
            Assert.That(image, Does.Contain("title=\"⚠ removed: style=color:red\""));
        }

        [Test]
        public void ShowRemoved_KeepsTheAuthorsTitle()
        {
            var result = BuildStyleStripper()
                .Sanitize("<img src=\"a.webp\" title=\"Our new campus\" data-toggle=\"tooltip\" style=\"color:red\">");

            Assert.That(result, Does.Contain("Our new campus"));      // the real tooltip survives
            Assert.That(result, Does.Contain("⚠ removed: style=color:red")); // ...on a second line
            Assert.That(result, Does.Contain("data-toggle=\"tooltip\""));
        }

        [Test]
        public void ShowRemoved_TitleDoesNotStackAcrossPasses()
        {
            // The image already carries an old marker line; sanitizing again must replace it, not add to it.
            var result = BuildStyleStripper()
                .Sanitize("<img src=\"a.webp\" title=\"⚠ removed: onclick=e()\" style=\"color:red\">");

            Assert.That(result.Split(new[] { "⚠ removed: " }, StringSplitOptions.None).Length - 1,
                Is.EqualTo(1), "the marker prefix must appear exactly once");
        }

        [Test]
        public void ShowRemoved_KeepsTheOlderRemovedValue()
        {
            // data-removed-style is already filled from an earlier pass. Both values must survive.
            var result = BuildStyleStripper()
                .Sanitize("<img src=\"a.webp\" data-removed-style=\"old\" style=\"new\">");

            Assert.That(result, Does.Contain("data-removed-style=\"old | new\""));
        }
    }
}
