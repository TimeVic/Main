using Domain.Abstractions;

namespace TimeTracker.Business.Common.Services.Format;

public interface ITimeParsingService: IDomainService
{
    string FormatTime(string timeString);

    TimeSpan ParseTimeSpan(string timeString);

    TimeSpan GetTimeSpanFromDateTime(DateTime dateTime);

    string TimeSpanToTimeString(TimeSpan timeSpan, bool isAddSecond = false);

    string DateTimeToTimeString(DateTimeOffset dateTime, bool isAddSecond = false);
    
    string DateTimeToTimeString(DateTimeOffset? dateTime, bool isAddSecond = false);
    
    string DateTimeToTimeString(DateTime dateTime, bool isAddSecond = false);

    string DateTimeToTimeString(DateTime? dateTime, bool isAddSecond = false);

    string TimeOnlyToStringString(TimeOnly time);

    TimeOnly ParseTimeOnly(string timeString);
}
