using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.SubTasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
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

public class AddTest : BaseTest
{
    private readonly string Url = "/dashboard/tasks/sub-task/add";

    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly TaskEntity _task;
    private readonly ITaskSeeder _taskSeeder;

    public AddTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();

        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _task = _taskSeeder.CreateAsync(user: _user).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new AddRequest
        {
            TaskId = _task.Id,
            Title = "New Subtask"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldAdd()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest
        {
            TaskId = _task.Id,
            Title = "First Subtask"
        });
        response.EnsureSuccessStatusCode();

        var actualEntity = await response.GetJsonDataAsync<TaskSubTaskDto>();
        Assert.NotEqual(Guid.Empty, actualEntity.Id);
        Assert.Equal("First Subtask", actualEntity.Title);
        Assert.False(actualEntity.IsCompleted);
        Assert.Equal(0, actualEntity.PositionIndex);
        Assert.Equal(_task.Id, actualEntity.TaskId);

        // Add second subtask and check position index
        var response2 = await PostRequestAsync(Url, _jwtToken, new AddRequest
        {
            TaskId = _task.Id,
            Title = "Second Subtask"
        });
        response2.EnsureSuccessStatusCode();

        var actualEntity2 = await response2.GetJsonDataAsync<TaskSubTaskDto>();
        Assert.Equal(1, actualEntity2.PositionIndex);
    }

    [Fact]
    public async Task ShouldNotAddIfNoAccess()
    {
        var (otherToken, _, _) = await UserSeeder.CreateAuthorizedAsync();

        var response = await PostRequestAsync(Url, otherToken, new AddRequest
        {
            TaskId = _task.Id,
            Title = "Unauthorized Subtask"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }

    [Fact]
    public async Task ShouldNotAddIfTaskNotFound()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest
        {
            TaskId = Guid.NewGuid(),
            Title = "Subtask for non-existent task"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }

    [Fact]
    public async Task ShouldNotAddIfEmptyTitle()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest
        {
            TaskId = _task.Id,
            Title = ""
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotAddMoreThanMaxSubTasks()
    {
        var taskSubTaskDao = ServiceProvider.GetRequiredService<ITaskSubTaskDao>();
        for (var i = 0; i < GlobalConstants.MaxSubTasksPerTask; i++)
        {
            await taskSubTaskDao.AddAsync(_task, $"Subtask {i}");
        }
        await FlushDbChanges();

        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest
        {
            TaskId = _task.Id,
            Title = "Over limit subtask"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new DataValidationException().GetTypeName(), error.ErrorCode);
    }
}
