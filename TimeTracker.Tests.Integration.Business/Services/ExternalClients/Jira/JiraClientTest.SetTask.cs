using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.Jira;

public partial class SendNewTimeEntityTest : BaseTest
{
    // [Fact]
    public async Task ShouldCreateTaskByExternalTaskId()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var taskList = await _taskListSeeder.CreateAsync(project);
        await DbSessionProvider.PerformCommitAsync();
    
        var actualTask = await _client.SetTimeEntryTaskAsync(
            taskList,
            _user,
            _taskId
        );
        Assert.NotNull(actualTask);
        Assert.Equal(_taskId, actualTask.ExternalTaskId);
    }
    
    // [Fact]
    public async Task ShouldCreateTaskByExternalTaskIdForActiveTimeEntry()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var taskList = await _taskListSeeder.CreateAsync(project);
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            DateTime.UtcNow.Date.ToDateOnly(),
            DateTime.UtcNow.TimeOfDay,
            true
        );
        await DbSessionProvider.PerformCommitAsync();
    
        var actualTask = await _client.SetTimeEntryTaskAsync(
            activeEntry,
            taskList,
            _taskId
        );
        Assert.NotNull(actualTask);
        Assert.Equal(_taskId, actualTask.ExternalTaskId);
        Assert.Equal(activeEntry.Task.Id, actualTask.Id);
    }
    
    // [Fact]
    public async Task ShouldThrowExceptionIfCreateTaskByExternalTaskIdForActiveTimeEntryIsIncorrect()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var taskList = await _taskListSeeder.CreateAsync(project);
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            DateTime.UtcNow.Date.ToDateOnly(),
            DateTime.UtcNow.TimeOfDay,
            true
        );
        await DbSessionProvider.PerformCommitAsync();

        await Assert.ThrowsAsync<RecordNotFoundException>(async () =>
        {
            var actualTask = await _client.SetTimeEntryTaskAsync(
                activeEntry,
                taskList,
                "fake id"
            );
        });
    }
}
