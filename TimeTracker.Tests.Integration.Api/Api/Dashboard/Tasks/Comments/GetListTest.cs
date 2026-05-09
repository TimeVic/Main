using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;
using TimeTracker.Business.Testing.Seeders.Entity;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Tasks.Comments;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/comment/get-list";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<TaskListEntity> _taskListFactory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ProjectEntity _project;
    private readonly IDataFactory<TaskCommentEntity> _taskCommentFactory;
    private readonly TaskCommentEntity _fakeComment;
    private readonly ITaskSeeder _taskSeeder;
    private readonly TaskEntity _task;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly ITaskCommentSeeder _taskCommentSeeder;
    private readonly ICollection<TaskCommentEntity> _comments;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _taskListFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskListEntity>>();
        _taskCommentFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskCommentEntity>>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _taskCommentSeeder = ServiceProvider.GetRequiredService<ITaskCommentSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectSeeder.CreateAsync(_workspace).Result;
        _task = _taskSeeder.CreateAsync(user: _user).Result;
        
        _fakeComment = _taskCommentFactory.Generate();
        _comments = _taskCommentSeeder.CreateSeveralAsync(_task, count: 4, user: _user).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            Page = 1,
            TaskId = _task.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldGet()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            Page = 1,
            TaskId = _task.Id
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(4, actualResponse.Items.Count);
        Assert.Equal(4, actualResponse.TotalCount);
        foreach (var currentItem in actualResponse.Items)
        {
            Assert.NotEqual(Guid.Empty, currentItem.Id);
            Assert.NotEmpty(currentItem.Comment);
            Assert.True(currentItem.CreatedAt.ToUniversalTime() <= DateTime.UtcNow);
            Assert.Equal(_task.TaskId, currentItem.Task.TaskId);
        }
    }
    
    [Fact]
    public async Task ShouldNotGetFromAnotherTask()
    {
        var task2 = await _taskSeeder.CreateAsync(user: _user);
        await _taskCommentSeeder.CreateSeveralAsync(task2, count: 5, user: _user);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            Page = 1,
            TaskId = _task.Id
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(4, actualResponse.Items.Count);
        Assert.Equal(4, actualResponse.TotalCount);
        Assert.All(actualResponse.Items, item =>
        {
            Assert.Equal(_task.TaskId, item.Task.TaskId);
        });
    }
    
    [Fact]
    public async Task ShouldNotGetIfHasNotPermissions()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        
        var response = await PostRequestAsync(Url, otherToken, new GetListRequest()
        {
            Page = 1,
            TaskId = _task.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }
}
