using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api;
using TimeTracker.Tests.Integration.Api.Core;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/membership/list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly WorkspaceEntity _workspace;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetListRequest()
        {
            WorkspaceId = _workspace.Id,
            Page = 1
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceiveListAsOwner()
    {
        var expectedCounter = 8;
        await CreateUsersAndAddMembers(expectedCounter);
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest()
        {
            WorkspaceId = _workspace.Id,
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter, actualDto.TotalCount);
        
        Assert.All(actualDto.Items, item =>
        {
            Assert.True(item.Id > 0);
            Assert.Equal(MembershipAccessType.User, item.Access);
            Assert.True(item.User.Id > 0);
        });
    }
    
    [Fact]
    public async Task UserWithRoleManagerShouldReceiveList()
    {
        var (otherJwtToken, otherUser, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var expectedMembership = await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            otherUser,
            MembershipAccessType.Manager
        );
        
        var expectedCounter = 8;
        await CreateUsersAndAddMembers(expectedCounter);
        
        var response = await PostRequestAsync(Url, otherJwtToken, new GetListRequest()
        {
            WorkspaceId = _workspace.Id,
            Page = 1
        });
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<GetListResponse>();
        Assert.Equal(expectedCounter + 1, actualDto.TotalCount);
        Assert.Contains(actualDto.Items, item => item.Id == expectedMembership.Id);
    }
    
    [Fact]
    public async Task UserWithRoleUserShouldNotReceiveList()
    {
        var (otherJwtToken, otherUser, workspace) = await UserSeeder.CreateAuthorizedAsync();
        var expectedMembership = await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            otherUser,
            MembershipAccessType.User
        );
        
        var expectedCounter = 8;
        await CreateUsersAndAddMembers(expectedCounter);
        
        var response = await PostRequestAsync(Url, otherJwtToken, new GetListRequest()
        {
            WorkspaceId = _workspace.Id,
            Page = 1
        });
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }

    private async Task CreateUsersAndAddMembers(int counter)
    {
        var users = await _userSeeder.CreateActivatedAsync(counter);
        foreach (var user in users)
        {
            await _workspaceAccessService.ShareAccessAsync(
                _workspace,
                user,
                MembershipAccessType.User
            );
        }
    }
}
