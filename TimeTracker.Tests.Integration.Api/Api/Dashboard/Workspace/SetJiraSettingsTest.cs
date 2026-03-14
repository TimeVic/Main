using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Workspace;

public class SetJiraSettingsTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/settings/set-jira";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<WorkspaceEntity> _workspaceFactory;
    private readonly string _jwtToken;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly WorkspaceEntity _workspace;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public SetJiraSettingsTest(ApiCustomWebApplicationFactory factory) : base(factory)
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
        var response = await PostRequestAsAnonymousAsync(Url, new SetJiraSettingsRequest()
        {
            WorkspaceId = _workspace.Id,
            Url = "http://bla.com",
            ApiKey = "someApi",
            UserName = "someTeamId"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldSet()
    {
        var expectUserName = "someTeamId";
        var expectApiKey = "someasdasdAPIKey";
        var response = await PostRequestAsync(Url, _jwtToken, new SetJiraSettingsRequest()
        {
            WorkspaceId = _workspace.Id,
            Url = "http://bla.com",
            ApiKey = expectApiKey,
            UserName = expectUserName
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<WorkspaceSettingsJiraDto>();
        Assert.Equal(expectApiKey, actual.ApiKey);
        Assert.Equal(expectUserName, actual.UserName);
    }
    
    [Fact]
    public async Task ShouldActivateSettings()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new SetJiraSettingsRequest()
        {
            WorkspaceId = _workspace.Id,
            Url = "http://bla.com",
            ApiKey = "someasdasdAPIKey",
            UserName = "someTeamId"
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<WorkspaceSettingsJiraDto>();
        Assert.True(actual.IsActive);

        await DbSessionProvider.CurrentSession.RefreshAsync(_workspace);
        var actualSettings = _workspace.GetJiraSettings(_user.Id);
        Assert.NotNull(actualSettings);
        Assert.True(actualSettings.IsActive);
    }
}
