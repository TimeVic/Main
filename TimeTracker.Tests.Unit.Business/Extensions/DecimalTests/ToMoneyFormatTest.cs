using TimeTracker.Business.Extensions;

namespace TimeTracker.Tests.Unit.Business.Extensions.DecimalTests;

public class ToMoneyFormatTest
{
    [Theory]
    [InlineData(12, null, "12.00")]
    [InlineData(100000, null, "100,000.00")]
    [InlineData(1234567.89, null, "1,234,567.89")]
    [InlineData(100000, "$", "100,000.00 $")]
    public void ShouldFormatMoneyWithThousandsSeparator(decimal value, string? symbol, string expected)
    {
        var result = value.ToMoneyFormat(symbol);

        Assert.Equal(expected, result);
    }
}
