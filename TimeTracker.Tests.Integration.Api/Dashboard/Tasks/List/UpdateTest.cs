using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
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

public class UpdateTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/list/update";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<TaskListEntity> _taskListFactory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly IProjectDao _projectDao;
    private readonly ProjectEntity _project;
    private readonly ITaskListDao _taskListDao;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _taskListFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskListEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        _taskListDao = ServiceProvider.GetRequiredService<ITaskListDao>();
        
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectDao.CreateAsync(_workspace, "Test adding").Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var taskList = _taskListFactory.Generate();
        taskList = await _taskListDao.CreateTaskListAsync(_project, taskList.Name);
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            Name = taskList.Name,
            ProjectId = _project.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpdate()
    {
        var expectedName = _taskListFactory.Generate().Name;
        var taskList = await _taskListDao.CreateTaskListAsync(_project, expectedName);
        
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            Name = expectedName,
            ProjectId = _project.Id,
            TaskListId = taskList.Id
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualProject = await response.GetJsonDataAsync<TaskListDto>();
        Assert.True(actualProject.Id > 0);
        Assert.Equal(expectedName, actualProject.Name);
        Assert.Equal(taskList.Id, actualProject.Id);
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfIncorrectWorkspaceId()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var otherProject = _projectDao.CreateAsync(otherWorkspace, "Test adding").Result;
        var taskList = await _taskListDao.CreateTaskListAsync(_project, "Some name");
        var project = _taskListFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            Name = project.Name,
            ProjectId = otherProject.Id,
            TaskListId = taskList.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
    
    [Fact]
    public async Task ShouldNotUpdateIfTaskListNotFound()
    {
        var project = _taskListFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            Name = project.Name,
            ProjectId = _project.Id,
            TaskListId = 9999999
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new RecordNotFoundException().GetTypeName(), error.Type);
    }
}
