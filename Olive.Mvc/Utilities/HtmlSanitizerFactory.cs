using AngleSharp.Dom;
using Ganss.Xss;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Olive.Mvc
{
    /// <summary>
    /// The policy values for <see cref="HtmlSanitizerFactory"/>, bound from the
    /// "Html:Sanitizer" configuration section. All lists are optional.
    /// <para>Semantics: <see cref="AllowedSchemes"/> REPLACES the whole scheme list.
    /// <see cref="AllowTags"/> / <see cref="AllowAttributes"/> / <see cref="UriAttributes"/>
    /// are ADDED to the library defaults. <see cref="RemoveAttributes"/> is REMOVED from them.</para>
    /// </summary>
    public sealed class HtmlSanitizerSettings
    {
        /// <summary>Replaces the whole allowed-scheme list (e.g. http, https, mailto, tel).</summary>
        public string[] AllowedSchemes { get; set; }

        /// <summary>Tags added to the library defaults (e.g. video, source, iframe).</summary>
        public string[] AllowTags { get; set; }

        /// <summary>Attributes added to the library defaults (e.g. id, class, role).</summary>
        public string[] AllowAttributes { get; set; }

        /// <summary>Attributes removed from the library defaults (e.g. style).</summary>
        public string[] RemoveAttributes { get; set; }

        /// <summary>Attributes whose value is treated as a URL and scheme-checked (e.g. poster).</summary>
        public string[] UriAttributes { get; set; }

        /// <summary>The only hosts an iframe may load (~ the CSP "frame-src" directive). Subdomains included.</summary>
        public string[] AllowedFrameDomains { get; set; }

        /// <summary>Allow inert data-* attributes.</summary>
        public bool AllowDataAttributes { get; set; }

        /// <summary>Keep inert aria-* attributes (open-ended a11y family).</summary>
        public bool AllowAriaAttributes { get; set; }

        /// <summary>Unwrap a disallowed tag but keep its text/children.</summary>
        public bool KeepChildNodes { get; set; }

        /// <summary>Write an Error log entry (with request context + stack trace) on each strip.</summary>
        public bool LogRemoved { get; set; }

        /// <summary>Master switch: leave a visible marker in the output where a tag/attribute
        /// was stripped. Bound from the "Html:Sanitizer" config section.</summary>
        public bool ShowRemoved { get; set; }

        /// <summary>An extra, per-request check applied on top of <see cref="ShowRemoved"/>.
        /// It runs each time something is stripped, so keep it cheap — a claim read, not a
        /// database call (e.g. () =&gt; Context.Current.User().IsTester()).
        /// It cannot come from JSON — assign it
        /// in Startup. A marker is shown only when ShowRemoved is true AND this returns true;
        /// null (not set, or no signed-in user) hides the markers.</summary>
        public Func<bool?> ShowRemovedWhen { get; set; }
    }

    /// <summary>
    /// Builds and caches the shared <see cref="HtmlSanitizer"/> used by string.Raw().
    /// The policy comes from the "Html:Sanitizer" configuration section when present
    /// (read via <see cref="Context"/>.Current); otherwise the built-in <see cref="Default"/> is used.
    /// </summary>
    public static class HtmlSanitizerFactory
    {
        const string ConfigKey = "Html:Sanitizer";

        // CSS ::before/::after never render on "replaced" elements, so the badge cannot show there.
        // Only these fall back to a native `title` tooltip.
        static readonly string[] ReplacedElements =
        {
            "IMG", "INPUT", "VIDEO", "IFRAME", "AUDIO", "EMBED", "OBJECT", "CANVAS", "SELECT", "TEXTAREA"
        };

        const string MarkerPrefix = "⚠ removed: ";

        // Created at type-load with NO config access, so Startup (ConfigureServices) can safely
        // assign delegates onto it before Context is ready.
        static readonly HtmlSanitizerSettings SettingsInstance = new();

        /// <summary>The live policy object. Delegates such as
        /// <see cref="HtmlSanitizerSettings.ShowRemovedWhen"/> are assigned onto it in Startup;
        /// the config-bound values are filled in on the first Sanitize() call.</summary>
        public static HtmlSanitizerSettings Settings => SettingsInstance;

        // Lazy (not a static field initializer) so the config/Context read happens on the first
        // Raw() call at request time, not at type-load. Avoids "Context is not initialized".
        static readonly Lazy<HtmlSanitizer> Shared = new(BuildFromConfig);

        /// <summary>
        /// The built-in baseline policy, used when no "Html:Sanitizer" config section exists.
        /// Mirrors Olive's original sanitizer (commit 1ad7ed4): the library defaults plus the
        /// mailto/tel/http/https schemes, keep inline "style" and id/class, and unwrap disallowed
        /// tags (KeepChildNodes). Apps tighten this — e.g. drop style, allow video/iframe, show or
        /// log removals — through the "Html:Sanitizer" config section.
        /// </summary>
        public static HtmlSanitizerSettings Default => new()
        {
            AllowedSchemes = new[] { "http", "https", "mailto", "tel" },
            AllowAttributes = new[] { "style", "id", "class" },
            KeepChildNodes = true
        };

        /// <summary>Sanitizes the given HTML with the shared, cached sanitizer.</summary>
        public static string Sanitize(string html) => Shared.Value.Sanitize(html);

        // Runs on the first Sanitize() call, when Context is ready. The config values are copied
        // INTO the existing SettingsInstance, so delegates already assigned in Startup survive.
        static HtmlSanitizer BuildFromConfig()
        {
            Apply(ReadSettings() ?? Default, onto: SettingsInstance);
            return Create(SettingsInstance);
        }

        /// <summary>Copies the config-bound values across, leaving delegates alone.</summary>
        static void Apply(HtmlSanitizerSettings source, HtmlSanitizerSettings onto)
        {
            onto.AllowedSchemes = source.AllowedSchemes;
            onto.AllowTags = source.AllowTags;
            onto.AllowAttributes = source.AllowAttributes;
            onto.RemoveAttributes = source.RemoveAttributes;
            onto.UriAttributes = source.UriAttributes;
            onto.AllowedFrameDomains = source.AllowedFrameDomains;
            onto.AllowDataAttributes = source.AllowDataAttributes;
            onto.AllowAriaAttributes = source.AllowAriaAttributes;
            onto.KeepChildNodes = source.KeepChildNodes;
            onto.ShowRemoved = source.ShowRemoved;
            onto.LogRemoved = source.LogRemoved;
            // ShowRemovedWhen is deliberately NOT copied: it never comes from config,
            // and Startup may already have assigned it.
        }

        static HtmlSanitizerSettings ReadSettings()
        {
            try
            {
                // Olive.Config resolves Context.Current.Config under the hood.
                var section = Config.GetSection(ConfigKey);
                return section.Exists() ? Config.Bind<HtmlSanitizerSettings>(ConfigKey) : null;
            }
            catch
            {
                // Context not initialized (unit tests / pre-startup) -> fall back to Default.
                return null;
            }
        }

        /// <summary>
        /// Builds a sanitizer from the given settings. Exposed so callers (and tests) can build a
        /// sanitizer for a specific policy without touching the cached shared instance.
        /// </summary>
        public static HtmlSanitizer Create(HtmlSanitizerSettings settings)
        {
            settings ??= Default;
            var sanitizer = new HtmlSanitizer();

            // Schemes: replace the whole list when provided (~ strict img-src / href).
            if (settings.AllowedSchemes?.Any() == true)
            {
                sanitizer.AllowedSchemes.Clear();
                foreach (var scheme in settings.AllowedSchemes) sanitizer.AllowedSchemes.Add(scheme);
            }

            // "style" is in the library's default allow-list, so it must be REMOVED, not just "not added".
            foreach (var attr in settings.RemoveAttributes.OrEmpty())
                sanitizer.AllowedAttributes.Remove(attr);

            foreach (var tag in settings.AllowTags.OrEmpty())
                sanitizer.AllowedTags.Add(tag);

            foreach (var attr in settings.AllowAttributes.OrEmpty())
                sanitizer.AllowedAttributes.Add(attr);

            foreach (var attr in settings.UriAttributes.OrEmpty())
                sanitizer.UriAttributes.Add(attr);

            sanitizer.AllowDataAttributes = settings.AllowDataAttributes;
            sanitizer.KeepChildNodes = settings.KeepChildNodes;

            // Both handlers are ALWAYS attached: whether to show a marker is decided per request
            // (see ShouldShow), so it cannot be settled here, when the sanitizer is built and cached.
            // They only run when something is actually being removed, so clean HTML pays nothing.
            sanitizer.RemovingAttribute += (_, e) => OnRemovingAttribute(e, settings);
            sanitizer.RemovingTag += (_, e) => OnRemovingTag(e, settings);

            // frame-src allow-list: an iframe may only point at an https URL on a trusted domain.
            var frameDomains = settings.AllowedFrameDomains ?? Array.Empty<string>();
            sanitizer.FilterUrl += (_, e) =>
            {
                // Only frames are host-restricted. Links, images and media keep the scheme rules above.
                if (e.Tag?.NodeName.Equals("IFRAME", StringComparison.OrdinalIgnoreCase) != true) return;

                var allowed = Uri.TryCreate(e.OriginalUrl, UriKind.Absolute, out var uri) &&
                    uri.Scheme == Uri.UriSchemeHttps &&
                    IsAllowedFrameHost(uri.Host, frameDomains);

                // Untrusted host, plain http, javascript:, or protocol-relative //evil.com all land here.
                if (!allowed) e.SanitizedUrl = null;
            };

            return sanitizer;
        }

        static void OnRemovingAttribute(RemovingAttributeEventArgs e, HtmlSanitizerSettings settings)
        {
            var name = e.Attribute.Name;

            // aria-* : keep it. Cancel = "do not remove". No marker.
            if (settings.AllowAriaAttributes && name.StartsWith("aria-", StringComparison.OrdinalIgnoreCase))
            {
                e.Cancel = true;
                return;
            }

            if (settings.LogRemoved)
                LogRemoval("attribute", $"{name}=\"{e.Attribute.Value}\"", e.Reason, e.Tag);

            // Mark, but DO NOT cancel -> the library still removes the unsafe attribute (e.g. onerror).
            // Cancelling here would KEEP onerror -> an XSS hole.
            // Never mark our own markers (data-removed*): if they are ever re-sanitized they must
            // just disappear, not cascade into data-removed-data-removed-*.
            if (ShouldShow(settings) && IsSafeAttrName(name) &&
                !name.StartsWith("data-removed", StringComparison.OrdinalIgnoreCase))
            {
                // Inert copy of the value, keeping any older one: a second pass must not lose the first.
                AppendValue(e.Tag, $"data-removed-{name}", e.Attribute.Value, " | ");

                // Aggregate a human-readable summary into ONE fixed attribute so a CSS ::after can
                // show it dynamically (::after can only read a known attribute name, not data-removed-*).
                var summary = AppendValue(e.Tag, "data-removed", $"{name}={e.Attribute.Value}", "; ");

                // Every other element shows the message through the ::after badge, so leave its title alone.
                if (e.Tag.NodeName.IsAnyOf(ReplacedElements)) SetMarkerTitle(e.Tag, summary);

                e.Tag.ClassList.Add("removed-attr");
            }
        }

        static void OnRemovingTag(RemovingTagEventArgs e, HtmlSanitizerSettings settings)
        {
            if (settings.LogRemoved)
                LogRemoval("tag", e.Tag.TagName, e.Reason, e.Tag);

            if (ShouldShow(settings))
            {
                var span = e.Tag.Owner.CreateElement("span");
                span.ClassName = "removed-tag"; // styled red by the app's CSS (no inline style: it would be stripped)
                span.TextContent = $"[REMOVED: {e.Tag.TagName}]";
                e.Tag.Replace(span);            // replaces the whole node -> inner content dropped
                e.Cancel = true;                // safe here: the node is already swapped out
            }
            // else: LogRemoved-only -> let the normal removal proceed (respects KeepChildNodes).
        }

        /// <summary>Adds a value to an attribute, keeping whatever is already there, and returns the
        /// new value. An entry that is already present is not added again, so re-sanitizing the same
        /// HTML changes nothing.</summary>
        static string AppendValue(IElement tag, string attribute, string value, string separator)
        {
            var parts = tag.GetAttribute(attribute).OrEmpty()
                .Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.HasValue())
                .ToList();

            if (!parts.Contains(value)) parts.Add(value);

            var result = string.Join(separator, parts);
            tag.SetAttribute(attribute, result);
            return result;
        }

        /// <summary>Puts our warning on the tooltip WITHOUT losing the author's own text
        /// (Bootstrap's data-toggle="tooltip" reads `title` too). Our previous line is replaced,
        /// not stacked, so running Raw() twice gives the same result.</summary>
        static void SetMarkerTitle(IElement tag, string summary)
        {
            var lines = tag.GetAttribute("title").OrEmpty()
                .Split('\n')
                .Select(x => x.Trim())
                .Where(x => x.HasValue() && !x.StartsWith(MarkerPrefix, StringComparison.Ordinal))
                .ToList();

            lines.Add(MarkerPrefix + summary);
            tag.SetAttribute("title", string.Join("\n", lines));
        }

        /// <summary>Show a marker only when the config bool AND the per-request check both allow it.
        /// The check is expected to be cheap (a claim read, not a database call), so it is not cached.
        /// It takes the settings as an argument, not the static Settings, because a sanitizer built
        /// by Create(custom) must use the settings it was built with.</summary>
        static bool ShouldShow(HtmlSanitizerSettings settings) =>
            settings.ShowRemoved && settings.ShowRemovedWhen?.Invoke() == true;

        /// <summary>Exact domain or a true subdomain of it. A naive EndsWith("youtube.com")
        /// would also match "evil-youtube.com", so the leading dot matters.</summary>
        static bool IsAllowedFrameHost(string host, string[] domains) =>
            domains.Any(domain =>
                host.Equals(domain, StringComparison.OrdinalIgnoreCase) ||
                host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase));

        /// <summary>An attribute name safe to embed into a data-removed-{name} attribute.</summary>
        static bool IsSafeAttrName(string name) =>
            name.HasValue() && Regex.IsMatch(name, "^[A-Za-z][A-Za-z0-9-]*$");

        /// <summary>
        /// Writes one detailed Error entry describing a strip. Request context (user, url, IP,
        /// trace id) is appended automatically by Olive's file logger via Log.ContextProvider,
        /// so here we add the parts it does not know: the removed item, reason, element and trace.
        /// Wrapped so logging can never break sanitizing (e.g. no logger/HttpContext in unit tests).
        /// </summary>
        static void LogRemoval(string kind, string detail, RemoveReason reason, IElement tag)
        {
            try
            {
                var message = new StringBuilder()
                    .AppendLine($"[HtmlSanitizer] Removed {kind}: {detail}")
                    .AppendLine($"Reason: {reason}")
                    .AppendLine($"Element: {tag?.OuterHtml.OrEmpty().Left(500)}")
                    .Append("StackTrace:").AppendLine().Append(Environment.StackTrace)
                    .ToString();

                Log.For(typeof(HtmlSanitizerFactory)).Error(message);
            }
            catch
            {
                // Never let logging failure affect the sanitized output.
            }
        }
    }
}
