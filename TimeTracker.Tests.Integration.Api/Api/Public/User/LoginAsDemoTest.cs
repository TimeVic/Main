using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Public.User;

public class LoginAsDemoTest : BaseTest
{
    private readonly string Url = "/user/login/as-demo";

    private readonly IJwtAuthService _jwtService;
    private readonly IUserAccessTokenDao _accessTokenDao;
    private readonly IUserDao _userDao;
    private readonly IDbSessionProvider _sessionProvider;

    public LoginAsDemoTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _jwtService = ServiceProvider.GetRequiredService<IJwtAuthService>();
        _accessTokenDao = ServiceProvider.GetRequiredService<IUserAccessTokenDao>();
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
        _sessionProvider = ServiceProvider.GetRequiredService<IDbSessionProvider>();
    }

    [Fact]
    public async Task ShouldReturnValidAuthData()
    {
        var response = await GetRequestAsAnonymousAsync(Url);
        response.EnsureSuccessStatusCode();

        var responseData = await response.GetJsonDataAsync<LoginResponseDto>();
        var jwtToken = response.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey());
        var accessToken = response.GetSetCookieValue(HttpCookieKeyEnum.AccessToken.GetKey());

        Assert.True(_jwtService.IsValidJwt(jwtToken!));
        Assert.NotEmpty(accessToken);
        Assert.Empty(responseData.JwtToken);
        Assert.Empty(responseData.AccessToken);
        Assert.NotEqual(Guid.Empty, responseData.User.Id);
        Assert.NotEmpty(responseData.User.Email);
        Assert.NotNull(responseData.User.DefaultWorkspace);
        Assert.True(responseData.User.DefaultWorkspace.IsDefault);

        var actualAccessToken = await _accessTokenDao.GetByToken(accessToken!);
        Assert.NotNull(actualAccessToken);
        Assert.Contains(actualAccessToken.JwtTokens, item => item.Token == jwtToken);
    }

    [Fact]
    public async Task ShouldReturnDemoEmail()
    {
        var response = await GetRequestAsAnonymousAsync(Url);
        response.EnsureSuccessStatusCode();

        var responseData = await response.GetJsonDataAsync<LoginResponseDto>();

        Assert.True(DemoAccountConstants.IsDemoEmail(responseData.User.Email));
    }

    [Fact]
    public async Task ShouldReuseExistingDemoUserIfCreatedWithinWeek()
    {
        var response1 = await GetRequestAsAnonymousAsync(Url);
        response1.EnsureSuccessStatusCode();
        var data1 = await response1.GetJsonDataAsync<LoginResponseDto>();

        var response2 = await GetRequestAsAnonymousAsync(Url);
        response2.EnsureSuccessStatusCode();
        var data2 = await response2.GetJsonDataAsync<LoginResponseDto>();

        Assert.Equal(data1.User.Id, data2.User.Id);
        Assert.Equal(data1.User.Email, data2.User.Email);
    }

    [Fact]
    public async Task ShouldCreateNewDemoUserIfOlderThanWeek()
    {
        var response1 = await GetRequestAsAnonymousAsync(Url);
        response1.EnsureSuccessStatusCode();
        var data1 = await response1.GetJsonDataAsync<LoginResponseDto>();

        // Age the existing demo user beyond 7 days
        var demoUser = await _userDao.GetLastDemoUserAsync();
        Assert.NotNull(demoUser);
        demoUser.CreatedAt = DateTime.UtcNow.AddDays(-8);
        await FlushDbChanges();

        var response2 = await GetRequestAsAnonymousAsync(Url);
        response2.EnsureSuccessStatusCode();
        var data2 = await response2.GetJsonDataAsync<LoginResponseDto>();

        Assert.NotEqual(data1.User.Id, data2.User.Id);
        Assert.NotEqual(data1.User.Email, data2.User.Email);
    }

    [Fact]
    public async Task ShouldReturnSoloModeByDefault()
    {
        var response = await GetRequestAsAnonymousAsync(Url);
        response.EnsureSuccessStatusCode();

        var responseData = await response.GetJsonDataAsync<LoginResponseDto>();
        Assert.NotNull(responseData.User.SelectedWorkspace);
        Assert.Equal(WorkspaceMode.Solo, responseData.User.SelectedWorkspace.Mode);
    }

    [Fact]
    public async Task ShouldReturnTeamModeWhenRequested()
    {
        var response = await GetRequestAsAnonymousAsync($"{Url}?mode=Team");
        response.EnsureSuccessStatusCode();

        var responseData = await response.GetJsonDataAsync<LoginResponseDto>();
        Assert.NotNull(responseData.User.SelectedWorkspace);
        Assert.Equal(WorkspaceMode.Team, responseData.User.SelectedWorkspace.Mode);
    }

    [Fact]
    public async Task ShouldReturnSoloModeWhenRequestedExplicitly()
    {
        var response = await GetRequestAsAnonymousAsync($"{Url}?mode=Solo");
        response.EnsureSuccessStatusCode();

        var responseData = await response.GetJsonDataAsync<LoginResponseDto>();
        Assert.NotNull(responseData.User.SelectedWorkspace);
        Assert.Equal(WorkspaceMode.Solo, responseData.User.SelectedWorkspace.Mode);
    }

    [Fact]
    public async Task ShouldCreateWorkspacesWithDifferentModes()
    {
        var response = await GetRequestAsAnonymousAsync(Url);
        response.EnsureSuccessStatusCode();

        var demoUser = await _userDao.GetLastDemoUserAsync();
        Assert.NotNull(demoUser);

        var workspaces = await _userDao.GetUsersWorkspaces(demoUser);
        Assert.Contains(workspaces, w => w.Mode == WorkspaceMode.Solo);
        Assert.Contains(workspaces, w => w.Mode == WorkspaceMode.Team);
    }

    [Fact]
    public async Task ShouldCreateTeamMembersWithApprovedAndUnapprovedTimeEntries()
    {
        var response = await GetRequestAsAnonymousAsync($"{Url}?mode=Team");
        response.EnsureSuccessStatusCode();

        var demoUser = await _userDao.GetLastDemoUserAsync();
        Assert.NotNull(demoUser);

        var workspaces = await _userDao.GetUsersWorkspaces(demoUser);
        var teamWorkspace = workspaces.First(w => w.Mode == WorkspaceMode.Team);

        // Verify team workspace has members
        var members = teamWorkspace.Members.Where(m => m.User.Id != demoUser.Id).ToList();
        Assert.Equal(2, members.Count);

        // Verify time entries for each member include approved and unapproved entries
        foreach (var member in members)
        {
            var entries = await _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
                .Where(e => e.Workspace.Id == teamWorkspace.Id && e.User.Id == member.User.Id)
                .Fetch(e => e.Approvals)
                .ToListAsync();

            Assert.NotEmpty(entries);
            Assert.Contains(entries, e => e.Status == TimeEntryStatus.Approved && e.Approvals.Count > 0);
            Assert.Contains(entries, e => e.Status != TimeEntryStatus.Approved);
        }

        // Verify demo user also has time entries with both approved and unapproved statuses in team workspace
        var ownerEntries = await _sessionProvider.CurrentSession.Query<TimeEntryEntity>()
            .Where(e => e.Workspace.Id == teamWorkspace.Id && e.User.Id == demoUser.Id)
            .Fetch(e => e.Approvals)
            .ToListAsync();
        Assert.NotEmpty(ownerEntries);
        Assert.Contains(ownerEntries, e => e.Status == TimeEntryStatus.Approved && e.Approvals.Count > 0);
        Assert.Contains(ownerEntries, e => e.Status != TimeEntryStatus.Approved);
    }
}
