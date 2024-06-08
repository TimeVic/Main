using TimeTracker.Business.Common.Helpers;

namespace TimeTracker.Web.Services.UI;

public class MarkdownService
{
    public string ToHtml(string markdown)
    {
        return MarkdownHelper.ToHtml($"{markdown}");
    }
}
