using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Unit.Business.Extensions.DateTimeTests
{
    public class GetIso8601WeekOfYearTest
    {
        [Theory]
        [InlineData("2007-01-05T01:00:00Z", 1)]
        [InlineData("2007-03-01T01:00:00Z", 9)]
        [InlineData("2007-12-25T01:00:00Z", 52)]
        [InlineData("2007-12-30T01:00:00Z", 52)]
        public void ShouldGetCorrectWeekNumber(string dateString, int expectWeekNumber)
        {
            var date = DateTime.Parse(dateString);
            Assert.Equal(expectWeekNumber, date.GetIso8601WeekOfYear());
        }
    }
}
