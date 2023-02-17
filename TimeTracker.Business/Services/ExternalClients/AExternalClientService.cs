using System.Net;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
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
    
    public async Task<bool> IsValidClientSettings(WorkspaceEntity workspace, UserEntity user)
    {
        return await SendSettingsValidationRequest(workspace, user);
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
            _logger.LogError("TimeEntry is active");
            return false;
        }
        if (string.IsNullOrEmpty(timeEntry.ExternalTaskId))
        {
            _logger.LogError("TimeEntry does not contain TaskId: {TimeEntryId}", timeEntry.Id);
            return false;
        }

        return true;
    }

    public abstract Task<bool> IsFillTimeEntryDescription(TimeEntryEntity timeEntry);
    
    public abstract bool IsCorrectTaskId(TimeEntryEntity timeEntry);
    
    protected abstract Task<SynchronizedTimeEntryDto?> SendTimeEntryAsync(TimeEntryEntity timeEntry);
    
    protected abstract Task<bool> SendDeleteTimeEntryRequestAsync(TimeEntryEntity timeEntry);
    
    protected abstract Task<bool> SendSettingsValidationRequest(WorkspaceEntity workspace, UserEntity user);
}
