using TimeTracker.Business.Common.Utils;

namespace TimeTracker.Tests.Unit.Business.Utils.SecurityUtilsTests
{
    public class PasswordGenerationTest
    {
        [Theory]
        [InlineData(12, 12)]
        [InlineData(256, 256)]
        public void ShouldGeneratePassword(int size, int expectedSize)
        {
            var password = SecurityUtil.GeneratePassword(size);
            Assert.Equal(expectedSize, password.Length);
        }
        
        [Theory]
        [InlineData(12)]
        [InlineData(256)]
        [InlineData(512)]
        public void ShouldReturnRandomPassword(int size)
        {
            var result1 = SecurityUtil.GeneratePassword(size);
            var result2 = SecurityUtil.GeneratePassword(size);
            Assert.True(result1 != result2);
        }
    }
}
