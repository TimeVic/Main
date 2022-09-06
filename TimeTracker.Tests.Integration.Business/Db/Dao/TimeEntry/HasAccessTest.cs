using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public class HasAccessTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IDataFactory<TimeEntryEntity> _timeEntryFactory;
    private readonly IProjectDao _projectDao;

    public HasAccessTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _timeEntryFactory = Scope.Resolve<IDataFactory<TimeEntryEntity>>();
    }

    [Fact]
    public async Task ShouldReturnTrueIfHasAccess()
    {
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var expectedDto = new TimeEntryCreationDto()
        {
            EndTime = fakeTimeEntry.EndTime.Value,
            StartTime = fakeTimeEntry.StartTime,
        };
        
        var user = await _userSeeder.CreateActivatedAsync();
        var expectWorkspace = user.Workspaces.First();
        var expectProject = await _projectDao.Create(expectWorkspace, "Test project");
        
        var expectedEntry = await _timeEntryDao.SetAsync(expectWorkspace, expectedDto, expectProject);
        
        var hasAccess = await _timeEntryDao.HasAccessAsync(user, expectedEntry);
        Assert.True(hasAccess);
    }
    
    [Fact]
    public async Task ShouldReturnTruIfEntryFromAnotherWorkspace()
    {
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var expectedDto = new TimeEntryCreationDto()
        {
            EndTime = fakeTimeEntry.EndTime.Value,
            StartTime = fakeTimeEntry.StartTime,
        };
        
        var user = await _userSeeder.CreateActivatedAsync();
        var expectWorkspace = await _workspaceDao.CreateWorkspace(user, "New");
        
        var expectedEntry = await _timeEntryDao.SetAsync(expectWorkspace, expectedDto);
        
        var hasAccess = await _timeEntryDao.HasAccessAsync(user, expectedEntry);
        Assert.True(hasAccess);
    }
    
    [Fact]
    public async Task ShouldReturnFalseIfNotOwner()
    {
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var expectedDto = new TimeEntryCreationDto()
        {
            EndTime = fakeTimeEntry.EndTime.Value,
            StartTime = fakeTimeEntry.StartTime,
        };
        
        var ownedUser = await _userSeeder.CreateActivatedAsync();
        var expectWorkspace = await _workspaceDao.CreateWorkspace(ownedUser, "New");
        
        var expectedEntry = await _timeEntryDao.SetAsync(expectWorkspace, expectedDto);
        
        var anotherUser = await _userSeeder.CreateActivatedAsync();
        var hasAccess = await _timeEntryDao.HasAccessAsync(anotherUser, expectedEntry);
        Assert.False(hasAccess);
    }
}
