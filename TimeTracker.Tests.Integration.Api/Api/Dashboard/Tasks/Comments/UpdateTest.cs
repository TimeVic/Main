using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Notification.Center;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Tasks.Comments;

public class UpdateTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/comment/update";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<TaskListEntity> _taskListFactory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly IProjectDao _projectDao;
    private readonly ProjectEntity _project;
    private readonly IDataFactory<TaskCommentEntity> _taskCommentFactory;
    private readonly TaskCommentEntity _fakeComment;
    private readonly ITaskSeeder _taskSeeder;
    private readonly TaskEntity _task;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly ITaskCommentSeeder _taskCommentSeeder;
    private readonly TaskCommentEntity _comment;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;
    private readonly INotificationCenterService _notificationCenterService;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _taskListFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskListEntity>>();
        _taskCommentFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskCommentEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _taskCommentSeeder = ServiceProvider.GetRequiredService<ITaskCommentSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        _userNotificationTokenDao = ServiceProvider.GetRequiredService<IUserNotificationTokenDao>();
        _notificationCenterService = ServiceProvider.GetRequiredService<INotificationCenterService>();
        
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectDao.CreateAsync(_workspace, "Test adding").Result;
        var taskList = _taskListSeeder.CreateAsync(_project).Result;
        _task = _taskSeeder.CreateAsync(taskList, user: _user).Result;
        
        _fakeComment = _taskCommentFactory.Generate();
        _comment = _taskCommentSeeder.CreateAsync(_task, user: _user).Result;
        QueueProcess(QueueChannel.Notifications).Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            CommentId = _comment.Id,
            Comment = _fakeComment.Comment
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpdate()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            CommentId = _comment.Id,
            Comment = _fakeComment.Comment
        });
        response.EnsureSuccessStatusCode();

        var actualEntity = await response.GetJsonDataAsync<TaskCommentDto>();
        Assert.NotEqual(Guid.Empty, actualEntity.Id);
        Assert.Equal(_fakeComment.Comment, actualEntity.Comment);
        Assert.Equal(_user.Id, actualEntity.User.Id);
        
        var actualProcessedCounter = await QueueProcess(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        Assert.True(SmtpClientServiceMock.IsEmailSent);
        Assert.Contains(
            SmtpClientServiceMock.SentMessages, 
            item => item.To == _user.Email
                && item.Body.Contains("updated")
                && item.Body.Contains($"{_task.Id}")
        );
    }
    
    [Fact]
    public async Task ShouldUpdateWithGcmNotification()
    {
        // Arrange
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var (otherToken3, user3, otherWorkspace3) = await UserSeeder.CreateAuthorizedAsync();
        
        await _userNotificationTokenDao.Set(user3, FirebaseClientServiceMock.SuccessToken);
        
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            user2,
            MembershipAccessType.User, 
            new List<ProjectAccessModel>()
            {
                new()
                {
                    Project = _project
                }
            }
        );
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            user3,
            MembershipAccessType.User, 
            new List<ProjectAccessModel>()
            {
                new()
                {
                    Project = _project
                }
            }
        );
        await FlushDbChanges();
        await _notificationCenterService.MarkAllAsRead(_user, _task.Workspace);
        
        // Act
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            CommentId = _comment.Id,
            Comment = _fakeComment.Comment,
            WatcherIds = new List<Guid>() { user2.Id, user3.Id }
        });
        await FlushDbChanges(true);
        await QueueProcess(QueueChannel.Default);
        
        // Assert
        response.EnsureSuccessStatusCode();

        Assert.Equal(0, await _notificationCenterService.GetUnreadCount(_user, _task.Workspace));
        Assert.Equal(1, await _notificationCenterService.GetUnreadCount(user2, _task.Workspace));
        Assert.Equal(1, await _notificationCenterService.GetUnreadCount(user3, _task.Workspace));
    }
    
    [Fact]
    public async Task ShouldUpdateWithWatchers()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var (otherToken3, user3, otherWorkspace3) = await UserSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            user2,
            MembershipAccessType.User, 
            new List<ProjectAccessModel>()
            {
                new()
                {
                    Project = _project
                }
            }
        );
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            user3,
            MembershipAccessType.User, 
            new List<ProjectAccessModel>()
            {
                new()
                {
                    Project = _project
                }
            }
        );
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            CommentId = _comment.Id,
            Comment = _fakeComment.Comment,
            WatcherIds = new List<Guid>() { user2.Id, user3.Id }
        });
        response.EnsureSuccessStatusCode();

        var actualEntity = await response.GetJsonDataAsync<TaskCommentDto>();
        Assert.NotEqual(Guid.Empty, actualEntity.Id);
        Assert.Equal(2, actualEntity.Watchers.Count);
        Assert.Contains(actualEntity.Watchers, item => item.Id == user2.Id);
        Assert.Contains(actualEntity.Watchers, item => item.Id == user3.Id);
        
        var actualProcessedCounter = await QueueProcess(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);
        
        Assert.True(SmtpClientServiceMock.IsEmailSent);
        Assert.Contains(
            SmtpClientServiceMock.SentMessages, 
            item => item.To == user2.Email || item.To == user3.Email
        );
    }
    
    [Fact]
    public async Task ShouldNotUpdateWithUnsharedWatcher()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var (otherToken3, user3, otherWorkspace3) = await UserSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            user2,
            MembershipAccessType.User, 
            new List<ProjectAccessModel>()
            {
                new()
                {
                    Project = _task.TaskList.Project
                }
            }
        );
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            CommentId = _comment.Id,
            Comment = _fakeComment.Comment,
            WatcherIds = new List<Guid>() { user2.Id, user3.Id }
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfNotOwner()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            user2,
            MembershipAccessType.User, 
            new List<ProjectAccessModel>()
            {
                new()
                {
                    Project = _task.TaskList.Project
                }
            }
        );
        var response = await PostRequestAsync(Url, otherToken, new UpdateRequest()
        {
            CommentId = _comment.Id,
            Comment = _fakeComment.Comment,
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }
    
    [Fact]
    public async Task ShouldUpdateForSharedUser()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var comment = _taskCommentSeeder.CreateAsync(_task, user: user2).Result;
        
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            user2,
            MembershipAccessType.User, 
            new List<ProjectAccessModel>()
            {
                new()
                {
                    Project = _task.TaskList.Project
                }
            }
        );
        var response = await PostRequestAsync(Url, otherToken, new UpdateRequest()
        {
            CommentId = comment.Id,
            Comment = _fakeComment.Comment,
        });
        response.EnsureSuccessStatusCode();

        var actualEntity = await response.GetJsonDataAsync<TaskCommentDto>();
        Assert.NotEqual(Guid.Empty, actualEntity.Id);
        Assert.Equal(_fakeComment.Comment, actualEntity.Comment);
        Assert.Equal(user2.Id, actualEntity.User.Id);
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfIncorrectCommentId()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            CommentId = Guid.Empty,
            Comment = _fakeComment.Comment
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }
}
