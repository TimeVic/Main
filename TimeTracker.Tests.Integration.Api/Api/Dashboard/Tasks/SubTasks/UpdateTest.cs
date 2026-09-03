using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.SubTasks;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Business.Testing.Seeders.Entity.Task;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Tasks.SubTasks;

public class UpdateTest : BaseTest
{
    private readonly string Url = "/dashboard/tasks/sub-task/update";

    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly TaskEntity _task;
    private readonly TaskSubTaskEntity _subTask;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ITaskSubTaskDao _taskSubTaskDao;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _taskSubTaskDao = ServiceProvider.GetRequiredService<ITaskSubTaskDao>();

        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _task = _taskSeeder.CreateAsync(user: _user).Result;
        _subTask = _taskSubTaskDao.AddAsync(_task, "Initial title").Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest
        {
            SubTaskId = _subTask.Id,
            Title = "Updated title",
            IsCompleted = true
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldUpdate()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest
        {
            SubTaskId = _subTask.Id,
            Title = "Updated Title",
            IsCompleted = true
        });
        response.EnsureSuccessStatusCode();

        var actualEntity = await response.GetJsonDataAsync<TaskSubTaskDto>();
        Assert.Equal(_subTask.Id, actualEntity.Id);
        Assert.Equal("Updated Title", actualEntity.Title);
        Assert.True(actualEntity.IsCompleted);

        // Toggle back to false
        var response2 = await PostRequestAsync(Url, _jwtToken, new UpdateRequest
        {
            SubTaskId = _subTask.Id,
            Title = "Updated Title 2",
            IsCompleted = false
        });
        response2.EnsureSuccessStatusCode();

        var actualEntity2 = await response2.GetJsonDataAsync<TaskSubTaskDto>();
        Assert.Equal("Updated Title 2", actualEntity2.Title);
        Assert.False(actualEntity2.IsCompleted);
    }

    [Fact]
    public async Task ShouldNotUpdateIfNoAccess()
    {
        var (otherToken, _, _) = await UserSeeder.CreateAuthorizedAsync();

        var response = await PostRequestAsync(Url, otherToken, new UpdateRequest
        {
            SubTaskId = _subTask.Id,
            Title = "Unauthorized edit",
            IsCompleted = true
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }

    [Fact]
    public async Task ShouldNotUpdateIfSubTaskNotFound()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest
        {
            SubTaskId = Guid.NewGuid(),
            Title = "Non-existent subtask",
            IsCompleted = false
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }

    [Fact]
    public async Task ShouldNotUpdateIfEmptyTitle()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest
        {
            SubTaskId = _subTask.Id,
            Title = "",
            IsCompleted = false
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
