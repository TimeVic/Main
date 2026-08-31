using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Business.Common.Utils;

public static class StringUtils
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    public static bool IsEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }
        var trimmed = value.Trim();
        if (trimmed.StartsWith('@') || trimmed.EndsWith('@') || !trimmed.Contains('@'))
        {
            return false;
        }
        return EmailValidator.IsValid(trimmed);
    }

    public static string? GetUserNameFromEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }
        var paths = email.Split("@");
        if (paths.Length <= 1)
        {
            return null;
        }
        var userName = paths.First().Trim().ToLower();
        return string.IsNullOrEmpty(userName) ? null : userName;
    }

    public static string NormalizeLogin(string? login)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            return string.Empty;
        }
        return login.Trim().TrimStart('@').ToLower();
    }
    
    public static string BytesToString(long byteCount)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
        if (byteCount == 0)
            return "0" + suf[0];
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(byteCount) * num).ToString() + suf[place];
    }
}
