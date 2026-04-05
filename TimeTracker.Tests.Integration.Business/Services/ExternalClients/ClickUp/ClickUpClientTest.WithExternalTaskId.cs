using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Business.Common.Constants;
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
    public async Task ShouldUseTaskIdFromTaskIfExists()
    {
        var task = await _taskSeeder.CreateAsync(user: _user);
        task.ExternalTaskId = _externalTaskId;
        await FlushDbChanges();
        
        var startTime = DateTime.UtcNow;
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            startTime,
            true,
            internalTask: task
        );
        await FlushDbChanges();
        await _timeEntryDao.StopActiveAsync(_workspace, _user, startTime.AddMinutes(1));
        await FlushDbChanges();
        await DbSessionProvider.CurrentSession.RefreshAsync(activeEntry);
    
        var actualResponse = await _сlickUpClient.SetTimeEntryAsync(activeEntry);
        Assert.NotNull(actualResponse);
        Assert.False(actualResponse.IsError);
        Assert.NotEmpty(actualResponse.Id);

        activeEntry.ClickUpId = actualResponse.Id;
        var isDeleted = await _сlickUpClient.DeleteTimeEntryAsync(activeEntry);
        Assert.True(isDeleted);
    }
}
