using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tasks.Comments;

public class DeleteTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/comment/delete";
    
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

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _taskListFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskListEntity>>();
        _taskCommentFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskCommentEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _taskCommentSeeder = ServiceProvider.GetRequiredService<ITaskCommentSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectDao.CreateAsync(_workspace, "Test adding").Result;
        var taskList = _taskListSeeder.CreateAsync(_project).Result;
        _task = _taskSeeder.CreateAsync(taskList, user: _user).Result;
        
        _fakeComment = _taskCommentFactory.Generate();
        _comment = _taskCommentSeeder.CreateAsync(_task, user: _user).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new DeleteRequest()
        {
            CommentId = _comment.Id,
        });
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldDelete()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            CommentId = _comment.Id
        });
        response.EnsureSuccessStatusCode();

        await DbSessionProvider.CurrentSession.RefreshAsync(_comment);
        Assert.True(_comment.IsArchived);
    }
    
    [Fact]
    public async Task ShouldNotDeleteIfNotOwner()
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
        var response = await PostRequestAsync(Url, otherToken, new DeleteRequest()
        {
            CommentId = _comment.Id,
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
    
    [Fact]
    public async Task ShouldNotDeleteIfIncorrectCommentId()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            CommentId = 999999,
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
}
