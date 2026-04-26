using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Tasks;

public partial class AddTask
{
    [Fact]
    public async Task ShouldAddByClickUpExternalId()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            TaskListId = _taskList.Id,
            ExternalTaskId = _clickUpTaskId
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskDto>();
        Assert.True(actualData.TaskId > 0);
        Assert.Equal(_taskList.Id, actualData.TaskList.Id);
        Assert.NotEmpty(actualData.Title);
        Assert.Equal(_clickUpTaskId, actualData.ExternalTaskId);
        Assert.Equal(ExternalSourceType.ClickUp, actualData.ExternalSourceType);
    }
    
    [Fact]
    public async Task ShouldAddByClickUpExternalIdAndTimeEntry()
    {
        var timeEntry = await _timeEntrySeeder.CreateAsync(_workspace, _user);
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            TaskListId = _taskList.Id,
            ExternalTaskId = _clickUpTaskId,
            TimeEntryId = timeEntry.Id
        });
        response.EnsureSuccessStatusCode();

        await FlushDbChanges();
        
        var actualData = await response.GetJsonDataAsync<TaskDto>();
        Assert.True(actualData.TaskId > 0);
        Assert.Equal(_taskList.Id, actualData.TaskList.Id);
        Assert.NotEmpty(actualData.Title);
        Assert.Equal(_clickUpTaskId, actualData.ExternalTaskId);
        Assert.Equal(ExternalSourceType.ClickUp, actualData.ExternalSourceType);

        await FlushDbChanges(true);
        var actualTimeEntry = await DbSessionProvider.CurrentSession.GetAsync<TimeEntryEntity>(timeEntry.Id);
        Assert.NotNull(actualTimeEntry.Task);
        Assert.Equal(actualData.TaskId, actualTimeEntry.Task.TaskId);
    }
    
    [Fact]
    public async Task ShouldAddByJiraExternalIdAndTimeEntry()
    {
        var timeEntry = await _timeEntrySeeder.CreateAsync(_workspace, _user);
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            TaskListId = _taskList.Id,
            ExternalTaskId = _jiraTaskId,
            TimeEntryId = timeEntry.Id
        });
        response.EnsureSuccessStatusCode();

        await FlushDbChanges();
        
        var actualData = await response.GetJsonDataAsync<TaskDto>();
        Assert.True(actualData.TaskId > 0);
        Assert.Equal(_taskList.Id, actualData.TaskList.Id);
        Assert.NotEmpty(actualData.Title);
        Assert.Equal(_jiraTaskId, actualData.ExternalTaskId);
        Assert.Equal(TimeSpan.FromHours(1), actualData.OriginalEstimate);
        Assert.Equal(ExternalSourceType.Jira, actualData.ExternalSourceType);

        await FlushDbChanges(true);
        var actualTimeEntry = await DbSessionProvider.CurrentSession.GetAsync<TimeEntryEntity>(timeEntry.Id);
        Assert.NotNull(actualTimeEntry.Task);
        Assert.Equal(actualData.TaskId, actualTimeEntry.Task.TaskId);
        Assert.Equal(TimeSpan.FromHours(1), actualTimeEntry.Task.OriginalEstimate);
    }
    
    [Fact]
    public async Task ShouldAddByJiraExternalIdAndTimeEntryWithAdditionalData()
    {
        var expectedTask = _taskFactory.Generate();
        
        var timeEntry = await _timeEntrySeeder.CreateAsync(_workspace, _user);
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            TaskListId = _taskList.Id,
            ExternalTaskId = _jiraTaskId,
            TimeEntryId = timeEntry.Id,
            Status = expectedTask.Status,
            StartTime = expectedTask.StartTime,
            EndTime = expectedTask.EndTime,
            Priority = expectedTask.Priority
        });
        response.EnsureSuccessStatusCode();

        await FlushDbChanges();
        
        var actualData = await response.GetJsonDataAsync<TaskDto>();
        Assert.True(actualData.TaskId > 0);
        Assert.Equal(_taskList.Id, actualData.TaskList.Id);
        Assert.NotEmpty(actualData.Title);
        Assert.Equal(_jiraTaskId, actualData.ExternalTaskId);

        Assert.Equal(expectedTask.Status, actualData.Status);
        Assert.Equal(TimeSpan.FromHours(1), actualData.OriginalEstimate);
        Assert.Equal(ExternalSourceType.Jira, actualData.ExternalSourceType);
        Assert.Equal(expectedTask.StartTime?.ToString("g"), actualData.StartTime?.ToUniversalTime().ToString("g"));
        Assert.Equal(expectedTask.EndTime?.ToString("g"), actualData.EndTime?.ToUniversalTime().ToString("g"));
        Assert.Equal(expectedTask.Priority, actualData.Priority);
        
        await FlushDbChanges(true);
        var actualTimeEntry = await DbSessionProvider.CurrentSession.GetAsync<TimeEntryEntity>(timeEntry.Id);
        Assert.NotNull(actualTimeEntry.Task);
        Assert.Equal(actualData.TaskId, actualTimeEntry.Task.TaskId);
    }
}
