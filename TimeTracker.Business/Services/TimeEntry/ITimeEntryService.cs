using Domain.Abstractions;

namespace TimeTracker.Business.Services.TimeEntry;

public interface ITimeEntryService: IDomainService
{
    Task StopActiveEntriesFromPastDayAsync(CancellationToken cancellationToken = default);
}
