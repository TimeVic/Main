using System.Globalization;
using System.Text.RegularExpressions;

namespace TimeTracker.Business.Common.Services.Format;

public class TimeParsingService: ITimeParsingService
{
    private readonly Regex _cleanUpRegex = new Regex("[^0-9]");
    
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

        return $"{hours:0}:{minutes:00}";
    }
    
    public TimeSpan ParseTime(string timeString)
    {
        var formattedTime = FormatTime(timeString);
        return TimeSpan.Parse(formattedTime);
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
}
