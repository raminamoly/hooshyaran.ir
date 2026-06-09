using Ganss.Xss;

namespace Hooshyaran.Web.Services;

public class CmsHtmlService : ICmsHtmlService
{
    private readonly HtmlSanitizer sanitizer;

    public CmsHtmlService()
    {
        sanitizer = new HtmlSanitizer();

        sanitizer.AllowedTags.Clear();
        foreach (var tag in new[]
        {
            "p", "br", "strong", "b", "em", "i", "u", "s",
            "ul", "ol", "li", "a",
            "h2", "h3", "h4", "blockquote",
            "img", "figure", "figcaption",
            "table", "thead", "tbody", "tr", "th", "td",
            "hr", "pre", "code"
        })
        {
            sanitizer.AllowedTags.Add(tag);
        }

        sanitizer.AllowedAttributes.Clear();
        foreach (var attribute in new[] { "href", "src", "alt", "title", "target", "rel", "class", "style", "colspan", "rowspan" })
        {
            sanitizer.AllowedAttributes.Add(attribute);
        }

        sanitizer.AllowedSchemes.Clear();
        foreach (var scheme in new[] { "http", "https", "mailto", "tel", "" })
        {
            sanitizer.AllowedSchemes.Add(scheme);
        }

        sanitizer.AllowedCssProperties.Clear();
        foreach (var property in new[] { "text-align", "direction", "width", "height" })
        {
            sanitizer.AllowedCssProperties.Add(property);
        }

        sanitizer.PostProcessNode += (_, args) =>
        {
            if (args.Node is not AngleSharp.Html.Dom.IHtmlAnchorElement anchor)
            {
                return;
            }

            if (string.Equals(anchor.Target, "_blank", StringComparison.OrdinalIgnoreCase))
            {
                anchor.Relation = "noopener noreferrer";
            }
        };
    }

    public string ToSafeHtml(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var html = LooksLikeHtml(value) ? value : PlainTextToHtml(value);

        return sanitizer.Sanitize(html);
    }

    public string PlainTextToHtml(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join("", value
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(block => $"<p>{System.Net.WebUtility.HtmlEncode(block).Replace("\n", "<br>")}</p>"));
    }

    private static bool LooksLikeHtml(string value) => value.Contains('<') && value.Contains('>');
}
