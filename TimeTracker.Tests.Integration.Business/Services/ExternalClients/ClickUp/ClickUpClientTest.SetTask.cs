using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.ClickUp;

public partial class SendNewTimeEntityTest : BaseTest
{
    // [Fact]
    public async Task ShouldCreateTaskByExternalTaskId()
    {
        var project = await _projectSeeder.CreateAsync(_workspace);
        var taskList = await _taskListSeeder.CreateAsync(project);
        await DbSessionProvider.PerformCommitAsync();
    
        var actualTask = await _сlickUpClient.SetTimeEntryTaskAsync(
            taskList,
            _user,
            _externalTaskIdForCreation
        );
        Assert.NotNull(actualTask);
        Assert.Equal(_externalTaskIdForCreation, actualTask.ExternalTaskId);
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
    
        var actualTask = await _сlickUpClient.SetTimeEntryTaskAsync(
            activeEntry,
            taskList,
            _externalTaskIdForCreation
        );
        Assert.NotNull(actualTask);
        Assert.Equal(_externalTaskIdForCreation, actualTask.ExternalTaskId);
        Assert.NotNull(activeEntry.Task);
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
            var actualTask = await _сlickUpClient.SetTimeEntryTaskAsync(
                activeEntry,
                taskList,
                "fake id"
            );
        });
    }
}
