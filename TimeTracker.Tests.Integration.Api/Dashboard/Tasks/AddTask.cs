using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tasks;

public partial class AddTask: BaseTest
{
    private readonly string Url = "/dashboard/tasks/add";
    
    private WorkspaceEntity _workspace;
    private readonly TaskListEntity _taskList;
    private readonly ProjectEntity _project;
    private readonly string? _clickUpTaskId;
    private readonly string? _jiraTaskId;
    private readonly string _jwtToken;
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<TaskEntity> _taskFactory;
    private readonly IProjectDao _projectDao;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly ITaskDao _taskDao;

    public AddTask(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _taskFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        _taskDao = ServiceProvider.GetRequiredService<ITaskDao>();
        
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectDao.CreateAsync(_workspace, "Test adding").Result;
        _taskList = _taskListSeeder.CreateAsync(_project).Result;
        
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        _clickUpTaskId = configuration.GetValue<string>("Integration:ClickUp:TaskId");
        _jiraTaskId = "SP-3341";
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
            StartTime = task.StartTime,
            EndTime = task.EndTime,
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
            StartTime = task.StartTime,
            EndTime = task.EndTime,
            Status = task.Status,
            IsArchived = task.IsArchived,
            Priority = task.Priority
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskFullDto>();
        Assert.True(actualData.TaskId > 0);
        Assert.Equal(_taskList.Id, actualData.TaskList.Id);
        Assert.Equal(task.Title, actualData.Title);
        Assert.Equal(task.Description, actualData.Description);
        Assert.Equal(task.Status, actualData.Status);
        Assert.Equal(task.Priority, actualData.Priority);
        Assert.Equal(task.IsArchived, actualData.IsArchived);
        Assert.Equal(task.EndTime.Value.ToLongTimeString(), actualData.EndTime.Value.ToUniversalTime().ToLongTimeString());
        Assert.Equal(task.StartTime.Value.ToLongTimeString(), actualData.StartTime.Value.ToUniversalTime().ToLongTimeString());
    }
    
    [Fact]
    public async Task ShouldAddWithAttachedTimeEntry()
    {
        var timeEntry = await _timeEntrySeeder.CreateAsync(_workspace, _user);
        
        var task = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            TaskListId = _taskList.Id,
            Title = task.Title,
            TimeEntryId = timeEntry.Id
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskFullDto>();
        Assert.True(actualData.TaskId > 0);
        Assert.Equal(_taskList.Id, actualData.TaskList.Id);
        Assert.Equal(task.Title, actualData.Title);
        
        var actualTimeEntry = await DbSessionProvider.CurrentSession.GetAsync<TimeEntryEntity>(timeEntry.Id);
        Assert.NotNull(actualTimeEntry.Task);
        Assert.Equal(actualData.TaskId, actualTimeEntry.Task.TaskId);
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
            StartTime = task.StartTime,
            EndTime = task.EndTime,
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
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
            StartTime = task.StartTime,
            EndTime = task.EndTime,
            Status = task.Status,
            IsArchived = task.IsArchived,
        });
        response.EnsureSuccessStatusCode();
        
        var actualData = await response.GetJsonDataAsync<TaskFullDto>();
        var actualTask = await _taskDao.GetByWorkspaceTaskId(_workspace.Id, actualData.TaskId);
        Assert.NotNull(actualTask);
        Assert.Single(actualTask.HistoryItems);
        var historyItem = actualTask.HistoryItems.First();
        Assert.Equal(task.Title, historyItem.Title);
        Assert.Equal(task.Description, historyItem.Description);
        Assert.Equal(task.StartTime.ToString(), historyItem.StartTime.ToString());
        Assert.Equal(task.EndTime.ToString(), historyItem.EndTime.ToString());
        Assert.Equal(task.Status, historyItem.Status);
        Assert.Equal(task.IsArchived, historyItem.IsArchived);
        Assert.True(historyItem.IsNewTask);
    }
    
    [Fact]
    public async Task ShouldSetTimeEntriesProjectIfWasNotProvided()
    {
        var timeEntry = await _timeEntrySeeder.CreateAsync(_workspace, _user, project: null);
        
        var task = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            TaskListId = _taskList.Id,
            Title = task.Title,
            TimeEntryId = timeEntry.Id
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskFullDto>();
        Assert.True(actualData.TaskId > 0);
        Assert.Equal(_taskList.Id, actualData.TaskList.Id);
        Assert.Equal(task.Title, actualData.Title);
        
        var actualTimeEntry = await DbSessionProvider.CurrentSession.GetAsync<TimeEntryEntity>(timeEntry.Id);
        Assert.NotNull(actualTimeEntry.Project);
        Assert.Equal(_taskList.Project.Id, actualTimeEntry.Project.Id);
    }
}
