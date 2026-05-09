using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry;

public class SetTest: BaseTest
{
    private readonly string Url = "/dashboard/time-entry/set";

    private readonly UserEntity _user;
    private readonly IDataFactory<TimeEntryEntity> _timeEntryFactory;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly IProjectDao _projectDao;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IWorkspaceDao _workspaceDao;
    private new readonly IQueueDao _queueDao;
    private readonly IQueueService _queueService;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IUserSeeder _userSeeder;
    private readonly IClientSeeder _clientSeeder;

    public SetTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _workspaceDao = ServiceProvider.GetRequiredService<IWorkspaceDao>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _timeEntryFactory = ServiceProvider.GetRequiredService<IDataFactory<TimeEntryEntity>>();
        _queueDao = ServiceProvider.GetRequiredService<IQueueDao>();
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        _clientSeeder = ServiceProvider.GetRequiredService<IClientSeeder>();
        
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _queueDao.CompleteAllPending().Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var timeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _defaultWorkspace,
            DateTime.UtcNow.AddSeconds(1)
        );
        var response = await PostRequestAsAnonymousAsync(Url, new SetRequest()
        {
            Id = timeEntry.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldCreateNewTimeEntry()
    {
        var fakeEntry = _timeEntryFactory.Generate();
        var expectedProject = await _projectDao.CreateAsync(_defaultWorkspace, "Test");
        await FlushDbChanges();

        var startTime = DateTime.UtcNow.AddSeconds(1);
        var endTime = DateTime.UtcNow.AddHours(1);
        var response = await PostRequestAsync(Url, _jwtToken, new SetRequest()
        {
            Description = fakeEntry.Description,
            EndTime = endTime,
            StartTime = startTime,
            HourlyRate = fakeEntry.HourlyRate,
            IsBillable = fakeEntry.IsBillable,
            ProjectId = expectedProject.Id
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.NotNull(actualDto.Project);
        Assert.NotEqual(Guid.Empty, actualDto.Id);
        Assert.Equal(endTime, actualDto.EndTime);
        Assert.Equal(startTime, actualDto.StartTime);
        Assert.Equal(fakeEntry.Description, actualDto.Description);
        Assert.Equal(fakeEntry.IsBillable, actualDto.IsBillable);
        Assert.Equal(fakeEntry.HourlyRate, actualDto.HourlyRate);
        Assert.Equal(expectedProject.Id, actualDto.Project.Id);
        
        Assert.False(await _workspaceDao.HasActiveTimeEntriesAsync(_defaultWorkspace));

        var processedCounter = await QueueProcess(QueueChannel.ExternalClient);
        Assert.True(processedCounter > 0);
    }
    
    [Fact]
    public async Task ShouldUpdateActiveEntry()
    {
        var fakeEntry = _timeEntryFactory.Generate();
        var expectedProject = await _projectDao.CreateAsync(_defaultWorkspace, "Test");
        var client = await _clientSeeder.Create(_defaultWorkspace);
        expectedProject.Client = client;
        
        var timeEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _defaultWorkspace, 
            DateTime.UtcNow.AddSeconds(1)
        );

        var response = await PostRequestAsync(Url, _jwtToken, new SetRequest()
        {
            Id = timeEntry.Id,
            Description = fakeEntry.Description,
            EndTime = null,
            StartTime = fakeEntry.StartTime,
            HourlyRate = fakeEntry.HourlyRate,
            IsBillable = fakeEntry.IsBillable,
            ProjectId = expectedProject.Id
        });
        await response.EnsureSuccessStatusCodeWithoutError();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.NotNull(actualDto.Project);
        Assert.NotEqual(Guid.Empty, actualDto.Id);
        Assert.Null(actualDto.EndTime);
        Assert.Equal(fakeEntry.StartTime, actualDto.StartTime);
        Assert.Equal(fakeEntry.Description, actualDto.Description);
        Assert.Equal(fakeEntry.IsBillable, actualDto.IsBillable);
        Assert.Equal(fakeEntry.HourlyRate, actualDto.HourlyRate);
        Assert.Equal(expectedProject.Id, actualDto.Project.Id);

        var processedCounter = await QueueProcess(QueueChannel.ExternalClient);
        Assert.True(processedCounter > 0);
    }
    
    [Fact]
    public async Task ShouldUpdateWithSharedProject()
    {
        var expectedProject = await _projectDao.CreateAsync(_defaultWorkspace, "Test");
        var (jwtToken, otherUser, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _defaultWorkspace,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
            {
                new () { Project = expectedProject }
            }
        );
        var timeEntry = await _timeEntryDao.StartNewAsync(
            otherUser,
            _defaultWorkspace, 
            DateTime.UtcNow.AddSeconds(1)
        );
        
        var fakeEntry = _timeEntryFactory.Generate();
        
        var startTime = DateTime.UtcNow.AddSeconds(1);
        var response = await PostRequestAsync(Url, jwtToken, new SetRequest()
        {
            Id = timeEntry.Id,
            Description = fakeEntry.Description,
            EndTime = null,
            StartTime = startTime,
            HourlyRate = fakeEntry.HourlyRate,
            IsBillable = fakeEntry.IsBillable,
            ProjectId = expectedProject.Id
        }, _defaultWorkspace.Id);
        await response.EnsureSuccessStatusCodeWithoutError();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.NotEqual(Guid.Empty, actualDto.Id);
        Assert.Null(actualDto.EndTime);
        Assert.NotNull(actualDto.Project);
        Assert.Equal(expectedProject.Id, actualDto.Project.Id);
    }
    
    [Fact]
    public async Task ShouldSetDefaultHourlyRateIfNull()
    {
        var expectedHourlyRate = 14.3m;
        
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var project = await _projectDao.CreateAsync(_defaultWorkspace, "Test project");
        project.DefaultHourlyRate = expectedHourlyRate;

        var response = await PostRequestAsync(Url, _jwtToken, new StartRequest()
        {
            ProjectId = project.Id,
            Description = fakeTimeEntry.Description,
            StartTime = DateTime.UtcNow.AddSeconds(1),
            
            IsBillable = true,
            HourlyRate = null
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.Equal(expectedHourlyRate, actualDto.HourlyRate);
    }
}
