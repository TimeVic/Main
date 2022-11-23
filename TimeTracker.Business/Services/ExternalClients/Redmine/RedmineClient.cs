using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.Dto;
using TimeTracker.Business.Services.ExternalClients.Redmine.Dto;

namespace TimeTracker.Business.Services.ExternalClients.Redmine;

public class RedmineClient: AExternalClientService, IRedmineClient
{
    /**
     * Redmine integration documentation: https://www.redmine.org/projects/redmine/wiki/Rest_TimeEntries
     */

    private static readonly Regex TaskRegex = new(@"^[0-9]+$");
    
    private const string GetIssuesUrl = "{0}/time_entries.json";
    private const string CreateTimeEntryUrl = "{0}/time_entries.json";
    private const string UpdateTimeEntryUrl = "{0}/time_entries/{1}.json";
    private const string DeleteTimeEntryUrl = "{0}/time_entries/{1}.json";
    
    public RedmineClient(ILogger<RedmineClient> logger): base(logger)
    {
    }

    public override Task<bool> IsFillTimeEntryDescription(TimeEntryEntity timeEntry)
    {
        return Task.FromResult(false);
    }

    public override bool IsCorrectTaskId(TimeEntryEntity timeEntry)
    {
        return TaskRegex.IsMatch(timeEntry.TaskId);
    }

    protected override async Task<SynchronizedTimeEntryDto?> SendTimeEntryAsync(TimeEntryEntity timeEntry)
    {
        var httpClient = _newHttpClient;
        var settings = GetSettings(timeEntry);

        var jsonContent = JsonConvert.SerializeObject(new SetTimeEntryRequestDto()
        {
            TimeEntryRequest = new TimeEntryRequestDto()
            {
                Duration = timeEntry.Duration,
                TaskId = timeEntry.TaskId,
                UserId = settings.RedmineUserId,
                Comments = timeEntry.Description ?? "",
                SpentOnDate = timeEntry.Date,
                ActivityId = settings.ActivityId
            }
        });
        var requestData = new StringContent(
            jsonContent,
            Encoding.UTF8, 
            "application/json"
        );
        httpClient.DefaultRequestHeaders.Add("X-Redmine-API-Key", settings.ApiKey);
        HttpResponseMessage httpResponse;
        string url = "";
        if (string.IsNullOrEmpty(timeEntry.RedmineId))
        {
            url = string.Format(CreateTimeEntryUrl, settings.Url);
            httpResponse = await httpClient.PostAsync(
                url,
                requestData
            );
        }
        else
        {
            url = string.Format(UpdateTimeEntryUrl, settings.Url, timeEntry.RedmineId);
            httpResponse = await httpClient.PutAsync(
                url,
                requestData
            );
        }
        if (
            httpResponse.StatusCode != HttpStatusCode.OK
            && httpResponse.StatusCode != HttpStatusCode.Created
            && httpResponse.StatusCode != HttpStatusCode.Accepted
        )
        {
            _logger.LogDebug("Returned status code: {code}", httpResponse.StatusCode);
            return new SynchronizedTimeEntryDto() { IsError = true };
        }

        if (!string.IsNullOrEmpty(timeEntry.RedmineId))
        {
            // Is updated
            return new SynchronizedTimeEntryDto()
            {
                Id = timeEntry.RedmineId,
                IsError = false
            };
        }

        var responseData = await HandleResponse<SetTimeEntryResponseDto?>(url, httpResponse, requestData);
        if (responseData == null || !responseData.IsSuccess)
        {
            _logger.LogDebug(
                "Redmine returned error: {error}",
                string.Join(",", responseData.Errors)
            );
            return new SynchronizedTimeEntryDto() { IsError = true };
        }
        return new SynchronizedTimeEntryDto()
        {
            Id = responseData.TimeEntryRequest.Id.ToString(),
            Description = responseData.TimeEntryRequest.Issue.Name,
            Comment = responseData.TimeEntryRequest.Comments,
            IsError = false
        };
    }
    
    protected override async Task<bool> SendDeleteTimeEntryRequestAsync(TimeEntryEntity timeEntry)
    {
        var httpClient = _newHttpClient;
        var settings = GetSettings(timeEntry);

        httpClient.DefaultRequestHeaders.Add("X-Redmine-API-Key", settings.ApiKey);
        HttpResponseMessage httpResponse;
        httpResponse = await httpClient.DeleteAsync(
            string.Format(DeleteTimeEntryUrl, settings.Url, timeEntry.RedmineId)
        );
        if (
            httpResponse.StatusCode != HttpStatusCode.OK
            && httpResponse.StatusCode != HttpStatusCode.Created
            && httpResponse.StatusCode != HttpStatusCode.Accepted
        )
        {
            _logger.LogDebug("Returned status code: {code}", httpResponse.StatusCode);
            return false;
        }

        return true;
    }

    protected override async Task<bool> SendSettingsValidationRequest(WorkspaceEntity workspace, UserEntity user)
    {
        var httpClient = _newHttpClient;
        var settings = workspace.GetRedmineSettings(user.Id);

        httpClient.DefaultRequestHeaders.Add("X-Redmine-API-Key", settings.ApiKey);

        var url = string.Format(GetIssuesUrl, settings.Url);
        var urlParams = new Dictionary<string, string>();
        urlParams.Add("offset", "0");
        urlParams.Add("limit", "1");
        var uri = new Uri(QueryHelpers.AddQueryString(url, urlParams), UriKind.Absolute);
        try
        {
            var httpResponse = await httpClient.GetAsync(uri);
            return httpResponse.StatusCode == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            _logger.LogTrace("Redmine. Settings validation failed");
        }

        return false;
    }

    private WorkspaceSettingsRedmineEntity GetSettings(TimeEntryEntity timeEntry)
    {
        var settings = timeEntry.Workspace.GetRedmineSettings(timeEntry.User.Id);
        if (settings == null)
        {
            throw new Exception($"Redmine settings not found EntryId: {timeEntry.Id}");
        }

        return settings;
    }
}
