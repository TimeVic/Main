using Domain.Abstractions;

namespace TimeTracker.Business.Common.Services.Format;

public interface ITimeParsingService: IDomainService
{
    string FormatTime(string timeString);

    TimeSpan ParseTime(string timeString);

    TimeSpan GetTimeSpanFromDateTime(DateTime dateTime);

    string TimeSpanToTimeString(TimeSpan timeSpan, bool isAddSecond = false);
}
