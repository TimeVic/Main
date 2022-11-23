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
            WorkspaceId = _workspace.Id,
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
            WorkspaceId = _workspace.Id,
            SecurityKey = expectApiKey,
            TeamId = expectTeamId,
            IsCustomTaskIds = true,
            IsFillTimeEntryWithTaskDetails = false
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<WorkspaceSettingsClickUpDto>();
        Assert.Equal(expectApiKey, actual.SecurityKey);
        Assert.Equal(expectTeamId, actual.TeamId);
        Assert.Equal(true, actual.IsCustomTaskIds);
        Assert.Equal(false, actual.IsFillTimeEntryWithTaskDetails);
    }
}
