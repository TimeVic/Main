namespace TimeTracker.Web.Constants;

public static class SiteMetadata
{
    public const string BaseUrl = "https://timevic.com";

    public const string IndexRobots = "index, follow";
    public const string NoIndexRobots = "noindex, nofollow, noarchive";

    public const string HomeTitle = "TimeVic — Time Tracking for Small Teams";
    public const string HomeDescription =
        "TimeVic is a time tracking and task management workspace for small teams, freelancers, and agencies. Track hours, manage tasks, chat, organize clients, and review payments in one place.";
    public const string HomeKeywords =
        "time tracking software, team time tracker, project time tracking, timesheet app, task management, freelancer time tracking, agency time tracking, client billing software";

    public const string RegistrationTitle = "Create a TimeVic Account — Time Tracking Workspace";
    public const string RegistrationDescription =
        "Create a TimeVic account to start tracking work hours, organizing tasks, collaborating in team chat, and reviewing project payments in one workspace.";
    public const string RegistrationKeywords =
        "create time tracker account, team timesheet software, sign up time tracking app, work hours tracker, project time management";

    public static string ToAbsoluteUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath == "/")
        {
            return BaseUrl;
        }

        return $"{BaseUrl}{relativePath}";
    }
}
