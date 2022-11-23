using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Workspace;

public class AddTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/add";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<WorkspaceEntity> _workspaceFactory;
    private readonly string _jwtToken;

    public AddTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _workspaceFactory = ServiceProvider.GetRequiredService<IDataFactory<WorkspaceEntity>>();
        (_jwtToken, _user, _) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var project = _workspaceFactory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new AddRequest()
        {
            Name = project.Name,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldAdd()
    {
        var workspace = _workspaceFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            Name = workspace.Name
        });
        await response.GetJsonDataAsync();
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<WorkspaceDto>();
        Assert.True(actual.Id > 0);
        Assert.Equal(workspace.Name, actual.Name);
        Assert.Equal(MembershipAccessType.Owner, actual.CurrentUserAccess);
    }
}
