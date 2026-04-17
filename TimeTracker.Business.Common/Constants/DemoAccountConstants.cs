using System.Text.RegularExpressions;

namespace TimeTracker.Business.Common.Constants;

public static class DemoAccountConstants
{
    private const string EmailPrefix = "demo+";
    private const string EmailDomain = "timevic.com";

    private static readonly Regex DemoEmailRegex = new(
        @"^demo\+.+@timevic\.com$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    public static string GenerateEmail()
        => $"{EmailPrefix}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}@{EmailDomain}";

    public static bool IsDemoEmail(string email)
        => DemoEmailRegex.IsMatch(email);
}

