using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.ExternalClients.ClickUp.Model;
using TimeTracker.Business.Services.ExternalClients.Dto;
using SetTimeEntryDto = TimeTracker.Business.Services.ExternalClients.Jira.Dto.SetTimeEntryDto;
using TimeEntryDto = TimeTracker.Business.Services.ExternalClients.Jira.Dto.TimeEntryDto;

namespace TimeTracker.Business.Services.ExternalClients.Jira;

public partial class JiraClient: AExternalClientService, IJiraClient
{
    private readonly ITaskDao _taskDao;
    private readonly IDbSessionProvider _dbSessionProvider;

    private static readonly Regex TaskIdRegex = new(@"^[a-zA-Z0-9\-]{1,12}$");
    
    private const string BaseUrl = "https://lampego.atlassian.net/rest/api/3";
    
    public JiraClient(
        ILogger<JiraClient> logger,
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
        return TaskIdRegex.IsMatch(externalTaskId ?? "");
    }

    protected override async Task<SynchronizedTimeEntryDto?> SendTimeEntryAsync(TimeEntryEntity timeEntry)
    {
        var httpClient = BuildHttpClient(timeEntry.Workspace, timeEntry.User);
        
        var request = new SetTimeEntryDto
        {
            Started = timeEntry.Date.Add(timeEntry.StartTime).ToString("yyyy-MM-ddTHH:mm:ss.fff+0000"),
            TimeSpentSeconds = GetTotalSeconds(timeEntry)
        };
        request.Comment.SetText(timeEntry.Description);
        var requestData = JsonContent.Create(request);
        var uri = BuildChangeTimeEntryUri(timeEntry.ExternalTaskId, timeEntry.JiraId);
        _logger.LogDebug(
            "Jira. Send request to: {Uri}. Body: {Body}",
            uri,
            await requestData.ReadAsStringAsync()
        );
        HttpResponseMessage response;
        if (timeEntry.JiraId.HasValue)
        {
            response = await httpClient.PutAsync(uri, requestData);
        }
        else
        {
            response = await httpClient.PostAsync(uri, requestData);
        }
        var responseData = await HandleResponse<TimeEntryDto?>(uri, response, requestData);
        if (responseData == null || responseData.IsError)
        {
            _logger.LogDebug(
                "Jira returned error: {error}",
                responseData.ErrorMessages
            );
            return new SynchronizedTimeEntryDto { IsError = true };
        }

        var jiraTask = await GetTaskAsync(timeEntry, timeEntry.ExternalTaskId);
        return new SynchronizedTimeEntryDto()
        {
            Id = responseData.Id.ToString(),
            Comment = responseData.Comment.GetText(),
            AdditionalDescription = MarkdownHelper.ToMarkdown(
                jiraTask?.RenderedFields.DescriptionHtml ?? ""
            )
        };
    }

    protected override async Task<bool> SendDeleteTimeEntryRequestAsync(TimeEntryEntity timeEntry)
    {
        var httpClient = BuildHttpClient(timeEntry.Workspace, timeEntry.User);
        
        var uri = BuildChangeTimeEntryUri(timeEntry.ExternalTaskId, timeEntry.JiraId);
        _logger.LogDebug("Jira. Send request to: {Uri}", uri);
        var response = await httpClient.DeleteAsync(uri);
        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            _logger.LogDebug(
                $"Jira returned status code: {response.StatusCode}"
            );
            return false;
        }
        return true;
    }

    protected override async Task<bool> SendSettingsValidationRequest(WorkspaceEntity workspace, UserEntity user)
    {
        var httpClient = BuildHttpClient(workspace, user);
        var uri = new Uri(BaseUrl + $"/events", UriKind.Absolute);
        _logger.LogDebug("Jira. Send checking request to: {Uri}", uri);
        var response = await httpClient.GetAsync(uri);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogDebug(
                $"Jira returned status code: {response.StatusCode}"
            );
            return false;
        }
        return true;
    }

    private string BuildChangeTimeEntryUri(string taskId, long? timeEntryId = null)
    {
        var url = new UriBuilder(
            $"{BaseUrl}/issue/{taskId}/worklog"
            + (timeEntryId.HasValue ? $"/{timeEntryId}" : "")
        );
        return url.ToString();
    }

    private WorkspaceSettingsJiraEntity GetSettings(WorkspaceEntity workspace, UserEntity user)
    {
        var settings = workspace.GetJiraSettings(user.Id);
        if (settings == null)
        {
            throw new Exception($"Jira settings not found WorkspaceId: {workspace.Id}");
        }
        return settings;
    }

    private HttpClient BuildHttpClient(WorkspaceEntity workspace, UserEntity user)
    {
        var settings = workspace.GetJiraSettings(user.Id);
        if (settings == null)
            throw new NullReferenceException("Jira settings not found");
        var httpClient = new HttpClient();
        httpClient.AddBasicAuthCredentials(settings.UserName, settings.ApiKey);
        return httpClient;
    }

    private long GetTotalSeconds(TimeEntryEntity timeEntry)
    {
        var duration = timeEntry.EndTime - timeEntry.StartTime;
        var totalSeconds = (long)(duration?.TotalSeconds ?? 0);   
        if (totalSeconds < 60)
        {
            totalSeconds = 60;
        }
        return totalSeconds;
    }
}
