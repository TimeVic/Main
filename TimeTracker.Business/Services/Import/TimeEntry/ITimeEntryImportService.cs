using Domain.Abstractions;
using TimeTracker.Business.Common.Constants.Import;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Import.TimeEntry.Model;

namespace TimeTracker.Business.Services.Import.TimeEntry;

public interface ITimeEntryImportService : IDomainService
{
    Task<TimeEntryImportResultDto> ImportAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        Stream fileStream,
        TimeEntryImportSourceType sourceType,
        bool defaultIsBillable,
        decimal? defaultHourlyRate,
        CancellationToken cancellationToken = default
    );
}
