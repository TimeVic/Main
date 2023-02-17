using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tasks;

public class UpdateTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/update";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<TaskEntity> _taskFactory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly IProjectDao _projectDao;
    private readonly ProjectEntity _project;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ITaskListSeeder _taskListSeeder;
    
    private readonly TaskListEntity _taskList;
    private readonly TaskEntity _task;
    private readonly TaskListEntity _otherTaskList;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _taskFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectDao.CreateAsync(_workspace, "Test adding").Result;
        _taskList = _taskListSeeder.CreateAsync(_project).Result;
        _otherTaskList = _taskListSeeder.CreateAsync(_project).Result;
        _task = _taskSeeder.CreateAsync(_taskList).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var task = _taskFactory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            TaskId = task.Id,
            Title = task.Title,
            Description = task.Description,
            NotificationTime = task.NotificationTime
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpdate()
    {
        var expectedTask = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            TaskId = _task.Id,
            TaskListId = _otherTaskList.Id,
            Title = expectedTask.Title,
            Description = expectedTask.Description,
            NotificationTime = expectedTask.NotificationTime,
            IsDone = expectedTask.IsDone,
            IsArchived = expectedTask.IsArchived,
            UserId = _user.Id
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskDto>();
        Assert.Equal(_task.Id, actualData.Id);
        Assert.Equal(_otherTaskList.Id, actualData.TaskList.Id);
        Assert.Equal(expectedTask.Title, actualData.Title);
        Assert.Equal(expectedTask.Description, actualData.Description);
        Assert.Equal(expectedTask.IsDone, actualData.IsDone);
        Assert.Equal(expectedTask.IsArchived, actualData.IsArchived);
    }
    
    [Fact]
    public async Task ShouldNotSetTaskIdFromOtherWorkspace()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var otherProject = _projectDao.CreateAsync(otherWorkspace, "Test adding").Result;
        var otherTaskList = _taskListSeeder.CreateAsync(otherProject).Result;
        
        var newTask = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            TaskId = _task.Id,
            TaskListId = otherTaskList.Id,
            Title = newTask.Title,
            Description = newTask.Description,
            NotificationTime = newTask.NotificationTime,
            IsDone = newTask.IsDone,
            IsArchived = newTask.IsArchived,
            UserId = _user.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new ValidationException().GetTypeName(), error.Type);
    }
    
    [Fact]
    public async Task ShouldNotSetUserIdFromOtherWorkspace()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();

        var newTask = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            TaskId = _task.Id,
            TaskListId = _taskList.Id,
            Title = newTask.Title,
            Description = newTask.Description,
            NotificationTime = newTask.NotificationTime,
            IsDone = newTask.IsDone,
            IsArchived = newTask.IsArchived,
            UserId = user2.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
    
    [Fact]
    public async Task ShouldNotSetUserIdFromWhichDoesNotHaveAccessToProject()
    {
        var user2 = await _userSeeder.CreateActivatedAsync();
        _workspaceAccessService.ShareAccessAsync(
            _workspace,
            user2,
            MembershipAccessType.User,
            new List<ProjectAccessModel>
            {
                new() { Project = _project }
            }
        ).Wait();
        
        var project2 = _projectDao.CreateAsync(_workspace, "Test adding").Result;
        var taskList2 = _taskListSeeder.CreateAsync(project2).Result;
        var task2 = _taskSeeder.CreateAsync(taskList2).Result;
        
        var newTask = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            TaskId = task2.Id,
            TaskListId = _taskList.Id,
            Title = newTask.Title,
            Description = newTask.Description,
            NotificationTime = newTask.NotificationTime,
            IsDone = newTask.IsDone,
            IsArchived = newTask.IsArchived,
            UserId = user2.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
        Assert.Contains("for task", error.Message);
    }
    
    [Fact]
    public async Task ShouldNotSetUserIdFromWhichDoesNotHaveAccessToTaskList()
    {
        var user2 = await _userSeeder.CreateActivatedAsync();
        _workspaceAccessService.ShareAccessAsync(
            _workspace,
            user2,
            MembershipAccessType.User,
            new List<ProjectAccessModel>
            {
                new() { Project = _project }
            }
        ).Wait();
        
        var project2 = _projectDao.CreateAsync(_workspace, "Test adding").Result;
        var taskList2 = _taskListSeeder.CreateAsync(project2).Result;
        var task2 = _taskSeeder.CreateAsync(taskList2).Result;
        
        var newTask = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            TaskId = task2.Id,
            TaskListId = taskList2.Id,
            Title = newTask.Title,
            Description = newTask.Description,
            NotificationTime = newTask.NotificationTime,
            IsDone = newTask.IsDone,
            IsArchived = newTask.IsArchived,
            UserId = user2.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
        Assert.Contains("for provided task list", error.Message);
    }
}
