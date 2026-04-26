using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.ExternalClients.Jira.Dto;

namespace TimeTracker.Business.Services.ExternalClients.Jira;

public partial class JiraClient
{
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
        var httpClient = BuildHttpClient(workspace, user);

        var uri = BuildUrl(workspace, user, $"issue/{externalTaskId}");
        _logger.LogDebug("Jira. Send request to: {Uri}", uri);
        var response = await httpClient.GetAsync(uri.ToString());
        return await HandleResponse<GetTaskResponseDto>(uri.ToString(), response);
    }

    protected override async Task<TaskEntity> CreateOrUpdateTimeEntryTaskAsync(
        TaskListEntity taskList,
        UserEntity user,
        string externalTaskId
    )
    {
        var externalTask = await GetValidatedTaskAsync(taskList.Project.Workspace, user, externalTaskId);
        return await CreateTaskAsync(taskList, user, externalTaskId, externalTask);
    }

    protected override async Task<TaskEntity> CreateOrUpdateTimeEntryTaskAsync(
        TimeEntryEntity timeEntry,
        TaskListEntity taskList,
        string externalTaskId
    )
    {
        var externalTask = await GetValidatedTaskAsync(taskList.Project.Workspace, timeEntry.User, externalTaskId);
        var task = await CreateTaskAsync(taskList, timeEntry.User, externalTaskId, externalTask);
        timeEntry.Task = task;
        await _dbSessionProvider.CurrentSession.SaveAsync(timeEntry);
        return task;
    }

    private async Task<GetTaskResponseDto> GetValidatedTaskAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        string externalTaskId
    )
    {
        var externalTask = await GetTaskAsync(workspace, user, externalTaskId);
        if (externalTask == null)
        {
            throw new RecordNotFoundException("Incorrect ExternalTaskId");
        }

        if (externalTask.IsError)
        {
            _logger.LogError(
                $"Jira API returned error: TaskId: {externalTaskId}, Error: {string.Join(",", externalTask.ErrorMessages)}"
            );
            throw new RecordNotFoundException("Incorrect ExternalTaskId");
        }

        return externalTask;
    }

    private async Task<TaskEntity> CreateTaskAsync(
        TaskListEntity taskList,
        UserEntity user,
        string externalTaskId,
        GetTaskResponseDto externalTask
    )
    {
        var taskInfo = BuildTaskInfo(externalTask);
        var task = await _taskDao.AddTaskAsync(
            taskList,
            user,
            externalTask.Fields.Summary!,
            MarkdownHelper.ToMarkdown(externalTask.RenderedFields.DescriptionHtml ?? ""),
            originalEstimate: taskInfo.OriginalEstimate
        );
        task.ExternalTaskId = externalTaskId;
        await _dbSessionProvider.CurrentSession.SaveAsync(task);
        return task;
    }
}
