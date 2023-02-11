using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;
using AddRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List.AddRequest;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tasks.List;

public class AddTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/list/add";
    
    private readonly IQueueService _queueService;
    private readonly UserEntity _user;
    private readonly IDataFactory<TaskListEntity> _taskListFactory;
    private readonly string _jwtToken;
    private WorkspaceEntity _workspace;
    private readonly IProjectDao _projectDao;
    private readonly ProjectEntity _project;

    public AddTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _taskListFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskListEntity>>();
        _projectDao = ServiceProvider.GetRequiredService<IProjectDao>();
        
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectDao.CreateAsync(_workspace, "Test adding").Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var project = _taskListFactory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new AddRequest()
        {
            Name = project.Name,
            ProjectId = _project.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldAdd()
    {
        var taskList = _taskListFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            Name = taskList.Name,
            ProjectId = _project.Id
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actualProject = await response.GetJsonDataAsync<TaskListDto>();
        Assert.True(actualProject.Id > 0);
        Assert.Equal(taskList.Name, actualProject.Name);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task ShouldNotAddIfIncorrectWorkspaceId()
    {
        var (otherToken, user2, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var otherProject = _projectDao.CreateAsync(otherWorkspace, "Test adding").Result;
        var project = _taskListFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            Name = project.Name,
            ProjectId = otherProject.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
}
