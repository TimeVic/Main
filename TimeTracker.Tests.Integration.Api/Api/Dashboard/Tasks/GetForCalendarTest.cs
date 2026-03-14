using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Storage;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Tasks;

public class GetForCalendarTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/get-for-calendar";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly TaskListEntity _taskList;
    
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IDataFactory<TaskListEntity> _taskListFactory;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly ProjectEntity _project;
    private readonly ITaskSeeder _taskSeeder;
    private readonly IFileStorage _fileStorage;
    private readonly ITagSeeder _tagSeeder;

    public GetForCalendarTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskListFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskListEntity>>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _tagSeeder = ServiceProvider.GetRequiredService<ITagSeeder>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _fileStorage = ServiceProvider.GetRequiredService<IFileStorage>();
        
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectSeeder.CreateAsync(_workspace).Result;
        _taskList = _taskListSeeder.CreateAsync(_project).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetForCalendarRequest()
        {
            WorkspaceId = _workspace.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedStartTime = DateTime.UtcNow.AddHours(-1);
        var expectedEndTime = expectedStartTime.AddHours(1);
        var expectedCounter = 8;
        var tasks = await _taskSeeder.CreateSeveralAsync(_taskList, expectedCounter, user: _user);
        foreach (var task in tasks)
        {
            task.StartTime = expectedStartTime;
            task.EndTime = expectedEndTime;
        }

        var response = await PostRequestAsync(Url, _jwtToken, new GetForCalendarRequest()
        {
            WorkspaceId = _workspace.Id,
            StartTime = expectedStartTime.AddMinutes(-1),
            EndTime = expectedEndTime.AddMinutes(1)
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
    }

    [Fact]
    public async Task ShouldNotReceiveIfStartAndEndTimeIsNotFromInterval()
    {
        var expectedStartTime = DateTime.UtcNow.AddHours(-1);
        var expectedEndTime = expectedStartTime.AddHours(1);
        var expectedCounter = 8;
        var tasks = await _taskSeeder.CreateSeveralAsync(_taskList, expectedCounter, user: _user);
        foreach (var task in tasks)
        {
            task.StartTime = expectedStartTime;
            task.EndTime = expectedEndTime;
        }
        var otherTasks = await _taskSeeder.CreateSeveralAsync(_taskList, expectedCounter, user: _user);
        foreach (var task in otherTasks)
        {
            task.StartTime = expectedStartTime.AddDays(-1);
            task.EndTime = expectedEndTime.AddDays(-1);
        }

        var response = await PostRequestAsync(Url, _jwtToken, new GetForCalendarRequest()
        {
            WorkspaceId = _workspace.Id,
            StartTime = expectedStartTime.AddMinutes(-1),
            EndTime = expectedEndTime.AddMinutes(1)
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
    }
}
