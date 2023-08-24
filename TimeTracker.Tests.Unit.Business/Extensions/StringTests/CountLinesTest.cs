using TimeTracker.Business.Extensions;

namespace TimeTracker.Tests.Unit.Business.Extensions.StringTests
{
    public class CountLinesTest
    {
        [Fact]
        public void ShouldCalculateNewLine1()
        {
            Assert.Equal(3, $"asd{Environment.NewLine}aaaaa {Environment.NewLine} ".CountLines());
        }
        
        [Fact]
        public void ShouldCalculateNewLine2()
        {
            Assert.Equal(2, $"asd{Environment.NewLine}aaaaa".CountLines());
        }
        
        [Fact]
        public void ShouldCalculateNewLine3()
        {
            Assert.Equal(1, $"2023-10-30T00:00:00.0000000".CountLines());
        }
        
        [Fact]
        public void ShouldCalculateNewLine4()
        {
            Assert.Equal(0, $"".CountLines());
        }
    }
}
