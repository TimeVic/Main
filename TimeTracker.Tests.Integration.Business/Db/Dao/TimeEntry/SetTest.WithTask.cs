using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Exceptions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

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
            EndTime = fakeTimeEntry.EndTime.Value,
            StartTime = fakeTimeEntry.StartTime,
            HourlyRate = fakeTimeEntry.HourlyRate,
            IsBillable = fakeTimeEntry.IsBillable
        };
        
        var expectWorkspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();;
        var expectProject = await _projectDao.CreateAsync(expectWorkspace, "Test project");
        var initialEntry = await _timeEntryDao.StartNewAsync(
            _user,
            expectWorkspace,
            DateTime.Now, 
            TimeSpan.FromSeconds(1),
            fakeTimeEntry.IsBillable,
            fakeTimeEntry.Description,
            expectProject.Id,
            internalTask: task
        );

        expectedDto.Id = initialEntry.Id;
        var actualEntry = await _timeEntryDao.SetAsync(_user, expectWorkspace, expectedDto, expectProject);
        Assert.Equal(task.TaskList.Project.Id, actualEntry.Project.Id);
    }
}
