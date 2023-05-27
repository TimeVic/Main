using ColorCode.Styling;
using Markdig;
using Markdown.ColorCode;

namespace TimeTracker.Business.Common.Helpers;

public class MarkdownHelper
{
    private static MarkdownPipeline _pipeline;
    
    private static MarkdownPipeline Pipeline
    {
        get
        {
            if (_pipeline == null)
            {
                _pipeline = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .UseColorCode(StyleDictionary.DefaultDark)
                    .Build();
            }

            return _pipeline;
        }
    }

    public static string ToHtml(string markdown)
    {
        return Markdig.Markdown.ToHtml($"{markdown}", Pipeline);
    }
}
