using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Unit.Business.Extensions.DateTimeTests
{
    public class StartOfMonthTest
    {
        [Theory]
        [InlineData("2022-10-31T00:00:00.0000000", "2022-10-01T00:00:00.0000000")]
        [InlineData("2022-10-30T00:00:00.0000000", "2022-10-01T00:00:00.0000000")]
        [InlineData("2022-10-21T00:00:00.0000000", "2022-10-01T00:00:00.0000000")]
        public void ShouldReturnStartOfMonth(string dateString, string expectDateString)
        {
            var date = DateTime.Parse(dateString);
            Assert.Equal(expectDateString, date.StartOfMonth().ToString("o"));
        }
        
        [Theory]
        [InlineData("2022-10-31T00:00:00.0000000", "2022-10-31T00:00:00.0000000")]
        [InlineData("2022-10-30T00:00:00.0000000", "2022-10-31T00:00:00.0000000")]
        [InlineData("2022-10-21T00:00:00.0000000", "2022-10-31T00:00:00.0000000")]
        public void ShouldReturnEndOfMonth(string dateString, string expectDateString)
        {
            var date = DateTime.Parse(dateString);
            Assert.Equal(expectDateString, date.EndOfMonth().ToString("o"));
        }
    }
}
