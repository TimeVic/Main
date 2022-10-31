using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Unit.Business.Extensions.DateTimeTests
{
    public class OfYearTest
    {
        [Theory]
        [InlineData("2022-10-31T00:00:00.0000000", "2022-01-01T00:00:00.0000000")]
        [InlineData("2023-10-30T00:00:00.0000000", "2023-01-01T00:00:00.0000000")]
        public void ShouldReturnStartOfYear(string dateString, string expectDateString)
        {
            var date = DateTime.Parse(dateString);
            Assert.Equal(expectDateString, date.StartOfYear().ToString("o"));
        }
        
        [Theory]
        [InlineData("2022-10-31T00:00:00.0000000", "2022-12-31T00:00:00.0000000")]
        [InlineData("2023-10-30T00:00:00.0000000", "2023-12-31T00:00:00.0000000")]
        public void ShouldReturnEndOfYear(string dateString, string expectDateString)
        {
            var date = DateTime.Parse(dateString);
            Assert.Equal(expectDateString, date.EndOfYear().ToString("o"));
        }
    }
}
