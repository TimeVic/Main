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

public class SetClickUpSettingsTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/settings/set-clickup";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<WorkspaceEntity> _workspaceFactory;
    private readonly string _jwtToken;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly WorkspaceEntity _workspace;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public SetClickUpSettingsTest(ApiCustomWebApplicationFactory factory) : base(factory)
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
        var response = await PostRequestAsAnonymousAsync(Url, new SetClickUpSettingsRequest()
        {
            SecurityKey = "someApi",
            TeamId = "someTeamId",
            IsCustomTaskIds = true,
            IsFillTimeEntryWithTaskDetails = false
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldSet()
    {
        var expectTeamId = "someTeamId";
        var expectApiKey = "someasdasdAPIKey";
        var response = await PostRequestAsync(Url, _jwtToken, new SetClickUpSettingsRequest()
        {
            SecurityKey = expectApiKey,
            TeamId = expectTeamId,
            IsCustomTaskIds = true,
            IsFillTimeEntryWithTaskDetails = false
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<WorkspaceSettingsClickUpDto>();
        Assert.Equal(expectApiKey, actual.SecurityKey);
        Assert.Equal(expectTeamId, actual.TeamId);
        Assert.Equal(true, actual.IsCustomTaskIds);
        Assert.Equal(false, actual.IsFillTimeEntryWithTaskDetails);
    }
    
    [Fact]
    public async Task ShouldActivateSettings()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new SetClickUpSettingsRequest()
        {
            SecurityKey = "someasdasdAPIKey",
            TeamId = "someTeamId",
            IsCustomTaskIds = true,
            IsFillTimeEntryWithTaskDetails = false
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<WorkspaceSettingsClickUpDto>();
        Assert.True(actual.IsActive);

        await DbSessionProvider.CurrentSession.RefreshAsync(_workspace);
        var actualSettings = _workspace.GetClickUpSettings(_user.Id);
        Assert.NotNull(actualSettings);
        Assert.True(actualSettings.IsActive);
    }
}
