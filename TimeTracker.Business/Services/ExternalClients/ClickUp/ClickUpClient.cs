using System.Net;
using System.Net.Http.Json;
using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp.Model;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp;

public class ClickUpClient: IClickUpClient
{
    private const string BaseUrl = "https://api.clickup.com/api/v2";
    
    private static HttpClient _newHttpClient => new();

    private readonly ILogger<ClickUpClient> _logger;
    
    public ClickUpClient(ILogger<ClickUpClient> logger)
    {
        _logger = logger;
    }

    public async Task<GetTaskResponseDto?> GetTaskAsync(TimeEntryEntity timeEntry)
    {
        if (!IsValidTimeEntry(timeEntry))
        {
            return null;
        }

        var httpClient = _newHttpClient;
        var settings = timeEntry.Workspace.GetClickUpSettings(timeEntry.User.Id);
        if (settings == null)
        {
            throw new Exception($"ClickUp settings not found EntryId: {timeEntry.Id}");
        }
        if (!settings.IsFillTimeEntryWithTaskDetails || !string.IsNullOrEmpty(timeEntry.Description))
        {
            return null;
        }

        httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, settings.SecurityKey);
        
        var uri = BuildGetTaskUri(
            settings.TeamId,
            timeEntry.TaskId,
            settings.IsCustomTaskIds,
            timeEntry.ClickUpId
        );
        _logger.LogDebug("ClickUp. Send request to: {Uri}", uri);
        HttpResponseMessage response;
        response = await httpClient.GetAsync(uri);
        return await HandleResponse<GetTaskResponseDto>(uri, response);
    }
    
    public async Task<SetTimeEntryResponseDto?> SendTimeEntryAsync(TimeEntryEntity timeEntry)
    {
        if (!IsValidTimeEntry(timeEntry))
        {
            return null;
        }
        
        var httpClient = _newHttpClient;
        var settings = timeEntry.Workspace.GetClickUpSettings(timeEntry.User.Id);
        if (settings == null)
        {
            throw new Exception($"ClickUp settings not found EntryId: {timeEntry.Id}");
        }

        var startTime = timeEntry.Date.Add(timeEntry.StartTime);
        var endTime = timeEntry.Date.Add(timeEntry.EndTime.Value);
        var requestData = JsonContent.Create(new SetTimeEntryDto()
        {
            Start = startTime.ToUnixTime(),
            End = endTime.ToUnixTime(),
        });
        httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, settings.SecurityKey);
        
        var uri = BuildSetTimeEntryUri(
            settings.TeamId,
            timeEntry.TaskId,
            settings.IsCustomTaskIds,
            timeEntry.ClickUpId
        );
        _logger.LogDebug("ClickUp. Send request to: {Uri}", uri);
        HttpResponseMessage response;
        if (timeEntry.ClickUpId.HasValue)
        {
            response = await httpClient.PutAsync(uri, requestData);
        }
        else
        {
            response = await httpClient.PostAsync(uri, requestData);
        }
        var responseData = await HandleResponse<SetTimeEntryResponseDto?>(uri, response, requestData);
        if (responseData == null || responseData.Value.IsError)
        {
            _logger.LogDebug(
                "ClickUp returned error: {error}",
                responseData.Value.Error
            );
        }

        return responseData;
    }

    private async Task<T?> HandleResponse<T>(
        string uri,
        HttpResponseMessage httpResponse,
        JsonContent? request = null
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

    private string BuildSetTimeEntryUri(string teamId, string taskId, bool isCustomTaskIds, long? timeEntryId = null)
    {
        teamId = HttpUtility.UrlEncode(teamId);
        taskId = HttpUtility.UrlEncode(
            CleanUpTaskId(taskId, isCustomTaskIds)
        );
        
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams.Add("custom_task_ids", isCustomTaskIds.ToString().ToLower());
        queryParams.Add("team_id", teamId);
        var url = new UriBuilder(
            $"{BaseUrl}/task/{taskId}/time"
            + (timeEntryId.HasValue ? $"/{timeEntryId}" : "")
        );
        url.Query = queryParams.ToString();
        return url.ToString();
    }
    
    private string BuildGetTaskUri(string teamId, string taskId, bool isCustomTaskIds, long? timeEntryId = null)
    {
        teamId = HttpUtility.UrlEncode(teamId);
        taskId = HttpUtility.UrlEncode(
            CleanUpTaskId(taskId, isCustomTaskIds)
        );
        
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams.Add("custom_task_ids", isCustomTaskIds.ToString().ToLower());
        queryParams.Add("team_id", teamId);
        queryParams.Add("include_subtasks", "false");
        var url = new UriBuilder($"{BaseUrl}/task/{taskId}");
        url.Query = queryParams.ToString();
        return url.ToString();
    }

    public static string CleanUpTaskId(string taskId, bool isCustomTaskId)
    {
        taskId = taskId.Trim();
        if (!isCustomTaskId && taskId.StartsWith("#"))
        {
            return taskId.TrimStart('#');
        }
        return taskId;
    }
    
    private bool IsValidTimeEntry(TimeEntryEntity timeEntry)
    {
        if (timeEntry.IsActive)
        {
            _logger.LogError("TimeEntry is active");
            return false;
        }
        if (string.IsNullOrEmpty(timeEntry.TaskId))
        {
            _logger.LogError("TimeEntry does not contain TaskId: {TimeEntryId}", timeEntry.Id);
            return false;
        }

        return true;
    }
}
