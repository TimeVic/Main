using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.TimeEntry;

public class GetFilteredListTest: BaseTest
{
    private readonly string Url = "/dashboard/time-entry/filtered-list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IProjectSeeder _projectSeeder;
    private readonly IUserSeeder _userSeeder;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly ITaskSeeder _taskSeeder;

    public GetFilteredListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetFilteredListRequest()
        {
            Page = 1
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedProject = await _projectSeeder.CreateAsync(_defaultWorkspace);
        var expectedEntry = (await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user, 6, expectedProject)).First();
        expectedEntry.Description = "Fake desCript223ion 123";
        expectedEntry.IsBillable = true;
        
        Assert.NotNull(expectedProject.Client);
        var response = await PostRequestAsync(Url, _jwtToken, new GetFilteredListRequest()
        {
            Page = 1,
            Search = "cript223",
            ClientId = expectedProject.Client.Id,
            ProjectId = expectedProject.Id,
            IsBillable = true,
            DateFrom = DateTime.Now.AddDays(-6),
            DateTo = DateTime.Now,
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetFilteredListResponse>();
        Assert.Equal(1, actualDto.TotalCount);
    }

    [Fact]
    public async Task ShouldReceiveListWithSharedAccess()
    {
        var projects = await _projectSeeder.CreateSeveralAsync(_defaultWorkspace, 2);
        var expectedProject = projects.First();
        var expectedProject2 = projects.Last();

        var (otherJwt, otherUser, otherWorkspace) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _defaultWorkspace,
            MembershipAccessType.User,
            projects: new List<ProjectAccessModel>()
            {
                new () { Project = expectedProject }
            }
        );
        await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, otherUser, 3, expectedProject);
        
        await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user, 3, expectedProject);
        await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user, 3, expectedProject2);

        var response = await PostRequestAsync(Url, otherJwt, new GetFilteredListRequest()
        {
            Page = 1
        }, _defaultWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetFilteredListResponse>();
        Assert.Equal(3 + 3, actualDto.TotalCount);
        Assert.All(actualDto.Items, item =>
        {
            Assert.True(item.User.Id == _user.Id || item.User.Id == otherUser.Id);
        });
    }

    [Fact]
    public async Task ShouldReceiveOnlyEntriesForRequestedTask()
    {
        var project = await _projectSeeder.CreateAsync(_defaultWorkspace);
        var taskList = await _taskListSeeder.CreateAsync(project);
        var requestedTask = await _taskSeeder.CreateAsync(taskList, _user);
        var otherTask = await _taskSeeder.CreateAsync(taskList, _user);
        var requestedEntry = await _timeEntryDao.SetAsync(_user, _defaultWorkspace, new TimeEntryCreationDto { StartTime = DateTime.UtcNow.AddHours(-2), EndTime = DateTime.UtcNow.AddHours(-1) }, project);
        requestedEntry.Task = requestedTask;
        var otherEntry = await _timeEntryDao.SetAsync(_user, _defaultWorkspace, new TimeEntryCreationDto { StartTime = DateTime.UtcNow.AddHours(-4), EndTime = DateTime.UtcNow.AddHours(-3) }, project);
        otherEntry.Task = otherTask;
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new GetFilteredListRequest { Page = 1, TaskId = requestedTask.Id });
        response.EnsureSuccessStatusCode();
        var result = await response.GetJsonDataAsync<GetFilteredListResponse>();
        Assert.Single(result.Items);
        Assert.Equal(requestedEntry.Id, result.Items.Single().Id);
    }

    [Fact]
    public async Task ShouldNotReceiveEntriesForSoftDeletedProject()
    {
        var project = await _projectSeeder.CreateAsync(_defaultWorkspace);
        await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user, 1, project);
        project.DeletedAt = DateTime.UtcNow;
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new GetFilteredListRequest { Page = 1 });
        response.EnsureSuccessStatusCode();

        var result = await response.GetJsonDataAsync<GetFilteredListResponse>();
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task ShouldNotAllowReadingEntriesForTaskOutsideCurrentWorkspace()
    {
        var (_, otherUser, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var project = await _projectSeeder.CreateAsync(otherWorkspace);
        var task = await _taskSeeder.CreateAsync(await _taskListSeeder.CreateAsync(project), otherUser);

        var response = await PostRequestAsync(Url, _jwtToken, new GetFilteredListRequest { Page = 1, TaskId = task.Id });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
