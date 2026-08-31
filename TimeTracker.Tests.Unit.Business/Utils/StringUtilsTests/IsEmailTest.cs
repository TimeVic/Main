using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Tests.Unit.Business.Utils.StringUtilsTests
{
    public class IsEmailTest
    {
        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("user.name+tag@sub.domain.org", true)]
        [InlineData("123@domain.io", true)]
        [InlineData("user@domain", true)]
        [InlineData("@username", false)]
        [InlineData("@example.com", false)]
        [InlineData("username", false)]
        [InlineData("user@", false)]
        [InlineData("user@domain@other", false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData(null, false)]
        public void ShouldValidateEmail(string? input, bool expected)
        {
            var actualFromUtils = StringUtils.IsEmail(input);
            Assert.Equal(expected, actualFromUtils);

            var actualFromExtension = input.IsEmail();
            Assert.Equal(expected, actualFromExtension);
        }
    }
}
