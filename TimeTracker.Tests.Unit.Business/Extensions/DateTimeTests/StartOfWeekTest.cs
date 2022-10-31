using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Unit.Business.Extensions.DateTimeTests
{
    public class StartOfWeekTest
    {
        [Theory]
        [InlineData("2022-10-31T00:00:00.0000000Z", "2022-10-31T00:00:00.0000000Z")]
        [InlineData("2022-10-30T00:00:00.0000000Z", "2022-10-24T00:00:00.0000000Z")]
        [InlineData("2022-10-21T00:00:00.0000000Z", "2022-10-17T00:00:00.0000000Z")]
        public void ShouldReturnStartOfWeek(string dateString, string expectDateString)
        {
            var date = DateTime.Parse(dateString).ToUniversalTime();
            Assert.Equal(expectDateString, date.StartOfWeek().ToString("o"));
        }
    }
}
