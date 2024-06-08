using TimeTracker.Business.Extensions;

namespace TimeTracker.Tests.Unit.Business.Extensions.TimeSpanTests
{
    public class ToReadableShortStringTest
    {
        [Theory]
        [InlineData("1.07:15:02.5800000", "31:15:02")]
        [InlineData("07:15:02.5800000", "07:15:02")]
        [InlineData("12.07:15:02.5800000", "295:15:02")]
        public void ShouldReturnCorrect(string actual, string expected)
        {
            var timeSpan = TimeSpan.Parse(actual);
            Assert.Equal(expected, timeSpan.ToReadableShortString());
        }
    }
}
