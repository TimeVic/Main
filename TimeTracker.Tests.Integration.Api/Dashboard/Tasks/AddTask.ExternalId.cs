using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tasks;

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

        await CommitDbChanges();
        
        var actualData = await response.GetJsonDataAsync<TaskDto>();
        Assert.True(actualData.TaskId > 0);
        Assert.Equal(_taskList.Id, actualData.TaskList.Id);
        Assert.NotEmpty(actualData.Title);
        Assert.Equal(_clickUpTaskId, actualData.ExternalTaskId);

        var actualTimeEntry = await DbSessionProvider.CurrentSession.GetAsync<TimeEntryEntity>(timeEntry.Id);
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

        await CommitDbChanges();
        
        var actualData = await response.GetJsonDataAsync<TaskDto>();
        Assert.True(actualData.TaskId > 0);
        Assert.Equal(_taskList.Id, actualData.TaskList.Id);
        Assert.NotEmpty(actualData.Title);
        Assert.Equal(_jiraTaskId, actualData.ExternalTaskId);

        var actualTimeEntry = await DbSessionProvider.CurrentSession.GetAsync<TimeEntryEntity>(timeEntry.Id);
        Assert.Equal(actualData.TaskId, actualTimeEntry.Task.TaskId);
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

        await CommitDbChanges();
        
        var actualData = await response.GetJsonDataAsync<TaskDto>();
        Assert.True(actualData.TaskId > 0);
        Assert.Equal(_taskList.Id, actualData.TaskList.Id);
        Assert.NotEmpty(actualData.Title);
        Assert.Equal(_jiraTaskId, actualData.ExternalTaskId);

        Assert.Equal(expectedTask.Status, actualData.Status);
        Assert.Equal(expectedTask.StartTime?.ToString("g"), actualData.StartTime?.ToUniversalTime().ToString("g"));
        Assert.Equal(expectedTask.EndTime?.ToString("g"), actualData.EndTime?.ToUniversalTime().ToString("g"));
        Assert.Equal(expectedTask.Priority, actualData.Priority);
        
        var actualTimeEntry = await DbSessionProvider.CurrentSession.GetAsync<TimeEntryEntity>(timeEntry.Id);
        Assert.Equal(actualData.TaskId, actualTimeEntry.Task.TaskId);
    }
}
