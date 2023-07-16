using System.Text.RegularExpressions;
using ColorCode.Styling;
using Markdig;
using Markdown.ColorCode;
using ReverseMarkdown;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Business.Common.Helpers;

public class MarkdownHelper
{
    private static readonly MarkdownPipeline MBuilderPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseColorCode(StyleDictionary.DefaultDark)
        .Build();
    
    private static readonly ReverseMarkdown.Converter HtmlToMarkdownBuilder = new(new () {
        // Include the unknown tag completely in the result (default as well)
        UnknownTags = Config.UnknownTagsOption.PassThrough,
        // generate GitHub flavoured markdown, supported for BR, PRE and table tags
        GithubFlavored = true,
        // will ignore all comments
        RemoveComments = true,
        // remove markdown output for links where appropriate
        SmartHrefHandling = true
    });
    
    public static string ToHtml(string markdown)
    {
        return Markdig.Markdown.ToHtml($"{markdown}", MBuilderPipeline);
    }
    
    public static string ToMarkdown(string html)
    {
        html = html.RemoveNewLineSymbols()
            .Replace("\\n", "\n")
            .Replace("\\r", "\n")
            .Trim();
        var markdown = HtmlToMarkdownBuilder.Convert(html);
        markdown ??= "";
        markdown = markdown.Trim('\r', '\n')
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");
        return markdown;
    }
}
