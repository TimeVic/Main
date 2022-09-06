using Domain.Abstractions;

namespace TimeTracker.Business.Common.Services.Format;

public interface ITimeParsingService: IDomainService
{
    string FormatTime(string timeString);

    DateTimeOffset ParseTime(string timeString);
}
