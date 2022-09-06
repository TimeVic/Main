using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Unit.Business.Utils.Environment
{
    public class GetPodIdTest
    {
        [Theory]
        [InlineData("timevic-front-api-production-deployment-7f54bb45b7-pkk9j", "pkk9j")]
        [InlineData("timevic-web-production-deployment-775df6f7f6-66z6j", "66z6j")]
        public void ShouldClearUserName(string hostName, string hash)
        {
            System.Environment.SetEnvironmentVariable("HOSTNAME", hostName);
            Assert.Equal(EnvironmentHelper.GetPodId(), hash);
        }
    }
}
