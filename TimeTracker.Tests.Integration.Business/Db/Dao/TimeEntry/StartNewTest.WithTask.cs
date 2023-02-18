using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public partial class StartNewTest: BaseTest
{
    [Fact]
    public async Task ShouldRewriteProjectAndOtherIfTaskProvided()
    {
        var task = await _taskSeeder.CreateAsync(user: _user);
        
        var expectHourlyRate = 123.56m;
        
        var workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        var project = await _projectDao.CreateAsync(workspace, "test");
        project.DefaultHourlyRate = expectHourlyRate;
        project.IsBillableByDefault = true;
        await DbSessionProvider.PerformCommitAsync();
        
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            workspace,
            DateTime.UtcNow,
            DateTime.UtcNow.TimeOfDay,
            isBillable: true,
            projectId: project.Id,
            internalTask: task
        );
        Assert.Equal(task.TaskList.Project.Id, activeEntry.Project.Id);
    }
}
