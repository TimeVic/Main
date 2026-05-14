using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;
using GetListRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List.GetListRequest;
using GetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List.GetListResponse;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Tasks.List;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/list/get-list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IDataFactory<TaskListEntity> _taskListFactory;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ProjectEntity _project;
    private readonly ITaskListDao _taskListDao;
    private readonly ITaskDao _taskDao;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskListFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskListEntity>>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _taskListDao = ServiceProvider.GetRequiredService<ITaskListDao>();
        _taskDao = ServiceProvider.GetRequiredService<ITaskDao>();
        
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectSeeder.CreateAsync(_defaultWorkspace).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 15;
        await _taskListSeeder.CreateSeveralAsync(_project, expectedCounter);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
        
        Assert.All(actualDto.Items, item =>
        {
            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.NotEmpty(item.Name);
            Assert.NotNull(item.Project);
            Assert.Equal(_project.Id, item.Project.Id);
        });
    }

    [Fact]
    public async Task ShouldReceiveTasksCountForEachTaskList()
    {
        var firstTaskList = await _taskListSeeder.CreateAsync(_project);
        var secondTaskList = await _taskListSeeder.CreateAsync(_project);
        var emptyTaskList = await _taskListSeeder.CreateAsync(_project);
        await _taskSeeder.CreateSeveralAsync(firstTaskList, 3, _user);
        await _taskSeeder.CreateSeveralAsync(secondTaskList, 2, _user);
        await _taskDao.AddTaskAsync(secondTaskList, _user, "Archived task", isArchived: true);

        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(3, actualDto.Items.Single(item => item.Id == firstTaskList.Id).TasksCount);
        Assert.Equal(2, actualDto.Items.Single(item => item.Id == secondTaskList.Id).TasksCount);
        Assert.Equal(0, actualDto.Items.Single(item => item.Id == emptyTaskList.Id).TasksCount);
    }
    
    [Fact]
    public async Task ShouldNotArchivedTasksLists()
    {
        var expectedCounter = 7;
        var taskLists = await _taskListSeeder.CreateSeveralAsync(_project, expectedCounter + 3);
        foreach (var taskList in taskLists.Skip(expectedCounter))
        {
            await _taskListDao.ArchiveTaskListAsync(taskList);
        }
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
    }

    [Fact]
    public async Task ShouldReceiveOnlyTaskListsFromSharedProjectsIfUserHasUserRole()
    {
        var projects = (await _projectSeeder.CreateSeveralAsync(_defaultWorkspace, 3)).ToList();
        var sharedProject1 = projects.First();
        var sharedProject2 = projects.Last();
        var unavailableProject = projects.Skip(1).First();

        var expectedTaskLists = new List<TaskListEntity>();
        expectedTaskLists.AddRange(await _taskListSeeder.CreateSeveralAsync(sharedProject1, 2));
        expectedTaskLists.AddRange(await _taskListSeeder.CreateSeveralAsync(sharedProject2, 3));
        await _taskListSeeder.CreateSeveralAsync(unavailableProject, 4);

        var (otherJwtToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _defaultWorkspace,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
            {
                new () { Project = sharedProject1 },
                new () { Project = sharedProject2 }
            }
        );
        
        var response = await PostRequestAsync(Url, otherJwtToken, new GetListRequest()
        {
        }, _defaultWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedTaskLists.Count, actualDto.TotalCount);
        Assert.Equal(
            expectedTaskLists.Select(item => item.Id).OrderBy(item => item),
            actualDto.Items.Select(item => item.Id).OrderBy(item => item)
        );
        Assert.All(actualDto.Items, item =>
        {
            Assert.NotEqual(unavailableProject.Id, item.Project.Id);
        });
    }

    [Fact]
    public async Task ShouldReceiveEmptyListIfUserHasNoSharedProjects()
    {
        await _taskListSeeder.CreateSeveralAsync(_project, 3);
        var (otherJwtToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _defaultWorkspace,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
        );
        
        var response = await PostRequestAsync(Url, otherJwtToken, new GetListRequest()
        {
        }, _defaultWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Empty(actualDto.Items);
        Assert.Equal(0, actualDto.TotalCount);
    }

    [Fact]
    public async Task ShouldReceiveAllTaskListsIfUserHasManagerRole()
    {
        var expectedCounter = 6;
        await _taskListSeeder.CreateSeveralAsync(_project, expectedCounter);
        var (managerJwtToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _defaultWorkspace,
            MembershipAccessType.Manager
        );
        
        var response = await PostRequestAsync(Url, managerJwtToken, new GetListRequest()
        {
        }, _defaultWorkspace.Id);
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
        Assert.All(actualDto.Items, item =>
        {
            Assert.Equal(_project.Id, item.Project.Id);
        });
    }
}
