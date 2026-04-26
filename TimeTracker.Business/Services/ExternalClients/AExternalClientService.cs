using System.Net;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.ExternalClients.Dto;

namespace TimeTracker.Business.Services.ExternalClients;

public abstract class AExternalClientService
{
    protected readonly ILogger<AExternalClientService> _logger;

    protected static HttpClient _newHttpClient => new();
    
    public AExternalClientService(ILogger<AExternalClientService> logger)
    {
        _logger = logger;
    }

    public async Task<SynchronizedTimeEntryDto?> SetTimeEntryAsync(TimeEntryEntity timeEntry)
    {
        if (!IsValidTimeEntry(timeEntry))
        {
            return null;
        }

        return await SendTimeEntryAsync(timeEntry);
    }
    
    public async Task<bool> DeleteTimeEntryAsync(TimeEntryEntity timeEntry)
    {
        if (!IsValidTimeEntry(timeEntry))
        {
            return false;
        }

        return await SendDeleteTimeEntryRequestAsync(timeEntry);
    }
    
    public async Task<TaskEntity> SetTimeEntryTaskAsync(
        TimeEntryEntity timeEntry,
        TaskListEntity taskListEntity,
        string externalTaskId
    )
    {
        return await CreateOrUpdateTimeEntryTaskAsync(timeEntry, taskListEntity, externalTaskId);
    }
    
    public async Task<TaskEntity> SetTimeEntryTaskAsync(
        TaskListEntity taskListEntity,
        UserEntity user,
        string externalTaskId
    )
    {
        return await CreateOrUpdateTimeEntryTaskAsync(taskListEntity, user, externalTaskId);
    }
    
    public async Task<bool> IsValidClientSettings(WorkspaceEntity workspace, UserEntity user)
    {
        return await SendSettingsValidationRequest(workspace, user);
    }

    public async Task<ExternalTaskInfoDto?> GetTaskInfoAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        string externalTaskId
    )
    {
        if (string.IsNullOrWhiteSpace(externalTaskId))
        {
            _logger.LogError("ExternalTaskId is empty");
            return null;
        }

        return await GetTaskInfoInternalAsync(workspace, user, externalTaskId);
    }

    protected async Task<T?> HandleResponse<T>(
        string uri,
        HttpResponseMessage httpResponse,
        HttpContent? request = null
    )
    {
        if (httpResponse.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogDebug("Returned status code: {code}", httpResponse.StatusCode);
        }

        var responseData = await httpResponse.GetJsonDataAsync<T?>();
        if (responseData == null)
        {
            _logger.LogDebug(
                "Returned empty response for url: {S} and data: {ReadAsStringAsync}",
                uri,
                request != null ? await request.ReadAsStringAsync() : ""
            );
            return default;
        }
        return responseData;
    }
    
    protected bool IsValidTimeEntry(TimeEntryEntity timeEntry)
    {
        if (timeEntry.IsActive)
        {
            _logger.LogError($"TimeEntry is active: {timeEntry.Id}");
            return false;
        }
        if (string.IsNullOrEmpty(timeEntry.ExternalTaskId))
        {
            _logger.LogError("TimeEntry does not contain TaskId: {TimeEntryId}", timeEntry.Id);
            return false;
        }

        return true;
    }

    public abstract Task<bool> IsFillTimeEntryDescriptionFromTaskTitle(TimeEntryEntity timeEntry);
    
    public abstract bool IsCorrectTaskId(TimeEntryEntity timeEntry);
    
    protected abstract Task<SynchronizedTimeEntryDto?> SendTimeEntryAsync(TimeEntryEntity timeEntry);
    
    protected abstract Task<bool> SendDeleteTimeEntryRequestAsync(TimeEntryEntity timeEntry);
    
    protected abstract Task<bool> SendSettingsValidationRequest(WorkspaceEntity workspace, UserEntity user);
    
    protected abstract Task<TaskEntity> CreateOrUpdateTimeEntryTaskAsync(
        TimeEntryEntity timeEntry,
        TaskListEntity taskList,
        string externalTaskId
    );

    protected abstract Task<TaskEntity> CreateOrUpdateTimeEntryTaskAsync(
        TaskListEntity taskList,
        UserEntity user,
        string externalTaskId
    );

    protected virtual Task<ExternalTaskInfoDto?> GetTaskInfoInternalAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        string externalTaskId
    )
    {
        return Task.FromResult<ExternalTaskInfoDto?>(null);
    }
}
