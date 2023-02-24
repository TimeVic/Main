using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;
using UpdateRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List.UpdateRequest;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tasks.List;

public class ArchiveTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/list/archive";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<TaskListEntity> _taskListFactory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly IProjectDao _projectDao;
    private readonly ProjectEntity _project;
    private readonly ITaskListDao _taskListDao;
    private readonly TaskListEntity _taskList;
    private readonly string _taskListName;

    public ArchiveTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskListFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskListEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _taskListDao = ServiceProvider.GetRequiredService<ITaskListDao>();
        
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectDao.CreateAsync(_workspace, "Test adding").Result;
        var taskList = _taskListFactory.Generate();

        _taskListName = taskList.Name;
        _taskList = _taskListDao.CreateTaskListAsync(_project, taskList.Name).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            Name = _taskList.Name,
            ProjectId = _project.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldArchive()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new ArchiveTaskListRequest()
        {
            TaskListId = _taskList.Id
        });
        response.EnsureSuccessStatusCode();

        var taskList = await DbSessionProvider.CurrentSession.GetAsync<TaskListEntity>(_taskList.Id);
        Assert.True(taskList.IsArchived);
    }

    [Fact]
    public async Task ShouldNotUpdateIfTaskListNotFound()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new ArchiveTaskListRequest()
        {
            TaskListId = 9999999
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), error.Type);
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfHasNotAccess()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );
        var response = await PostRequestAsync(Url, otherToken, new ArchiveTaskListRequest()
        {
            TaskListId = _taskList.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
}
