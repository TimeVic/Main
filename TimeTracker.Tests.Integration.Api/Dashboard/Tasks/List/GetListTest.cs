using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;
using GetListRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List.GetListRequest;
using GetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List.GetListResponse;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Tasks.List;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/tasks/list/get-list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly IProjectSeeder _projectSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IDataFactory<TaskListEntity> _taskListFactory;
    private readonly ITaskListSeeder _taskListSeeder;
    private readonly ProjectEntity _project;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _taskListFactory = ServiceProvider.GetRequiredService<IDataFactory<TaskListEntity>>();
        _taskListSeeder = ServiceProvider.GetRequiredService<ITaskListSeeder>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _project = _projectSeeder.CreateAsync(_defaultWorkspace).Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 15;
        await _taskListSeeder.CreateSeveralAsync(_project, expectedCounter);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            WorkspaceId = _defaultWorkspace.Id
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
        
        Assert.All(actualDto.Items, item =>
        {
            Assert.True(item.Id > 0);
            Assert.NotEmpty(item.Name);
            Assert.NotNull(item.Project);
            Assert.Equal(_project.Id, item.Project.Id);
        });
    }
}
