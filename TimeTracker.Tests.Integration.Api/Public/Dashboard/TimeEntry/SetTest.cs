using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.TimeEntry;

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

    public SetTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _workspaceDao = ServiceProvider.GetRequiredService<IWorkspaceDao>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _timeEntryFactory = ServiceProvider.GetRequiredService<IDataFactory<TimeEntryEntity>>();
        (_jwtToken, _user) = UserSeeder.CreateAuthorizedAsync().Result;
        _defaultWorkspace = _user.Workspaces.First();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var expectedEntry = await _timeEntryDao.StartNewAsync(_defaultWorkspace);
        
        var response = await PostRequestAsAnonymousAsync(Url, new SetRequest()
        {
            Id = expectedEntry.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldCreateNewTimeEntry()
    {
        var fakeEntry = _timeEntryFactory.Generate();
        var expectedProject = await _projectDao.Create(_defaultWorkspace, "Test");
        await CommitDbChanges();
        // var expectedEntry = await _timeEntryDao.StartNewAsync(_defaultWorkspace);
        
        var response = await PostRequestAsync(Url, _jwtToken, new SetRequest()
        {
            WorkspaceId = _defaultWorkspace.Id,
            Description = fakeEntry.Description,
            EndTime = fakeEntry.EndTime.Value,
            StartTime = fakeEntry.StartTime,
            HourlyRate = fakeEntry.HourlyRate,
            IsBillable = fakeEntry.IsBillable,
            ProjectId = expectedProject.Id
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.True(actualDto.Id > 0);
        Assert.Equal(fakeEntry.EndTime, actualDto.EndTime);
        Assert.Equal(fakeEntry.StartTime, actualDto.StartTime);
        Assert.Equal(fakeEntry.Description, actualDto.Description);
        Assert.Equal(fakeEntry.IsBillable, actualDto.IsBillable);
        Assert.Equal(fakeEntry.HourlyRate, actualDto.HourlyRate);
        Assert.Equal(expectedProject.Id, actualDto.Project.Id);
        
        Assert.False(await _workspaceDao.HasActiveTimeEntriesAsync(_defaultWorkspace));
    }
    
    [Fact]
    public async Task ShouldUpdateActiveEntry()
    {
        var fakeEntry = _timeEntryFactory.Generate();
        var expectedProject = await _projectDao.Create(_defaultWorkspace, "Test");
        await CommitDbChanges();
        var expectedEntry = await _timeEntryDao.StartNewAsync(_defaultWorkspace);
        
        var response = await PostRequestAsync(Url, _jwtToken, new SetRequest()
        {
            Id = expectedEntry.Id,
            WorkspaceId = _defaultWorkspace.Id,
            Description = fakeEntry.Description,
            EndTime = fakeEntry.EndTime.Value,
            StartTime = fakeEntry.StartTime,
            HourlyRate = fakeEntry.HourlyRate,
            IsBillable = fakeEntry.IsBillable,
            ProjectId = expectedProject.Id
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<TimeEntryDto>();
        Assert.True(actualDto.Id > 0);
        Assert.Equal(expectedEntry.EndTime, actualDto.EndTime);
        Assert.Equal(fakeEntry.StartTime, actualDto.StartTime);
        Assert.Equal(fakeEntry.Description, actualDto.Description);
        Assert.Equal(fakeEntry.IsBillable, actualDto.IsBillable);
        Assert.Equal(fakeEntry.HourlyRate, actualDto.HourlyRate);
        Assert.Equal(expectedProject.Id, actualDto.Project.Id);
        
        Assert.True(await _workspaceDao.HasActiveTimeEntriesAsync(_defaultWorkspace));
    }
}
