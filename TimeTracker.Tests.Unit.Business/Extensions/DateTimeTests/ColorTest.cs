using System.Drawing;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Unit.Business.Extensions.DateTimeTests
{
    public class ColorTest
    {
        [Theory]
        [InlineData("#eb4034", "#000000")]
        [InlineData("#241e1d", "#FFFFFF")]
        [InlineData("#736fe8", "#000000")]
        public void ShouldGetTextColor(string bgColorHex, string textColorHex)
        {
            var bgColor = ColorTranslator.FromHtml(bgColorHex);
            var textColor = bgColor.GetTextColorBasedOn();
            Assert.Equal(textColor.ToHexString(), textColor.ToHexString());
        }
    }
}
