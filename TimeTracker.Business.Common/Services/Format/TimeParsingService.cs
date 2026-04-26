using System.Globalization;
using System.Text.RegularExpressions;

namespace TimeTracker.Business.Common.Services.Format;

public class TimeParsingService: ITimeParsingService
{
    private const int WorkingHoursPerDay = 8;
    private const int WorkingDaysPerWeek = 5;
    private const int MinutesPerHour = 60;
    private const int MinutesPerDay = WorkingHoursPerDay * MinutesPerHour;
    private const int MinutesPerWeek = WorkingDaysPerWeek * MinutesPerDay;

    private readonly Regex _cleanUpRegex = new Regex("[^0-9]");
    private static readonly Regex _durationTokenRegex = new(
        @"(?<value>\d+(?:[.,]\d+)?)\s*(?<unit>w|wk|wks|week|weeks|d|day|days|h|hr|hrs|hour|hours|m|min|mins|minute|minutes)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );
    
    public string FormatTime(string timeString)
    {
        var cleanedTime = _cleanUpRegex.Replace(timeString, "");
        var minutesLength = cleanedTime.Length >= 2 ? 2 : cleanedTime.Length;
        var minutesStartIndex = cleanedTime.Length - minutesLength;
        var minutes = 0;
        if (minutesLength > 0)
        {
            minutes = int.Parse(
                cleanedTime.Substring(
                    minutesStartIndex,
                    minutesLength
                )    
            );            
        }

        int hours = 0;
        if (cleanedTime.Length >= 3)
        {
            hours = int.Parse(
                cleanedTime.Substring(
                    0,
                    minutesStartIndex
                )    
            );
        }
        if (minutes >= 60)
        {
            minutes -= 60;
            hours++;
        }
        hours = hours >= 24 ? 00 : hours;

        return $"{hours:00}:{minutes:00}";
    }
    
    public TimeSpan ParseTimeSpan(string timeString)
    {
        var formattedTime = FormatTime(timeString);
        return TimeSpan.Parse(formattedTime);
    }

    public bool TryParseDuration(string? value, out TimeSpan? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var normalizedValue = value.Trim();
        if (TimeSpan.TryParse(normalizedValue, CultureInfo.InvariantCulture, out var timeSpan))
        {
            result = timeSpan;
            return true;
        }

        var matches = _durationTokenRegex.Matches(normalizedValue);
        if (matches.Count == 0)
        {
            return false;
        }

        var remainder = _durationTokenRegex.Replace(normalizedValue, string.Empty);
        if (!string.IsNullOrWhiteSpace(remainder))
        {
            return false;
        }

        double totalMinutes = 0;
        foreach (Match match in matches)
        {
            if (!double.TryParse(
                    match.Groups["value"].Value.Replace(',', '.'),
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out var number))
            {
                return false;
            }

            totalMinutes += match.Groups["unit"].Value.ToLowerInvariant() switch
            {
                "w" or "wk" or "wks" or "week" or "weeks" => number * MinutesPerWeek,
                "d" or "day" or "days" => number * MinutesPerDay,
                "h" or "hr" or "hrs" or "hour" or "hours" => number * MinutesPerHour,
                "m" or "min" or "mins" or "minute" or "minutes" => number,
                _ => 0
            };
        }

        result = TimeSpan.FromMinutes(totalMinutes);
        return true;
    }

    public string TimeSpanToDurationString(TimeSpan? timeSpan)
    {
        if (!timeSpan.HasValue)
        {
            return string.Empty;
        }

        var totalMinutes = (int)Math.Round(timeSpan.Value.TotalMinutes, MidpointRounding.AwayFromZero);
        if (totalMinutes == 0)
        {
            return "0m";
        }

        var parts = new List<string>();

        var weeks = totalMinutes / MinutesPerWeek;
        if (weeks > 0)
        {
            parts.Add($"{weeks}w");
            totalMinutes -= weeks * MinutesPerWeek;
        }

        var days = totalMinutes / MinutesPerDay;
        if (days > 0)
        {
            parts.Add($"{days}d");
            totalMinutes -= days * MinutesPerDay;
        }

        var hours = totalMinutes / MinutesPerHour;
        if (hours > 0)
        {
            parts.Add($"{hours}h");
            totalMinutes -= hours * MinutesPerHour;
        }

        if (totalMinutes > 0)
        {
            parts.Add($"{totalMinutes}m");
        }

        return string.Join(" ", parts);
    }
    
    public TimeOnly ParseTimeOnly(string timeString)
    {
        var formattedTime = FormatTime(timeString);
        return TimeOnly.ParseExact(formattedTime, "HH:mm");
    }
    
    public TimeSpan GetTimeSpanFromDateTime(DateTime dateTime)
    {
        return TimeSpan.Parse(dateTime.ToString("HH:mm"));
    }
    
    public string TimeSpanToTimeString(TimeSpan timeSpan, bool isAddSecond = false)
    {
        return $"{Math.Abs(timeSpan.Hours):0}:{Math.Abs(timeSpan.Minutes):00}"
            + (
                isAddSecond ? $":{Math.Abs(timeSpan.Seconds):00}" : ""
            );
    }

    public string DateTimeToTimeString(DateTimeOffset dateTime, bool isAddSecond = false)
    {
        return $"{Math.Abs(dateTime.Hour):0}:{Math.Abs(dateTime.Minute):00}"
               + (
                   isAddSecond ? $":{Math.Abs(dateTime.Second):00}" : ""
               );
    }
    
    public string DateTimeToTimeString(DateTimeOffset? dateTime, bool isAddSecond = false)
    {
        return dateTime.HasValue ? DateTimeToTimeString(dateTime.Value, isAddSecond) : string.Empty;
    }
    
    public string DateTimeToTimeString(DateTime dateTime, bool isAddSecond = false)
    {
        return $"{Math.Abs(dateTime.Hour):0}:{Math.Abs(dateTime.Minute):00}"
            + (
                isAddSecond ? $":{Math.Abs(dateTime.Second):00}" : ""
            );
    }

    public string DateTimeToTimeString(DateTime? dateTime, bool isAddSecond = false)
    {
        return dateTime.HasValue ? DateTimeToTimeString(dateTime.Value, isAddSecond) : string.Empty;
    }
    
    public string TimeOnlyToStringString(TimeOnly time)
    {
        return time.ToString("HH:mm");
    }
}
