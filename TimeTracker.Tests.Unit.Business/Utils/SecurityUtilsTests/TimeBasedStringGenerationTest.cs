using System.Collections.Concurrent;
using TimeTracker.Business.Common.Utils;

namespace TimeTracker.Tests.Unit.Business.Utils.SecurityUtilsTests
{
    public class TimeBasedStringGenerationTest
    {
        [Fact]
        public void ShouldGenerateToken()
        {
            var token = SecurityUtil.GetTimeBasedToken();
            Assert.True(!string.IsNullOrEmpty(token));
            Assert.True(token.Length >= 10);
        }

        [Fact]
        public void GeneratedTokensShouldBeUniq()
        {
            var concurrentBag = new ConcurrentBag<string>();
            var bagAddTasks = new List<Task>();
            for (int i = 0; i < 1000000; i++)
            {
                bagAddTasks.Add(Task.Run(
                    () => concurrentBag.Add(SecurityUtil.GetTimeBasedToken())
                ));
            }

            // Wait for all tasks to complete
            Task.WaitAll(bagAddTasks.ToArray());

            Assert.Equal(concurrentBag.Count(), concurrentBag.Distinct().Count());
        }

        [Fact]
        public void BigArrayGenerationShouldNotBeLongerThan3Seconds()
        {
            var start = DateTime.UtcNow;
            for (int i = 0; i < 1000000; i++)
            {
                SecurityUtil.GetTimeBasedToken();
            }
            var end = DateTime.UtcNow;
            Assert.True(end - start < TimeSpan.FromSeconds(3));
        }
    }
}
