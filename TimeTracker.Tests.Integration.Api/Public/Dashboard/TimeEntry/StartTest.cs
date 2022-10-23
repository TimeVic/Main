using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.TimeEntry;

public class AddTest: BaseTest
{
    private readonly string Url = "/dashboard/time-entry/start";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<TimeEntryEntity> _timeEntryFactory;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly IProjectDao _projectDao;

    public AddTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _timeEntryFactory = ServiceProvider.GetRequiredService<IDataFactory<TimeEntryEntity>>();
        (_jwtToken, _user) = UserSeeder.CreateAuthorizedAsync().Result;
        _defaultWorkspace = _user.Workspaces.First();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new StartRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Date = DateTime.Now.Date,
            StartTime = TimeSpan.FromSeconds(1)
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldStartEmpty()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new StartRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Date = DateTime.Now.Date,
            StartTime = TimeSpan.FromSeconds(1)
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.True(actualDto.Id > 0);
        Assert.Null(actualDto.Description);
        Assert.True(actualDto.StartTime < DateTime.UtcNow.TimeOfDay);
        Assert.True(actualDto.Date > DateTime.MinValue);
        Assert.Null(actualDto.EndTime);
        Assert.Null(actualDto.Project);
        Assert.Null(actualDto.HourlyRate);
    }
    
    [Fact]
    public async Task ShouldNotStart2ItemsIfRequestIsAsync()
    {
        await PostRequestAsync(Url, _jwtToken, new StartRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Date = DateTime.Now.Date,
            StartTime = TimeSpan.FromSeconds(1)
        });
        var response = await PostRequestAsync(Url, _jwtToken, new StartRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Date = DateTime.Now.Date,
            StartTime = TimeSpan.FromSeconds(1)
        });
        response.EnsureSuccessStatusCode();

        var activeRecordsCount = await DbSessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(item => item.EndTime == null && item.Workspace.Id == _defaultWorkspace.Id)
            .CountAsync();
        Assert.Equal(1, activeRecordsCount);
    }
    
    [Fact]
    public async Task ShouldStartFilled()
    {
        var fakeTimeEntry = _timeEntryFactory.Generate();
        var project = await _projectDao.CreateAsync(_defaultWorkspace, "Test project");
        await CommitDbChanges();
        var response = await PostRequestAsync(Url, _jwtToken, new StartRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            ProjectId = project.Id,
            Description = fakeTimeEntry.Description,
            IsBillable = fakeTimeEntry.IsBillable,
            Date = DateTime.Now.Date,
            StartTime = TimeSpan.FromSeconds(1)
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.True(actualDto.Id > 0);
        Assert.Equal(fakeTimeEntry.Description, actualDto.Description);
        Assert.Equal(project.Id, actualDto.Project.Id);
        Assert.Equal(fakeTimeEntry.IsBillable, actualDto.IsBillable);
        Assert.Null(actualDto.EndTime);
    }
}
