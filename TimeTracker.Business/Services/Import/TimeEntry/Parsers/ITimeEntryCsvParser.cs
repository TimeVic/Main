using Domain.Abstractions;
using TimeTracker.Business.Common.Constants.Import;
using TimeTracker.Business.Services.Import.TimeEntry.Model;

namespace TimeTracker.Business.Services.Import.TimeEntry.Parsers;

public interface ITimeEntryCsvParser : IDomainService
{
    TimeEntryImportSourceType SourceType { get; }

    Task<IReadOnlyList<TimeEntryImportModel>> ParseAsync(Stream csvStream, CancellationToken cancellationToken = default);
}
