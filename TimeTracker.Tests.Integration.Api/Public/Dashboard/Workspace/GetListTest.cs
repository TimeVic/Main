using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.Workspace;

public class GetListTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/list";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private WorkspaceEntity _workspace;

    public GetListTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _workspaceSeeder = ServiceProvider.GetRequiredService<IWorkspaceSeeder>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
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
    
    [Fact]
    public async Task ShouldReceiveListWithAccessToWorkspace()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();

        var expectWorkspaces = await _workspaceSeeder.CreateSeveralAsync(otherUser, 2);
        await CommitDbChanges();
        
        var workspaceWithUserAccess = expectWorkspaces.First();
        await _workspaceAccessService.ShareAccessAsync(
            workspaceWithUserAccess,
            _user,
            MembershipAccessType.User
        );
        
        var workspaceWithManagerAccess = expectWorkspaces.Last();
        await _workspaceAccessService.ShareAccessAsync(
            workspaceWithManagerAccess,
            _user,
            MembershipAccessType.Manager
        );
        
        var response = await PostRequestAsync(Url, _jwtToken, new GetListRequest());
        response.EnsureSuccessStatusCode();

        var actualDto = await response.GetJsonDataAsync<PaginatedListDto<WorkspaceDto>>();
        Assert.Equal(3, actualDto.TotalCount);

        Assert.Contains(actualDto.Items, item =>
        {
            return item.Id == _workspace.Id && item.CurrentUserAccess == MembershipAccessType.Owner;
        });
        Assert.Contains(actualDto.Items, item =>
        {
            return item.Id == workspaceWithUserAccess.Id && item.CurrentUserAccess == MembershipAccessType.User;
        });
        Assert.Contains(actualDto.Items, item =>
        {
            return item.Id == workspaceWithManagerAccess.Id && item.CurrentUserAccess == MembershipAccessType.Manager;
        });
    }
}
