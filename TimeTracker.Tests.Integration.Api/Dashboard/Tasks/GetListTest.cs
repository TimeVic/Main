using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tasks;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/get-list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly TaskListEntity _taskList;
    
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IDataFactory<TaskListEntity> _taskListFactory;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly ProjectEntity _project;
    private readonly ITaskSeeder _taskSeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskListFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskListEntity>>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _taskSeeder = ServiceProvider.GetRequiredService<ITaskSeeder>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectSeeder.CreateAsync(_defaultWorkspace).Result;
        _taskList = _taskListSeeder.CreateAsync(_project).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            TaskListId = _taskList.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 15;
        await _taskSeeder.CreateSeveralAsync(_taskList, expectedCounter);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            TaskListId = _taskList.Id,
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
        
        Assert.All(actualDto.Items, item =>
        {
            Assert.True(item.Id > 0);
            Assert.NotEmpty(item.Title);
            Assert.NotNull(item.TaskList);
            Assert.NotEmpty(item.Description);
            Assert.Equal(_taskList.Id, item.TaskList.Id);
        });
    }
}
