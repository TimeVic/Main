using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.ExternalClients.ClickUp.Model;
using TimeTracker.Business.Services.ExternalClients.Dto;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp;

public partial class ClickUpClient
{
    private string BuildGetTaskUri(string teamId, string taskId, bool isCustomTaskIds)
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

    public async Task<GetTaskResponseDto?> GetTaskAsync(TimeEntryEntity timeEntry, string externalTaskId)
    {
        return await GetTaskAsync(timeEntry.Workspace, timeEntry.User, externalTaskId);
    }

    public async Task<GetTaskResponseDto?> GetTaskAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        string externalTaskId
    )
    {
        var httpClient = _newHttpClient;
        var settings = GetSettings(workspace, user);
        httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, settings.SecurityKey);

        var uri = BuildGetTaskUri(
            settings.TeamId,
            externalTaskId,
            settings.IsCustomTaskIds
        );
        _logger.LogDebug("ClickUp. Send request to: {Uri}", uri);
        var response = await httpClient.GetAsync(uri);
        return await HandleResponse<GetTaskResponseDto>(uri, response);
    }

    protected override async Task<TaskEntity> CreateOrUpdateTimeEntryTaskAsync(
        TaskListEntity taskList,
        UserEntity user,
        string externalTaskId
    )
    {
        var externalTask = await GetTaskAsync(taskList.Project.Workspace, user, externalTaskId);
        if (externalTask == null)
        {
            throw new RecordNotFoundException("Incorrect ExternalTaskId");
        }

        if (externalTask.Value.IsError)
        {
            _logger.LogError(
                $"ClickUp API returned error: TaskId: {externalTaskId}, Error: {externalTask.Value.Error}");
            throw new RecordNotFoundException("Incorrect ExternalTaskId");
        }

        var task = await _taskDao.AddTaskAsync(
            taskList,
            user,
            externalTask.Value.Name,
            externalTask.Value.Description
        );
        task.ExternalTaskId = externalTaskId;
        await _dbSessionProvider.CurrentSession.SaveAsync(task);
        return task;
    }

    protected override async Task<TaskEntity> CreateOrUpdateTimeEntryTaskAsync(
        TimeEntryEntity timeEntry,
        TaskListEntity taskList,
        string externalTaskId
    )
    {
        var task = await CreateOrUpdateTimeEntryTaskAsync(
            taskList,
            timeEntry.User,
            externalTaskId
        );
        timeEntry.Task = task;
        await _dbSessionProvider.CurrentSession.SaveAsync(timeEntry);
        return task;
    }
}
