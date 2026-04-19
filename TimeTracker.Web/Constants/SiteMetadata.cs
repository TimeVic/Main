namespace TimeTracker.Web.Constants;

public static class SiteMetadata
{
    public const string BaseUrl = "https://timevic.com";

    public const string IndexRobots = "index, follow";
    public const string NoIndexRobots = "noindex, nofollow, noarchive";

    public const string HomeTitle = "TimeVic — Time and Income Tracker for Freelance Developers";
    public const string HomeDescription =
        "TimeVic helps freelance developers track hours, calculate earnings, and see paid and unpaid balances across all clients and projects.";
    public const string HomeKeywords =
        "freelance developer time tracker, income tracker, client billing tracker, paid unpaid work tracker, project time tracking, timesheet app, Jira time tracking";

    public const string RegistrationTitle = "Create a TimeVic Account — Freelance Time and Income Tracker";
    public const string RegistrationDescription =
        "Create a TimeVic account to track hours, calculate earnings, review client balances, and record payments for freelance client work.";
    public const string RegistrationKeywords =
        "create freelance time tracker account, income tracking app, client balance tracker, paid unpaid work tracker, project time tracking";

    public static string ToAbsoluteUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath == "/")
        {
            return BaseUrl;
        }

        return $"{BaseUrl}{relativePath}";
    }
}
