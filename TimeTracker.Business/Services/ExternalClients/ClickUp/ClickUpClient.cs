using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp.Model;
using TimeTracker.Business.Services.ExternalClients.Dto;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp;

public class ClickUpClient: AExternalClientService, IClickUpClient
{
    private static readonly Regex DefaultTaskIdRegex = new(@"^\#{0,1}[a-zA-Z0-9]{1,10}$");
    private static readonly Regex CustomTaskIdRegex = new(@"^[a-zA-Z0-9\-]{1,12}$");
    
    private const string BaseUrl = "https://api.clickup.com/api/v2";
    
    private static HttpClient _newHttpClient => new();

    public ClickUpClient(ILogger<ClickUpClient> logger): base(logger)
    {
    }

    public override Task<bool> IsFillTimeEntryDescription(TimeEntryEntity timeEntry)
    {
        var settings = GetSettings(timeEntry);
        return Task.FromResult(
            settings.IsFillTimeEntryWithTaskDetails && string.IsNullOrEmpty(timeEntry.Description)    
        );
    }

    public override bool IsCorrectTaskId(TimeEntryEntity timeEntry)
    {
        return DefaultTaskIdRegex.IsMatch(timeEntry.TaskId ?? "")
            || CustomTaskIdRegex.IsMatch(timeEntry.TaskId ?? "");
    }
    
    public async Task<GetTaskResponseDto?> GetTaskAsync(TimeEntryEntity timeEntry)
    {
        var httpClient = _newHttpClient;
        var settings = GetSettings(timeEntry);
        httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, settings.SecurityKey);
        
        var uri = BuildGetTaskUri(
            settings.TeamId,
            timeEntry.TaskId,
            settings.IsCustomTaskIds,
            timeEntry.ClickUpId
        );
        _logger.LogDebug("ClickUp. Send request to: {Uri}", uri);
        var response = await httpClient.GetAsync(uri);
        return await HandleResponse<GetTaskResponseDto>(uri, response);
    }

    protected override async Task<SynchronizedTimeEntryDto?> SendTimeEntryAsync(TimeEntryEntity timeEntry)
    {
        var httpClient = _newHttpClient;
        var settings = GetSettings(timeEntry);
        
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
        if (string.IsNullOrEmpty(timeEntry.ClickUpId))
        {
            response = await httpClient.PostAsync(uri, requestData);
        }
        else
        {
            response = await httpClient.PutAsync(uri, requestData);
        }
        var responseData = await HandleResponse<SetTimeEntryResponseDto?>(uri, response, requestData);
        if (responseData == null || responseData.Value.IsError)
        {
            _logger.LogDebug(
                "ClickUp returned error: {error}",
                responseData.Value.Error
            );
            return new SynchronizedTimeEntryDto { IsError = true };
        }

        var clickUpTask = await GetTaskAsync(timeEntry);
        return new SynchronizedTimeEntryDto()
        {
            Id = responseData.Value.Id?.ToString() ?? "",
            Description = clickUpTask?.Name
        };
    }

    protected override async Task<bool> SendDeleteTimeEntryRequestAsync(TimeEntryEntity timeEntry)
    {
        var httpClient = _newHttpClient;
        var settings = GetSettings(timeEntry);
        httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, settings.SecurityKey);
        
        var uri = BaseUrl 
            + $"/team/{settings.TeamId}/time_entries/{timeEntry.ClickUpId}";
        _logger.LogDebug("ClickUp. Send request to: {Uri}", uri);
        var response = await httpClient.DeleteAsync(uri);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogDebug(
                "ClickUp returned status code: {response.StatusCode}"
            );
            return false;
        }
        return true;
    }

    protected override async Task<bool> SendSettingsValidationRequest(WorkspaceEntity workspace, UserEntity user)
    {
        var httpClient = _newHttpClient;
        var settings = workspace.GetClickUpSettings(user.Id);
        httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, settings.SecurityKey);
        
        var queryParams = new Dictionary<string, string>();
        queryParams.Add("custom_task_ids", settings.IsCustomTaskIds.ToString().ToLower());
        queryParams.Add("team_id", settings.TeamId);

        var url = BaseUrl + $"/team/{settings.TeamId}/time_entries";
        var uri = new Uri(QueryHelpers.AddQueryString(url, queryParams), UriKind.Absolute);
        _logger.LogDebug("ClickUp. Send checking request to: {Uri}", uri);
        var response = await httpClient.GetAsync(uri);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogDebug(
                "ClickUp returned status code: {response.StatusCode}"
            );
            return false;
        }
        return true;
    }

    private string BuildSetTimeEntryUri(string teamId, string taskId, bool isCustomTaskIds, string? timeEntryId = null)
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
            + (!string.IsNullOrEmpty(timeEntryId) ? $"/{timeEntryId}" : "")
        );
        url.Query = queryParams.ToString();
        return url.ToString();
    }
    
    private string BuildGetTaskUri(string teamId, string taskId, bool isCustomTaskIds, string? timeEntryId = null)
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

    private WorkspaceSettingsClickUpEntity GetSettings(TimeEntryEntity timeEntry)
    {
        var settings = timeEntry.Workspace.GetClickUpSettings(timeEntry.User.Id);
        if (settings == null)
        {
            throw new Exception($"ClickUp settings not found EntryId: {timeEntry.Id}");
        }
        return settings;
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
}
