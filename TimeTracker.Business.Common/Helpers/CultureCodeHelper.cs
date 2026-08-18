namespace TimeTracker.Business.Common.Helpers;

public static class CultureCodeHelper
{
    public const string EnglishCultureCode = "en";
    public const string UkrainianCultureCode = "uk-UA";

    public static string? GetSupportedCultureCode(string? cultureValue)
    {
        if (string.IsNullOrWhiteSpace(cultureValue))
        {
            return null;
        }

        var cultureCode = cultureValue
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => item.Split(';', StringSplitOptions.TrimEntries)[0])
            .FirstOrDefault(item => item.StartsWith("uk", StringComparison.OrdinalIgnoreCase)
                || item.StartsWith("en", StringComparison.OrdinalIgnoreCase));

        if (cultureCode == null)
        {
            return null;
        }

        return cultureCode.StartsWith("uk", StringComparison.OrdinalIgnoreCase)
            ? UkrainianCultureCode
            : EnglishCultureCode;
    }
}
