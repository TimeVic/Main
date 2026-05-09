using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.WorkspaceMember;

public class AddTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/member/add";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly IUserDao _userDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    
    private readonly UserEntity _newUserFake;
    private readonly WorkspaceEntity _workspace;
    private string _otherJwtToken;
    private UserEntity _otherUser;
    private WorkspaceEntity _otherWorkspace;
    private new readonly IQueueDao _queueDao;

    public AddTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userFactory = ServiceProvider.GetRequiredService<IDataFactory<UserEntity>>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
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
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldAdd()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            Email = _newUserFake.Email,
        });
        response.EnsureSuccessStatusCode();

        var actualMembership = await response.GetJsonDataAsync<WorkspaceMemberDto>();
        Assert.NotEqual(Guid.Empty, actualMembership.Id);
        Assert.NotNull(actualMembership.User);
        Assert.Equal(MembershipAccessType.User, actualMembership.Access);
        Assert.NotEqual(Guid.Empty, actualMembership.User.Id);
    }

    [Fact]
    public async Task ShouldSendRegistrationInvitationToNewMember()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            Email = _newUserFake.Email,
        });
        response.EnsureSuccessStatusCode();

        var invitedUser = await _userDao.GetByEmail(_newUserFake.Email);
        Assert.NotNull(invitedUser);
        Assert.False(invitedUser.IsActivated);
        Assert.NotEmpty(invitedUser.VerificationToken!);

        var actualProcessedCounter = await QueueProcess(QueueChannel.Notifications);
        Assert.True(actualProcessedCounter > 0);

        Assert.True(SmtpClientServiceMock.IsEmailSent);
        var actualEmail = SmtpClientServiceMock.SentMessages.LastOrDefault();
        Assert.NotNull(actualEmail);
        Assert.Contains(invitedUser.Email, actualEmail.To);
        Assert.Contains("/registration/verification/", actualEmail.Body);
        Assert.Contains(invitedUser.VerificationToken!, actualEmail.Body);
    }
    
    [Fact]
    public async Task ShouldNotAddIfIncorrectWorkspaceId()
    {
        var (_, _, otherWorkspace) = await UserSeeder.CreateAuthorizedAsync();
        var response = await PostRequestAsync(Url, _jwtToken, new AddRequest()
        {
            Email = _newUserFake.Email,
        }, otherWorkspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
        
        var actualUser = await _userDao.GetByEmail(_newUserFake.Email);
        Assert.Null(actualUser);
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
        }, _workspace.Id);
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
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new HasNoAccessException().GetTypeName(), error.ErrorCode);
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
        }, _workspace.Id);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new RecordIsExistsException().GetTypeName(), error.ErrorCode);
    }
}
