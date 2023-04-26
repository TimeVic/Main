using ColorCode.Styling;
using Markdig;
using Markdown.ColorCode;

namespace TimeTracker.Web.Services.UI;

public class MarkdownService
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

    public string ToHtml(string markdown)
    {
        return Markdig.Markdown.ToHtml($"{markdown}", Pipeline);
    }
}
