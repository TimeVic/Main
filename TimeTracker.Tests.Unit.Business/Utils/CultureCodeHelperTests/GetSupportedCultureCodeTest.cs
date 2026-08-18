using TimeTracker.Business.Common.Helpers;

namespace TimeTracker.Tests.Unit.Business.Utils.CultureCodeHelperTests;

public class GetSupportedCultureCodeTest
{
    [Theory]
    [InlineData("uk-UA,uk;q=0.9,en;q=0.8", CultureCodeHelper.UkrainianCultureCode)]
    [InlineData("de-DE,de;q=0.9,uk;q=0.8", CultureCodeHelper.UkrainianCultureCode)]
    [InlineData("en-US,en;q=0.9", CultureCodeHelper.EnglishCultureCode)]
    [InlineData("de-DE", null)]
    [InlineData(null, null)]
    public void ReturnsFirstSupportedCulture(string? cultureValue, string? expectedCultureCode)
    {
        var cultureCode = CultureCodeHelper.GetSupportedCultureCode(cultureValue);

        Assert.Equal(expectedCultureCode, cultureCode);
    }
}
