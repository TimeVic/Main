using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public partial class StartNewTest: BaseTest
{
    [Fact]
    public async Task ShouldRewriteProjectAndOtherIfTaskProvided()
    {
        var task = await _taskSeeder.CreateAsync(user: _user);
        
        var expectHourlyRate = 123.56m;
        
        var workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        var project = await _projectSeeder.CreateAsync(workspace);
        project.DefaultHourlyRate = expectHourlyRate;
        project.IsBillableByDefault = true;
        await FlushDbChanges();
        
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            workspace,
            DateTime.UtcNow,
            isBillable: true,
            projectId: project.Id,
            internalTask: task
        );
        Assert.NotNull(activeEntry.Project);
        Assert.Equal(task.TaskList.Project.Id, activeEntry.Project.Id);
    }

    [Fact]
    public async Task ShouldUpdateTaskStatusToToDoAndCalculateInProgress()
    {
        var task = await _taskSeeder.CreateAsync(user: _user);
        task.Status = TaskStatus.Backlog;
        await DbSessionProvider.CurrentSession.SaveOrUpdateAsync(task);
        await FlushDbChanges();
        await RefreshEntity(task);
        
        Assert.Equal(TaskStatus.Backlog, task.Status);
        Assert.Equal(ExtendedTaskStatus.Backlog, task.ExtendedStatus);
        
        var workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        
        var activeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            workspace,
            DateTime.UtcNow,
            internalTask: task
        );
        await FlushDbChanges();
        await RefreshEntity(task);
        
        Assert.Equal(TaskStatus.ToDo, task.Status);
        Assert.Equal(ExtendedTaskStatus.InProgress, task.ExtendedStatus);

        // Stop active entry and check ExtendedStatus goes back to ToDo
        await _timeEntryDao.StopActiveAsync(workspace, _user, DateTime.UtcNow.AddMinutes(10));
        await FlushDbChanges();
        await RefreshEntity(task);

        Assert.Equal(TaskStatus.ToDo, task.Status);
        Assert.Equal(ExtendedTaskStatus.ToDo, task.ExtendedStatus);
    }
}
