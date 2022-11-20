using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp.Model;
using TimeTracker.Business.Services.ExternalClients.Dto;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp;

public interface IClickUpClient
{
    Task<SynchronizedTimeEntryDto?> SetTimeEntryAsync(TimeEntryEntity timeEntry);

    Task<bool> IsFillTimeEntryDescription(TimeEntryEntity timeEntry);

    Task<GetTaskResponseDto?> GetTaskAsync(TimeEntryEntity timeEntry);

    bool IsCorrectTaskId(TimeEntryEntity timeEntry);

    Task<bool> DeleteTimeEntryAsync(TimeEntryEntity timeEntry);
}
