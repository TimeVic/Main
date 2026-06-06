using TimeTracker.Business.Common.Helpers;
using TimeTracker.Tests.Unit.Business.Core;

namespace TimeTracker.Tests.Unit.Business.Utils.MarkdownHelperTests;

public class ToHtmlTest : BaseUnitTest
{
    [Fact]
    public void ShouldPreserveSingleLineBreakWhenRequested()
    {
        var html = MarkdownHelper.ToHtml("First line\nSecond line", isPreserveLineBreaks: true);

        Assert.Contains("First line<br />\nSecond line", html);
    }

    [Fact]
    public void ShouldUseStandardMarkdownLineBreaksByDefault()
    {
        var html = MarkdownHelper.ToHtml("First line\nSecond line");

        Assert.DoesNotContain("<br", html);
    }
}
