using TimeTracker.Business.Common.Utils;

namespace TimeTracker.Tests.Unit.Business.Utils.StringUtilsTests
{
    public class GetUserNameFromEmailTest
    {
        [Theory]
        [InlineData("Test.User@test.com", "test.user")]
        [InlineData("123@test.com", "123")]
        [InlineData("test.com", null)]
        [InlineData(null, null)]
        public void ShouldReceiveUserName(string email, string expectUserName)
        {
            var userName = StringUtils.GetUserNameFromEmail(email);
            Assert.Equal(expectUserName, userName);
        }
    }
}
