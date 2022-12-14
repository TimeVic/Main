using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.WorkspaceMembership;

public class AddTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/membership/add";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    
    private readonly UserEntity _newUserFake;
    private readonly WorkspaceEntity _workspace;
    private string _otherJwtToken;
    private UserEntity _otherUser;
    private WorkspaceEntity _otherWorkspace;
    private readonly IQueueDao _queueDao;

    public AddTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userFactory = ServiceProvider.GetRequiredService<IDataFactory<UserEntity>>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _queueDao = ServiceProvider.GetRequiredService<IQueueDao>();
        _workspaceAccessService = ServiceProvider.GetRequiredService<IWorkspaceAccessService>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;

        _newUserFake = _userFactory.Generate();
        
        (_otherJwtToken, _otherUser, _otherWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        
        // Clear queue
        _queueDao.CompleteAllPending().Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new AddRequest()
        {
            Email = _newUserFake.Email,
            WorkspaceId = _workspace.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldAdd()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            Email = _newUserFake.Email,
            WorkspaceId = _workspace.Id
        });
        response.EnsureSuccessStatusCode();

        var actualMembership = await response.GetJsonDataAsync<WorkspaceMembershipDto>();
        Assert.True(actualMembership.Id > 0);
        Assert.NotNull(actualMembership.User);
        Assert.Equal(MembershipAccessType.User, actualMembership.Access);
        Assert.True(actualMembership.User.Id > 0);
    }
    
    [Fact]
    public async Task ShouldNotAddIfIncorrectWorkspaceId()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            Email = _newUserFake.Email,
            WorkspaceId = _otherWorkspace.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
    
    [Fact]
    public async Task UserWithManagerRoleShouldAdd()
    {
        var (otherJwtToken, otherUser, _) = await UserSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            otherUser,
            MembershipAccessType.Manager
        );
        
        var response = await PostRequestAsync(Url, otherJwtToken, new AddRequest()
        {
            Email = _newUserFake.Email,
            WorkspaceId = _workspace.Id
        });
        response.EnsureSuccessStatusCode();
    }
    
    [Fact]
    public async Task UserWithUserRoleShouldNotAdd()
    {
        var (otherJwtToken, otherUser, _) = await UserSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            otherUser,
            MembershipAccessType.User
        );
        
        var response = await PostRequestAsync(Url, otherJwtToken, new AddRequest()
        {
            Email = _newUserFake.Email,
            WorkspaceId = _workspace.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.Type);
    }
    
    [Fact]
    public async Task ShouldNotShareIfAlreadySharedAndNotPending()
    {
        var activeUser = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            activeUser,
            MembershipAccessType.User
        );
        
        var (otherJwtToken, otherUser, _) = await UserSeeder.CreateAuthorizedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            otherUser,
            MembershipAccessType.Manager
        );
        
        var response = await PostRequestAsync(Url, otherJwtToken, new AddRequest()
        {
            Email = activeUser.Email,
            WorkspaceId = _workspace.Id
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonErrorAsync();
        Assert.Equal(new RecordIsExistsException().GetTypeName(), error.Type);
    }
}
