using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Redmine;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.ExternalClients.Redmine;

public partial class RedmineClientTest
{
    [Fact]
    public async Task ShouldUseTaskIdFromTaskIfExists()
    {
        var task = await _taskSeeder.CreateAsync(user: _user);
        task.ExternalTaskId = _taskId;
        await DbSessionProvider.PerformCommitAsync();
        
        var expectedDescription = "Test description";
        var date = DateTime.UtcNow.Date;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            DateTime.UtcNow.Date,
            TimeSpan.FromMinutes(1),
            true,
            description: expectedDescription,
            internalTask: task
        );
        await DbSessionProvider.PerformCommitAsync();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, TimeSpan.FromMinutes(2), date);
        await CommitDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var actualResponse = await _redmineClient.SetTimeEntryAsync(activeEntry);
        Assert.NotEmpty(actualResponse.Id);
        Assert.Equal(expectedDescription, actualResponse.Comment);

        activeEntry.RedmineId = actualResponse.Id;
        var isDeleted = await _redmineClient.DeleteTimeEntryAsync(activeEntry);
        Assert.True(isDeleted);
    }
}
