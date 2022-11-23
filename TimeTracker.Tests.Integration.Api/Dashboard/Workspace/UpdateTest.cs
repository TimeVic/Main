using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Workspace;

public class UpdateTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/update";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<WorkspaceEntity> _workspaceFactory;
    private readonly string _jwtToken;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly WorkspaceEntity _workspace;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public UpdateTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _workspaceFactory = ServiceProvider.GetRequiredService<IDataFactory<WorkspaceEntity>>();
        _workspaceSeeder = ServiceProvider.GetRequiredService<IWorkspaceSeeder>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _workspace = _workspaceSeeder.CreateSeveralAsync(_user, 1).Result.First();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new UpdateRequest()
        {
            WorkspaceId = _workspace.Id,
            Name = _workspace.Name,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldUpdate()
    {
        var expectWorkspace = _workspaceFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new UpdateRequest()
        {
            WorkspaceId = _workspace.Id,
            Name = expectWorkspace.Name,
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<WorkspaceDto>();
        Assert.True(actual.Id > 0);
        Assert.Equal(expectWorkspace.Name, actual.Name);
        Assert.Equal(MembershipAccessType.Owner, actual.CurrentUserAccess);
    }
    
    [Fact]
    public async Task ShouldUpdateUserWithRoleManager()
    {
        var (otherJwt, otherUser, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.Manager
        );
        
        var expectWorkspace = _workspaceFactory.Generate();
        var response = await PostRequestAsync(Url, otherJwt, new UpdateRequest()
        {
            WorkspaceId = _workspace.Id,
            Name = expectWorkspace.Name,
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<WorkspaceDto>();
        Assert.True(actual.Id > 0);
        Assert.Equal(expectWorkspace.Name, actual.Name);
        Assert.Equal(MembershipAccessType.Manager, actual.CurrentUserAccess);
    }
    
    [Fact]
    public async Task ShouldNotUpdateUserWithRoleUser()
    {
        var (otherJwt, otherUser, _) = await _userSeeder.CreateAuthorizedAndShareAsync(
            _workspace,
            MembershipAccessType.User
        );
        
        var expectWorkspace = _workspaceFactory.Generate();
        var response = await PostRequestAsync(Url, otherJwt, new UpdateRequest()
        {
            WorkspaceId = _workspace.Id,
            Name = expectWorkspace.Name,
        });

        var actual = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), actual.Type);
    }
}
