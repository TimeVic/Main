using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;
using TimeTracker.Business.Testing.Seeders.Entity;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry;

public partial class StartTest: BaseTest
{
    private readonly string Url = "/dashboard/time-entry/start";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<TimeEntryEntity> _timeEntryFactory;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ITaskSeeder _taskSeeder;

    public StartTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _timeEntryFactory = ServiceProvider.GetRequiredService<IDataFactory<TimeEntryEntity>>();
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new StartRequest()
        {
            StartTime = DateTime.UtcNow.AddSeconds(1)
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldStartEmpty()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new StartRequest()
        {
            StartTime = DateTime.UtcNow
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.NotEqual(Guid.Empty, actualDto.Id);
        Assert.Null(actualDto.Description);
        Assert.True(actualDto.StartTime < DateTime.UtcNow);
        Assert.Null(actualDto.EndTime);
        Assert.Null(actualDto.Project);
        Assert.Null(actualDto.HourlyRate);
    }
    
    [Fact]
    public async Task ShouldNotStart2ItemsIfRequestIsAsync()
    {
        await PostRequestAsync(Url, _jwtToken, new StartRequest()
        {
            StartTime = DateTime.UtcNow.AddSeconds(1)
        });
        await _timeEntryDao.StopActiveAsync(_defaultWorkspace, _user, DateTime.UtcNow.AddHours(1));
        var response = await PostRequestAsync(Url, _jwtToken, new StartRequest()
        {
            StartTime = DateTime.UtcNow.AddSeconds(1)
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var activeRecordsCount = await DbSessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(item => item.EndTime == null && item.Workspace.Id == _defaultWorkspace.Id)
            .CountAsync();
        Assert.Equal(1, activeRecordsCount);
    }

    [Fact]
    public async Task ShouldStopCurrentEntryAndReturnNewActiveEntry()
    {
        var currentEntry = await _timeEntryDao.StartNewAsync(
            _user,
            _defaultWorkspace,
            DateTime.UtcNow.AddMinutes(-1)
        );
        var newStartTime = DateTime.UtcNow;

        var response = await PostRequestAsync(Url, _jwtToken, new StartRequest
        {
            Description = "New active entry",
            StartTime = newStartTime
        });
        response.EnsureSuccessStatusCode();

        var activeEntry = await response.GetJsonDataAsync<TimeEntryDto>();
        await FlushAndRefreshEntity(currentEntry);

        Assert.NotEqual(currentEntry.Id, activeEntry.Id);
        Assert.Equal("New active entry", activeEntry.Description);
        Assert.Null(activeEntry.EndTime);
        Assert.True((currentEntry.EndTime!.Value - newStartTime).Duration() < TimeSpan.FromMicroseconds(1));
    }

    [Fact]
    public async Task ShouldStartFilled()
    {
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var project = await _projectSeeder.CreateAsync(_defaultWorkspace);
        await FlushDbChanges();
        var response = await PostRequestAsync(Url, _jwtToken, new StartRequest()
        {
            ProjectId = project.Id,
            Description = fakeTimeEntry.Description,
            IsBillable = fakeTimeEntry.IsBillable,
            StartTime = DateTime.UtcNow.AddSeconds(1)
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.NotEqual(Guid.Empty, actualDto.Id);
        Assert.Equal(fakeTimeEntry.Description, actualDto.Description);
        Assert.NotNull(actualDto.Project);
        Assert.Equal(project.Id, actualDto.Project.Id);
        Assert.Equal(fakeTimeEntry.IsBillable, actualDto.IsBillable);
        Assert.Null(actualDto.EndTime);
    }
    
    [Fact]
    public async Task StartTimeShouldBeSameAsLocal()
    {
        var expectedStartTime = DateTime.UtcNow.AddMinutes(13);
        var response = await PostRequestAsync(Url, _jwtToken, new StartRequest()
        {
            StartTime = expectedStartTime
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.NotEqual(Guid.Empty, actualDto.Id);
        Assert.Equal(expectedStartTime, actualDto.StartTime);
    }
    
    [Fact]
    public async Task ShouldSetIsBillableIfNull()
    {
        var expectedHourlyRate = 14.3m;
        
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var project = await _projectSeeder.CreateAsync(_defaultWorkspace);
        project.IsBillableByDefault = true;
        project.DefaultHourlyRate = expectedHourlyRate;

        var response = await PostRequestAsync(Url, _jwtToken, new StartRequest()
        {
            ProjectId = project.Id,
            Description = fakeTimeEntry.Description,
            StartTime = DateTime.UtcNow.AddSeconds(1),
            
            IsBillable = null,
            HourlyRate = null
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.Equal(true, actualDto.IsBillable);
        Assert.Equal(expectedHourlyRate, actualDto.HourlyRate);
    }
    
    [Fact]
    public async Task ShouldSetDefaultHourlyRateIfNull()
    {
        var expectedHourlyRate = 14.3m;
        
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var project = await _projectSeeder.CreateAsync(_defaultWorkspace);
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
