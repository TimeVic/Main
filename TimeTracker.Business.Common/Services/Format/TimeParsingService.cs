using System.Globalization;
using System.Text.RegularExpressions;

namespace TimeTracker.Business.Common.Services.Format;

public class TimeParsingService: ITimeParsingService
{
    private readonly Regex _cleanUpRegex = new Regex("[^0-9]");
    
    public string FormatTime(string timeString)
    {
        var cleanedTime = _cleanUpRegex.Replace(timeString, "");
        var hoursLength = cleanedTime.Length >= 2 ? 2 : cleanedTime.Length;
        var hours = 0;
        if (hoursLength > 0)
        {
            hours = int.Parse(
                cleanedTime.Substring(
                    0,
                    hoursLength
                )    
            );            
        }

        int minutes = 0;
        if (cleanedTime.Length >= 3)
        {
            var minusLength = cleanedTime.Length - hoursLength;
            var minutesString = cleanedTime.Substring(
                2,
                minusLength >= 2 ? 2 : minusLength
            );
            if (minutesString.Length == 1)
                minutesString += "0";
            minutes = int.Parse(minutesString);
        }
        minutes = minutes >= 60 ? 60 : minutes;
        if (minutes == 60)
        {
            minutes = 0;
            hours++;
        }
        hours = hours >= 24 ? 00 : hours;

        return $"{hours:0}:{minutes:00}";
    }
    
    public DateTimeOffset ParseTime(string timeString)
    {
        var formattedTime = FormatTime(timeString);
        return DateTimeOffset.Parse(formattedTime);
    }
}
