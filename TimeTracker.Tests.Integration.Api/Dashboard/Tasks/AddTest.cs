using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tasks;

public class AddTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/add";
    
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

    public AddTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _taskFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectDao.CreateAsync(_workspace, "Test adding").Result;
        _taskList = _taskListSeeder.CreateAsync(_project).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var task = _taskFactory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new AddRequest()
        {
            TaskListId = _taskList.Id,
            Title = task.Title,
            Description = task.Description,
            NotificationTime = task.NotificationTime
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldAdd()
    {
        var task = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            TaskListId = _taskList.Id,
            Title = task.Title,
            Description = task.Description,
            NotificationTime = task.NotificationTime,
            IsDone = task.IsDone,
            IsArchived = task.IsArchived,
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskDto>();
        Assert.True(actualData.Id > 0);
        Assert.Equal(_taskList.Id, actualData.TaskList.Id);
        Assert.Equal(task.Title, actualData.Title);
        Assert.Equal(task.Description, actualData.Description);
        Assert.Equal(task.IsDone, actualData.IsDone);
        Assert.Equal(task.IsArchived, actualData.IsArchived);
    }
    
    [Fact]
    public async Task ShouldNotAddIfIncorrectWorkspaceId()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var otherProject = _projectDao.CreateAsync(otherWorkspace, "Test adding").Result;
        var otherTaskList = _taskListSeeder.CreateAsync(otherProject).Result;
        
        var task = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            TaskListId = otherTaskList.Id,
            Title = task.Title,
            Description = task.Description,
            NotificationTime = task.NotificationTime
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
    
    [Fact]
    public async Task ShouldCreateHistoryItem()
    {
        var task = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            TaskListId = _taskList.Id,
            Title = task.Title,
            Description = task.Description,
            NotificationTime = task.NotificationTime,
            IsDone = task.IsDone,
            IsArchived = task.IsArchived,
        });
        response.EnsureSuccessStatusCode();
        
        var actualData = await response.GetJsonDataAsync<TaskDto>();
        var actualTask = await DbSessionProvider.CurrentSession.GetAsync<TaskEntity>(actualData.Id);
        Assert.Single(actualTask.HistoryItems);
        var historyItem = actualTask.HistoryItems.First();
        Assert.Equal(task.Title, historyItem.Title);
        Assert.Equal(task.Description, historyItem.Description);
        Assert.Equal(task.NotificationTime.ToString(), historyItem.NotificationTime.ToString());
        Assert.Equal(task.IsDone, historyItem.IsDone);
        Assert.Equal(task.IsArchived, historyItem.IsArchived);
        Assert.True(historyItem.IsNewTask);
    }
}
