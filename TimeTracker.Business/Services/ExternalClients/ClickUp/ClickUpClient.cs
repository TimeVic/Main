using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp.Model;
using TimeTracker.Business.Services.ExternalClients.Dto;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp;

public partial class ClickUpClient: AExternalClientService, IClickUpClient
{
    private readonly ITaskDao _taskDao;
    private readonly IDbSessionProvider _dbSessionProvider;

    private static readonly Regex DefaultTaskIdRegex = new(@"^\#{0,1}[a-zA-Z0-9]{1,10}$");
    private static readonly Regex CustomTaskIdRegex = new(@"^[a-zA-Z0-9\-]{1,12}$");
    
    private const string BaseUrl = "https://api.clickup.com/api/v2";
    
    private static HttpClient _newHttpClient => new();

    public ClickUpClient(
        ILogger<ClickUpClient> logger,
        ITaskDao taskDao,
        IDbSessionProvider dbSessionProvider
    ): base(logger)
    {
        _taskDao = taskDao;
        _dbSessionProvider = dbSessionProvider;
    }

    public override Task<bool> IsFillTimeEntryDescriptionFromTaskTitle(TimeEntryEntity timeEntry)
    {
        var settings = GetSettings(timeEntry.Workspace, timeEntry.User);
        return Task.FromResult(
            settings.IsFillTimeEntryWithTaskDetails && string.IsNullOrEmpty(timeEntry.Description)    
        );
    }

    public override bool IsCorrectTaskId(TimeEntryEntity timeEntry)
    {
        return IsCorrectTaskId(timeEntry.TaskId ?? "");
    }

    public bool IsCorrectTaskId(string externalTaskId)
    {
        return DefaultTaskIdRegex.IsMatch(externalTaskId ?? "")
            || CustomTaskIdRegex.IsMatch(externalTaskId ?? "");
    }

    protected override async Task<SynchronizedTimeEntryDto?> SendTimeEntryAsync(TimeEntryEntity timeEntry)
    {
        var httpClient = _newHttpClient;
        var settings = GetSettings(timeEntry.Workspace, timeEntry.User);
        
        var startTime = timeEntry.Date.Add(timeEntry.StartTime);
        var endTime = timeEntry.Date.Add(timeEntry.EndTime.Value);
        var requestData = JsonContent.Create(new SetTimeEntryDto()
        {
            Start = startTime.ToUnixTime(),
            End = endTime.ToUnixTime(),
            Description = timeEntry.Description,
            TaskId = CleanUpTaskId(timeEntry.ExternalTaskId, settings.IsCustomTaskIds)
        });
        httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, settings.SecurityKey);
        
        var uri = BuildSetTimeEntryUri(
            settings.TeamId,
            settings.IsCustomTaskIds,
            timeEntry.ClickUpId
        );
        _logger.LogDebug(
            "ClickUp. Send request to: {Uri}. Body: {Body}",
            uri,
            await requestData.ReadAsStringAsync()
        );
        HttpResponseMessage response;
        if (string.IsNullOrEmpty(timeEntry.ClickUpId))
        {
            response = await httpClient.PostAsync(uri, requestData);
            var responseData = await HandleResponse<CreateTimeEntryResponseDto?>(uri, response, requestData);
            if (responseData == null || responseData.IsError)
            {
                _logger.LogDebug(
                    "ClickUp returned error: {error}",
                    responseData.Error
                );
                return new SynchronizedTimeEntryDto { IsError = true };
            }

            var clickUpTask = await GetTaskAsync(timeEntry, timeEntry.ExternalTaskId);
            return new SynchronizedTimeEntryDto()
            {
                Id = responseData.Data?.Id ?? "",
                Comment = responseData.Data?.Description,
                AdditionalDescription = clickUpTask?.Name
            };
        }
        else
        {
            response = await httpClient.PutAsync(uri, requestData);
            var responseData = await HandleResponse<UpdateTimeEntryResponseDto?>(uri, response, requestData);
            if (responseData == null || responseData.IsError)
            {
                _logger.LogDebug(
                    "ClickUp returned error: {error}",
                    responseData.Error
                );
                return new SynchronizedTimeEntryDto { IsError = true };
            }

            var clickUpTask = await GetTaskAsync(timeEntry, timeEntry.ExternalTaskId);
            var responseEntry = responseData.Data.FirstOrDefault();
            return new SynchronizedTimeEntryDto()
            {
                Id = responseEntry.Id ?? "",
                Comment = responseEntry.Description,
                AdditionalDescription = clickUpTask?.Name
            };
        }
    }

    protected override async Task<bool> SendDeleteTimeEntryRequestAsync(TimeEntryEntity timeEntry)
    {
        var httpClient = _newHttpClient;
        var settings = GetSettings(timeEntry.Workspace, timeEntry.User);
        httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, settings.SecurityKey);
        
        var uri = BaseUrl 
            + $"/team/{settings.TeamId}/time_entries/{timeEntry.ClickUpId}";
        _logger.LogDebug("ClickUp. Send request to: {Uri}", uri);
        var response = await httpClient.DeleteAsync(uri);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogDebug(
                $"ClickUp returned status code: {response.StatusCode}"
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
                $"ClickUp returned status code: {response.StatusCode}"
            );
            return false;
        }
        return true;
    }

    private string BuildSetTimeEntryUri(string teamId, bool isCustomTaskIds, string? timeEntryId = null)
    {
        teamId = HttpUtility.UrlEncode(teamId);
        
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams.Add("custom_task_ids", isCustomTaskIds.ToString().ToLower());
        queryParams.Add("team_id", teamId);
        var url = new UriBuilder(
            $"{BaseUrl}/team/{teamId}/time_entries"
            + (!string.IsNullOrEmpty(timeEntryId) ? $"/{timeEntryId}" : "")
        );
        url.Query = queryParams.ToString();
        return url.ToString();
    }

    private WorkspaceSettingsClickUpEntity GetSettings(WorkspaceEntity workspace, UserEntity user)
    {
        var settings = workspace.GetClickUpSettings(user.Id);
        if (settings == null)
        {
            throw new Exception($"ClickUp settings not found WorkspaceId: {workspace.Id}");
        }
        return settings;
    }
    
    public static string CleanUpTaskId(string taskId, bool isCustomTaskId)
    {
        taskId = (taskId ?? "").Trim();
        if (!isCustomTaskId && taskId.StartsWith("#"))
        {
            return taskId.TrimStart('#');
        }
        return taskId;
    }
}
