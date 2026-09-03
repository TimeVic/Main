using System.Net;
using Microsoft.Extensions.DependencyInjection;
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

public class DeleteTest : BaseTest
{
    private readonly string Url = "/dashboard/tasks/sub-task/delete";

    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly TaskEntity _task;
    private readonly TaskSubTaskEntity _subTask;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ITaskSubTaskDao _taskSubTaskDao;

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _taskSubTaskDao = ServiceProvider.GetRequiredService<ITaskSubTaskDao>();

        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _task = _taskSeeder.CreateAsync(user: _user).Result;
        _subTask = _taskSubTaskDao.AddAsync(_task, "To be deleted").Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new DeleteRequest
        {
            SubTaskId = _subTask.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldDelete()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest
        {
            SubTaskId = _subTask.Id
        });
        response.EnsureSuccessStatusCode();

        var deletedSubTask = await _taskSubTaskDao.GetById(_subTask.Id);
        Assert.Null(deletedSubTask);
    }

    [Fact]
    public async Task ShouldNotDeleteIfNoAccess()
    {
        var (otherToken, _, _) = await UserSeeder.CreateAuthorizedAsync();

        var response = await PostRequestAsync(Url, otherToken, new DeleteRequest
        {
            SubTaskId = _subTask.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }

    [Fact]
    public async Task ShouldNotDeleteIfSubTaskNotFound()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest
        {
            SubTaskId = Guid.NewGuid()
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }
}
