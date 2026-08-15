using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Client;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/client/list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly IClientSeeder _clientSeeder;
    private readonly IProjectSeeder _projectSeeder;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _clientSeeder = ServiceProvider.GetRequiredService<IClientSeeder>();
        _projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            Page = 1
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveList()
    {
        var expectedCounter = 15;
        await _clientSeeder.CreateSeveralAsync(_user, expectedCounter);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            Page = 1
        });
        await response.EnsureSuccessStatusCodeWithoutError();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
        
        Assert.All(actualDto.Items, item =>
        {
            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.NotEmpty(item.Name);
        });
    }

    [Fact]
    public async Task ShouldReceiveOnlyClientsWithSharedProjectsIfWorkspaceUser()
    {
        var sharedProject = await _projectSeeder.CreateAsync(_defaultWorkspace);
        var unsharedProject = await _projectSeeder.CreateAsync(_defaultWorkspace);
        var (userToken, _, _) = await UserSeeder.CreateAuthorizedAndShareAsync(
            _defaultWorkspace,
            MembershipAccessType.User,
            new List<ProjectAccessModel>
            {
                new() { Project = sharedProject }
            }
        );

        var response = await PostRequestAsync(Url, userToken, new GetListRequest
        {
            Page = 1
        }, _defaultWorkspace.Id);
        await response.EnsureSuccessStatusCodeWithoutError();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(1, actualDto.TotalCount);
        Assert.Equal(sharedProject.Client.Id, Assert.Single(actualDto.Items).Id);
        Assert.DoesNotContain(actualDto.Items, item => item.Id == unsharedProject.Client.Id);
    }
}
