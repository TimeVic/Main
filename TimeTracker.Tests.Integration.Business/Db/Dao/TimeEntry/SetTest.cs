using Autofac;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Exceptions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.TimeEntry;

public class SetTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;
    private readonly IDataFactory<TimeEntryEntity> _timeEntryFactory;
    private readonly IProjectDao _projectDao;

    public SetTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _projectDao = Scope.Resolve<IProjectDao>();
        _workspaceDao = Scope.Resolve<IWorkspaceDao>();
        _timeEntryFactory = Scope.Resolve<IDataFactory<TimeEntryEntity>>();
    }

    [Fact]
    public async Task ShouldSetNew()
    {
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var expectedDto = new TimeEntryCreationDto()
        {
            Description = fakeTimeEntry.Description,
            EndTime = fakeTimeEntry.EndTime.Value,
            StartTime = fakeTimeEntry.StartTime,
            HourlyRate = fakeTimeEntry.HourlyRate,
            IsBillable = fakeTimeEntry.IsBillable
        };
        
        var user = await _userSeeder.CreateActivatedAsync();
        var expectWorkspace = user.Workspaces.First();
        var expectProject = await _projectDao.CreateAsync(expectWorkspace, "Test project");
        
        var newEntry = await _timeEntryDao.SetAsync(user, expectWorkspace, expectedDto, expectProject);
        Assert.True(newEntry.Id > 0);
        Assert.Equal(expectWorkspace.Id, newEntry.Workspace.Id);
        Assert.Equal(expectProject.Id, newEntry.Project.Id);
        Assert.Equal(expectedDto.Description, newEntry.Description);
        Assert.Equal(expectedDto.EndTime, newEntry.EndTime);
        Assert.Equal(expectedDto.StartTime, newEntry.StartTime);
        Assert.Equal(expectedDto.HourlyRate, newEntry.HourlyRate);
        Assert.Equal(expectedDto.IsBillable, newEntry.IsBillable);
    }
    
    [Fact]
    public async Task ShouldSetExists()
    {
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var initialDto = new TimeEntryCreationDto()
        {
            Description = fakeTimeEntry.Description,
            EndTime = fakeTimeEntry.EndTime.Value,
            StartTime = fakeTimeEntry.StartTime,
            HourlyRate = fakeTimeEntry.HourlyRate,
            IsBillable = fakeTimeEntry.IsBillable
        };
        
        var user = await _userSeeder.CreateActivatedAsync();
        var initialWorkspace = user.Workspaces.First();
        var initialProject = await _projectDao.CreateAsync(initialWorkspace, "Test project1");
        
        var initialEntry = await _timeEntryDao.SetAsync(user, initialWorkspace, initialDto, initialProject);
        
        var fakeTimeEntry2 = _timeEntryFactory.Generate();
        var expectedDto = new TimeEntryCreationDto()
        {
            Id = initialEntry.Id,
            Description = fakeTimeEntry2.Description,
            EndTime = fakeTimeEntry2.EndTime.Value,
            StartTime = fakeTimeEntry2.StartTime,
            HourlyRate = fakeTimeEntry2.HourlyRate,
            IsBillable = fakeTimeEntry2.IsBillable
        };
        var expectedProject = await _projectDao.CreateAsync(initialWorkspace, "Test project2");
        var actualEntry = await _timeEntryDao.SetAsync(user, initialWorkspace, expectedDto, expectedProject);
        
        Assert.Equal(initialEntry.Id, actualEntry.Id);
        Assert.Equal(initialWorkspace.Id, actualEntry.Workspace.Id);
        Assert.Equal(expectedProject.Id, actualEntry.Project.Id);
        Assert.Equal(expectedDto.Description, actualEntry.Description);
        Assert.Equal(expectedDto.EndTime, actualEntry.EndTime);
        Assert.Equal(expectedDto.StartTime, actualEntry.StartTime);
        Assert.Equal(expectedDto.HourlyRate, actualEntry.HourlyRate);
        Assert.Equal(expectedDto.IsBillable, actualEntry.IsBillable);
    }
    
    [Fact]
    public async Task DatesShouldNotBeUpdatedForActiveItem()
    {
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var initialDto = new TimeEntryCreationDto()
        {
            Description = fakeTimeEntry.Description,
            EndTime = fakeTimeEntry.EndTime.Value,
            StartTime = fakeTimeEntry.StartTime,
            HourlyRate = fakeTimeEntry.HourlyRate,
            IsBillable = fakeTimeEntry.IsBillable
        };
        
        var user = await _userSeeder.CreateActivatedAsync();
        var initialWorkspace = user.Workspaces.First();
        var initialProject = await _projectDao.CreateAsync(initialWorkspace, "Test project1");
        
        var initialEntry = await _timeEntryDao.StartNewAsync(
            user,
            initialWorkspace,
            fakeTimeEntry.IsBillable,
            fakeTimeEntry.Description,
            initialProject.Id
        );
        
        var fakeTimeEntry2 = _timeEntryFactory.Generate();
        var expectedDto = new TimeEntryCreationDto()
        {
            Id = initialEntry.Id,
            Description = fakeTimeEntry2.Description,
            EndTime = fakeTimeEntry2.EndTime.Value,
            StartTime = fakeTimeEntry2.StartTime,
            HourlyRate = fakeTimeEntry2.HourlyRate,
            IsBillable = fakeTimeEntry2.IsBillable
        };
        var expectedProject = await _projectDao.CreateAsync(initialWorkspace, "Test project2");
        var actualEntry = await _timeEntryDao.SetAsync(user, initialWorkspace, expectedDto, expectedProject);
        
        Assert.Null(actualEntry.EndTime);
        Assert.Equal(initialEntry.StartTime, actualEntry.StartTime);
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfEndTimeLess()
    {
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var expectedDto = new TimeEntryCreationDto()
        {
            Description = fakeTimeEntry.Description,
            StartTime = TimeSpan.FromMinutes(120),
            EndTime = TimeSpan.FromMinutes(119),
            HourlyRate = fakeTimeEntry.HourlyRate,
            IsBillable = fakeTimeEntry.IsBillable
        };
        
        var user = await _userSeeder.CreateActivatedAsync();
        var expectWorkspace = user.Workspaces.First();

        await Assert.ThrowsAsync<DataInconsistentException>(async () =>
        {
            await _timeEntryDao.SetAsync(user, expectWorkspace, expectedDto);
        });
    }
}
