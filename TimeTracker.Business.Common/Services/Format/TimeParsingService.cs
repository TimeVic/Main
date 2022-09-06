using System.Globalization;
using System.Text.RegularExpressions;

namespace TimeTracker.Business.Common.Services.Format;

public class TimeParsingService: ITimeParsingService
{
    private readonly Regex _cleanUpRegex = new Regex("[^0-9]");
    
    public string FormatTime(string timeString)
    {
        var cleanedTime = _cleanUpRegex.Replace(timeString, "");
        var hours = int.Parse(
            cleanedTime.Substring(
                0,
                cleanedTime.Length >= 2 ? 2 : cleanedTime.Length
            )    
        );
        int minutes = 0;
        if (cleanedTime.Length >= 3)
        {
            minutes = int.Parse(
                cleanedTime.Substring(
                    2,
                    cleanedTime.Length >= 4 ? 2 : cleanedTime.Length
                )    
            );    
        }
        minutes = minutes >= 60 ? 60 : minutes;
        if (minutes == 60)
        {
            minutes = 0;
            hours++;
        }
        hours = hours > 24 ? 00 : hours;

        return $"{hours:0}:{minutes:00}";
    }
    
    public DateTimeOffset ParseTime(string timeString)
    {
        var formattedTime = FormatTime(timeString);
        return DateTimeOffset.Parse(formattedTime);
    }
}
