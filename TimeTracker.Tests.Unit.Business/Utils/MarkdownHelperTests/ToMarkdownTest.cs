using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Extensions;
using TimeTracker.Tests.Unit.Business.Core;

namespace TimeTracker.Tests.Unit.Business.Utils.MarkdownHelperTests
{
    public class ToMarkdownTest: BaseUnitTest
    {
        [Theory]
        [InlineData("This a sample <strong>paragraph</strong> from <a href=\"http://test.com\">my site</a>", "This a sample **paragraph** from [my site](http://test.com)")]
        [InlineData(
            "<p>Testing description</p>\n\n\n\n<p>Another paragraph</p>", 
            "Testing description\n\nAnother paragraph"
        )]
        public void ShouldConvert(string html, string expectedMarkdown)
        {
            var actualMarkdown = MarkdownHelper.ToMarkdown(html);
            Assert.Equal(expectedMarkdown, actualMarkdown);
        }
        
        // TODO: Restore in the feature
        // [Fact]
        public async Task ShouldConvertBigHtml_1()
        {
            var html = await GetStubString("actual_1.htm", "markdown");
            var actualMarkdown = MarkdownHelper.ToMarkdown(html);
            
            var expectedMarkdown = await GetStubString("expected_1.md", "markdown");
            Assert.Contains(expectedMarkdown.Trim().Trim('\n'), actualMarkdown.Trim().Trim('\n'));
        }
    }
}
