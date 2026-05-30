using System.Text.RegularExpressions;
using TimeTracker.Business.Common.Helpers;

namespace TimeTracker.Client.Core.Services.UI;

public class MarkdownService
{
    private static readonly Regex AnchorOpeningTagRegex = new("<a(?=\\s|>)(?<attributes>[^>]*)>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string ToHtml(string markdown)
    {
        var html = MarkdownHelper.ToHtml($"{markdown}");
        html = AnchorOpeningTagRegex.Replace(html, AddReadonlyLinkAttributes);
        return $"""<div class="markdown-body">{html}</div>""";
    }

    private static string AddReadonlyLinkAttributes(Match match)
    {
        var attributes = match.Groups["attributes"].Value;
        attributes = EnsureAttribute(attributes, "target", "_blank");
        attributes = EnsureAttribute(attributes, "rel", "noopener noreferrer");
        attributes = EnsureAttribute(attributes, "onclick", "event.stopPropagation()");
        return $"<a{attributes}>";
    }

    private static string EnsureAttribute(string attributes, string attributeName, string attributeValue)
    {
        if (Regex.IsMatch(attributes, $@"(^|\s){Regex.Escape(attributeName)}\s*=", RegexOptions.IgnoreCase))
        {
            return attributes;
        }

        return $"{attributes} {attributeName}=\"{attributeValue}\"";
    }
}
