using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Tests.Unit.Business.Utils.SecurityUtilsTests
{
    public class SaltGenerationTest
    {
        [Theory]
        [InlineData(12, 12)]
        [InlineData(256, 256)]
        public void ShouldGenerateSalt(int size, int expectedSize)
        {
            var bytes = SecurityUtil.GenerateSalt(size);
            Assert.Equal(expectedSize, bytes.Length);
        }
        
        [Theory]
        [InlineData(12)]
        [InlineData(256)]
        [InlineData(512)]
        public void ShouldReturnRandomBytes(int size)
        {
            var result1 = SecurityUtil.GenerateSalt(size);
            var result2 = SecurityUtil.GenerateSalt(size);
            Assert.False(result1.CompareTo(result2));
        }
    }
}
