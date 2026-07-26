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
        .UseColorCode(
            HtmlFormatterType.Css,
            styleDictionary: StyleDictionary.DefaultDark
        )
        .Build();

    private static readonly MarkdownPipeline MBuilderPipelineWithLineBreaks = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseSoftlineBreakAsHardlineBreak()
        .UseColorCode(
            HtmlFormatterType.Css,
            styleDictionary: StyleDictionary.DefaultDark
        )
        .Build();
    
    public static string ToHtml(string markdown, bool isPreserveLineBreaks = false)
    {
        var pipeline = isPreserveLineBreaks
            ? MBuilderPipelineWithLineBreaks
            : MBuilderPipeline;

        return Markdig.Markdown.ToHtml($"{markdown}", pipeline);
    }
    
    public static string ToMarkdown(string html)
    {
        html = html.RemoveNewLineSymbols()
            .Replace("\\n", "\n")
            .Replace("\\r", "\n")
            .Trim();
        var markdown = CreateHtmlToMarkdownConverter().Convert(html);
        markdown ??= "";
        markdown = markdown.Trim('\r', '\n')
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");
        return markdown;
    }

    private static ReverseMarkdown.Converter CreateHtmlToMarkdownConverter()
    {
        return new ReverseMarkdown.Converter(new Config
        {
            // Include the unknown tag completely in the result.
            UnknownTags = Config.UnknownTagsOption.PassThrough,
            // Generate GitHub-flavoured markdown, including BR, PRE and table tags.
            GithubFlavored = true,
            // Ignore all comments.
            RemoveComments = true,
            // Remove markdown output for links where appropriate.
            SmartHrefHandling = true
        });
    }
}
