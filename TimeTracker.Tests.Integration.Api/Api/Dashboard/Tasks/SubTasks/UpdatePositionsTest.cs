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

public class UpdatePositionsTest : BaseTest
{
    private readonly string Url = "/dashboard/tasks/sub-task/update-positions";

    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _workspace;
    private readonly TaskEntity _task;
    private readonly TaskSubTaskEntity _subTask1;
    private readonly TaskSubTaskEntity _subTask2;
    private readonly TaskSubTaskEntity _subTask3;
    private readonly ITaskSeeder _taskSeeder;
    private readonly ITaskSubTaskDao _taskSubTaskDao;

    public UpdatePositionsTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _taskSubTaskDao = ServiceProvider.GetRequiredService<ITaskSubTaskDao>();

        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _task = _taskSeeder.CreateAsync(user: _user).Result;
        _subTask1 = _taskSubTaskDao.AddAsync(_task, "Subtask 1").Result;
        _subTask2 = _taskSubTaskDao.AddAsync(_task, "Subtask 2").Result;
        _subTask3 = _taskSubTaskDao.AddAsync(_task, "Subtask 3").Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new UpdatePositionsRequest
        {
            TaskId = _task.Id,
            Positions = new Dictionary<Guid, int>
            {
                { _subTask1.Id, 2 },
                { _subTask2.Id, 0 },
                { _subTask3.Id, 1 }
            }
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldUpdatePositions()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new UpdatePositionsRequest
        {
            TaskId = _task.Id,
            Positions = new Dictionary<Guid, int>
            {
                { _subTask1.Id, 2 },
                { _subTask2.Id, 0 },
                { _subTask3.Id, 1 }
            }
        });
        response.EnsureSuccessStatusCode();

        await DbSessionProvider.CurrentSession.RefreshAsync(_subTask1);
        await DbSessionProvider.CurrentSession.RefreshAsync(_subTask2);
        await DbSessionProvider.CurrentSession.RefreshAsync(_subTask3);

        Assert.Equal(2, _subTask1.PositionIndex);
        Assert.Equal(0, _subTask2.PositionIndex);
        Assert.Equal(1, _subTask3.PositionIndex);
    }

    [Fact]
    public async Task ShouldNotUpdateIfNoAccess()
    {
        var (otherToken, _, _) = await UserSeeder.CreateAuthorizedAsync();

        var response = await PostRequestAsync(Url, otherToken, new UpdatePositionsRequest
        {
            TaskId = _task.Id,
            Positions = new Dictionary<Guid, int>
            {
                { _subTask1.Id, 1 }
            }
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }

    [Fact]
    public async Task ShouldNotUpdateIfTaskNotFound()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new UpdatePositionsRequest
        {
            TaskId = Guid.NewGuid(),
            Positions = new Dictionary<Guid, int>
            {
                { _subTask1.Id, 1 }
            }
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
    }
}
