namespace TimeTracker.Business.Common.Utils;

public static class StringUtils
{
    public static string? GetUserNameFromEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return null;
        }
        var paths = email.Split("@");
        if (paths.Length <= 1)
        {
            return null;
        }
        return paths.First().ToLower();
    }
}
