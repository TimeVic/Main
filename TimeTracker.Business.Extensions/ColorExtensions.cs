using System.Drawing;

namespace TimeTracker.Business.Extensions;

public static class ColorExtensions
{
    public static string? ToHexString(this Color c)
    {
        if (c == Color.Empty)
        {
            return null;
        }

        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    public static string? ToHexString(this Color? c)
    {
        if (c == null)
        {
            return null;
        }
        return c?.ToHexString();
    }

    public static string ToRgbString(this Color c) => $"RGB({c.R}, {c.G}, {c.B})";
}
