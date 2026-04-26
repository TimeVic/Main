using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Tasks;

public partial class UpdateTest: BaseTest
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
    private readonly ITagSeeder _tagSeeder;
    private readonly ITaskDao _taskDao;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _taskFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _tagSeeder = ServiceProvider.GetRequiredService<ITagSeeder>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        _taskDao = ServiceProvider.GetRequiredService<ITaskDao>();
        
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
            StartTime = task.StartTime,
            EndTime = task.EndTime,
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
            OriginalEstimate = expectedTask.OriginalEstimate,
            StartTime = expectedTask.StartTime,
            EndTime = expectedTask.EndTime,
            Status = expectedTask.Status,
            Priority = expectedTask.Priority,
            IsArchived = expectedTask.IsArchived,
            UserId = _user.Id,
            ExternalTaskId = expectedTask.ExternalTaskId,
            ReminderTime = expectedTask.ReminderTime
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskDto>();
        Assert.Equal(_task.TaskId, actualData.TaskId);
        Assert.Equal(_otherTaskList.Id, actualData.TaskList.Id);
        Assert.Equal(expectedTask.Title, actualData.Title);
        Assert.Equal(expectedTask.Description, actualData.Description);
        Assert.Equal(expectedTask.Status, actualData.Status);
        Assert.Equal(expectedTask.Priority, actualData.Priority);
        Assert.Equal(expectedTask.IsArchived, actualData.IsArchived);
        Assert.Equal(expectedTask.ExternalTaskId, actualData.ExternalTaskId);
        Assert.Equal(expectedTask.OriginalEstimate, actualData.OriginalEstimate);
        Assert.Equal(expectedTask.ReminderTime!.Value.ToShortTimeString(), actualData.ReminderTime!.Value.ToUniversalTime().ToShortTimeString());
        Assert.Equal(expectedTask.ReminderTime.Value.ToLongDateString(), actualData.ReminderTime.Value.ToUniversalTime().ToLongDateString());
    }

    [Fact]
    public async Task ShouldKeepExternalSourceTypeOnUpdate()
    {
        _task.ExternalSourceType = ExternalSourceType.Jira;
        _task.ExternalTaskId = "JIRA-123";
        await DbSessionProvider.CurrentSession.SaveOrUpdateAsync(_task);
        await FlushDbChanges(true);

        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            TaskId = _task.Id,
            TaskListId = _taskList.Id,
            Title = $"{_task.Title} updated",
            Description = _task.Description,
            OriginalEstimate = TimeSpan.FromHours(6),
            StartTime = _task.StartTime,
            EndTime = _task.EndTime,
            Status = _task.Status,
            Priority = _task.Priority,
            IsArchived = _task.IsArchived,
            UserId = _user.Id,
            ReminderTime = _task.ReminderTime
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskDto>();
        Assert.Equal(ExternalSourceType.Jira, actualData.ExternalSourceType);

        await FlushDbChanges(true);
        var actualTask = await _taskDao.GetById(_task.Id);
        Assert.NotNull(actualTask);
        Assert.Equal(ExternalSourceType.Jira, actualTask.ExternalSourceType);
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
            StartTime = newTask.StartTime,
            EndTime = newTask.EndTime,
            Status = newTask.Status,
            IsArchived = newTask.IsArchived,
            UserId = _user.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
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
            StartTime = newTask.StartTime,
            EndTime = newTask.EndTime,
            Status = newTask.Status,
            IsArchived = newTask.IsArchived,
            UserId = user2.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }
    
    [Fact]
    public async Task ShouldNotSetUserIdFromWhichDoesNotHaveAccessToProject()
    {
        var user2 = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            user2,
            MembershipAccessType.User,
            new List<ProjectAccessModel>
            {
                new() { Project = _project }
            }
        );
        
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
            StartTime = newTask.StartTime,
            EndTime = newTask.EndTime,
            Status = newTask.Status,
            IsArchived = newTask.IsArchived,
            UserId = user2.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
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
            StartTime = newTask.StartTime,
            EndTime = newTask.EndTime,
            Status = newTask.Status,
            IsArchived = newTask.IsArchived,
            UserId = user2.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
        Assert.Contains("for provided task list", error.Message);
    }
    
    [Fact]
    public async Task ShouldCreateHistoryItemUpdate()
    {
        var newTags = await _tagSeeder.CreateSeveralAsync(_workspace, 2);
        
        var expectedTask = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            TaskId = _task.Id,
            TaskListId = _otherTaskList.Id,
            Title = expectedTask.Title,
            Description = expectedTask.Description,
            StartTime = expectedTask.StartTime,
            EndTime = expectedTask.EndTime,
            Status = expectedTask.Status,
            IsArchived = expectedTask.IsArchived,
            UserId = _user.Id,
            ExternalTaskId = expectedTask.ExternalTaskId,
            TagIds = newTags.Select(item => item.Id).ToList()
        });
        response.EnsureSuccessStatusCode();

        await FlushDbChanges(true);
        var task = await _taskDao.GetById(_task.Id);
        Assert.NotNull(task);
        var historyItems = task.HistoryItems.ToList();
        Assert.Equal(2, historyItems.Count);
        var historyItem = historyItems.OrderByDescending(item => item.CreatedAt).FirstOrDefault();
        Assert.NotNull(historyItem);
        Assert.Equal(expectedTask.Title, historyItem.Title);
        Assert.Equal(expectedTask.Description, historyItem.Description);
        Assert.Equal(expectedTask.StartTime.ToString(), historyItem.StartTime.ToString());
        Assert.Equal(expectedTask.EndTime.ToString(), historyItem.EndTime.ToString());
        Assert.Equal(expectedTask.Status, historyItem.Status);
        Assert.Equal(expectedTask.IsArchived, historyItem.IsArchived);
        Assert.NotEmpty(historyItem.Tags ?? "");
        Assert.False(historyItem.IsNewTask);
    }
    
    [Fact]
    public async Task ShouldUpdateWithoutEndAndStartTime()
    {
        var expectedTask = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            TaskId = _task.Id,
            TaskListId = _otherTaskList.Id,
            Title = expectedTask.Title,
            Description = expectedTask.Description,
            StartTime = null,
            EndTime = null,
            Status = expectedTask.Status,
            Priority = expectedTask.Priority,
            IsArchived = expectedTask.IsArchived,
            UserId = _user.Id,
            ExternalTaskId = expectedTask.ExternalTaskId
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskFullDto>();
        Assert.Equal(_task.TaskId, actualData.TaskId);
        Assert.Equal(_otherTaskList.Id, actualData.TaskList.Id);
        Assert.Null(actualData.StartTime);
        Assert.Null(actualData.EndTime);
    }
    
    [Fact]
    public async Task ShouldUpdateWithoutReminderTime()
    {
        var expectedTask = _taskFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            TaskId = _task.Id,
            TaskListId = _otherTaskList.Id,
            Title = expectedTask.Title,
            Description = expectedTask.Description,
            Status = expectedTask.Status,
            Priority = expectedTask.Priority,
            IsArchived = expectedTask.IsArchived,
            UserId = _user.Id,
            ExternalTaskId = expectedTask.ExternalTaskId,
            ReminderTime = null
        });
        response.EnsureSuccessStatusCode();

        var actualData = await response.GetJsonDataAsync<TaskFullDto>();
        Assert.Equal(_task.TaskId, actualData.TaskId);
        Assert.Equal(_otherTaskList.Id, actualData.TaskList.Id);
        Assert.Null(actualData.ReminderTime);
    }
}
