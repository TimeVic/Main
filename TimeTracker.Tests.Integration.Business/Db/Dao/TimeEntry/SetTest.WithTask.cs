using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Exceptions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public partial class SetTest: BaseTest
{
    [Fact]
    public async Task ShouldRewriteProjectAndOtherIfTaskProvided()
    {
        var task = await _taskSeeder.CreateAsync(user: _user);
        
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var expectedDto = new TimeEntryCreationDto()
        {
            Description = fakeTimeEntry.Description,
            EndTime = fakeTimeEntry.EndTime!.Value,
            StartTime = fakeTimeEntry.StartTime,
            HourlyRate = fakeTimeEntry.HourlyRate,
            IsBillable = fakeTimeEntry.IsBillable
        };
        
        var expectWorkspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();;
        var expectProject = await _projectSeeder.CreateAsync(expectWorkspace);
        var initialEntry = await _timeEntryDao.StartNewAsync(
            _user,
            expectWorkspace,
            DateTime.UtcNow,
            fakeTimeEntry.IsBillable,
            fakeTimeEntry.Description,
            expectProject.Id,
            internalTask: task
        );

        expectedDto.Id = initialEntry.Id;
        
        await FlushDbChanges();
        var actualEntry = await _timeEntryDao.SetAsync(_user, expectWorkspace, expectedDto, expectProject);
        Assert.NotNull(actualEntry.Project);
        Assert.Equal(task.TaskList.Project.Id, actualEntry.Project.Id);
    }

    [Fact]
    public async Task ShouldUpdateTaskStatusToToDoWhenSettingActiveTimeEntry()
    {
        var task = await _taskSeeder.CreateAsync(user: _user);
        task.Status = TaskStatus.Backlog;
        await DbSessionProvider.CurrentSession.SaveOrUpdateAsync(task);
        await FlushDbChanges();
        
        var expectWorkspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        var expectProject = await _projectSeeder.CreateAsync(expectWorkspace);
        
        var initialEntry = await _timeEntryDao.StartNewAsync(
            _user,
            expectWorkspace,
            DateTime.UtcNow,
            internalTask: task
        );
        await FlushDbChanges();

        var expectedDto = new TimeEntryCreationDto()
        {
            Id = initialEntry.Id,
            Description = "Updated active description",
            StartTime = initialEntry.StartTime,
            EndTime = null,
            HourlyRate = 100,
            IsBillable = false
        };

        var actualEntry = await _timeEntryDao.SetAsync(_user, expectWorkspace, expectedDto, expectProject);
        await FlushDbChanges();
        await RefreshEntity(task);

        Assert.Equal(TaskStatus.ToDo, task.Status);
        Assert.Equal(ExtendedTaskStatus.InProgress, task.ExtendedStatus);
    }
}
