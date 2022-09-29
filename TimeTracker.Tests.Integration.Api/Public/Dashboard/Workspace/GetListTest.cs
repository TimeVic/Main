using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.Workspace;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly IWorkspaceSeeder _workspaceSeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _workspaceSeeder = ServiceProvider.GetRequiredService<IWorkspaceSeeder>();
        (_jwtToken, _user) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 8;
        await _workspaceSeeder.CreateSeveralAsync(_user, expectedCounter - 1);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest());
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<PaginatedListDto<WorkspaceDto>>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
        
        Assert.All(actualDto.Items, item =>
        {
            Assert.True(item.Id > 0);
            Assert.NotNull(item.Name);
        });
    }
}
