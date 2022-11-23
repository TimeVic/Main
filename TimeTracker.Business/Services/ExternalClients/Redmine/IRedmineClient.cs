using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.Dto;
using TimeTracker.Business.Services.ExternalClients.Redmine.Dto;

namespace TimeTracker.Business.Services.ExternalClients.Redmine;

public interface IRedmineClient: IDomainService
{
    Task<SynchronizedTimeEntryDto?> SetTimeEntryAsync(TimeEntryEntity timeEntry);

    Task<bool> IsFillTimeEntryDescription(TimeEntryEntity timeEntry);
    
    Task<bool> DeleteTimeEntryAsync(TimeEntryEntity timeEntry);

    bool IsCorrectTaskId(TimeEntryEntity timeEntry);

    Task<bool> IsValidClientSettings(WorkspaceEntity workspace, UserEntity user);
}
