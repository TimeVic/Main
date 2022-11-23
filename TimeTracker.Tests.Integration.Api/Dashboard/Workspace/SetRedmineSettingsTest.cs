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

public class SetRedmineSettingsTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/settings/set-redmine";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<WorkspaceEntity> _workspaceFactory;
    private readonly string _jwtToken;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly WorkspaceEntity _workspace;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public SetRedmineSettingsTest(ApiCustomWebApplicationFactory factory) : base(factory)
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
    public async Task ShouldSet()
    {
        var expectUserId = 9;
        var expectActivityId = 8;
        var expectApiKey = "someasdasdAPIKey";
        var expectUrl = "http://some.url/";
        var response = await PostRequestAsync(Url, _jwtToken, new SetRedmineSettingsRequest()
        {
            WorkspaceId = _workspace.Id,
            Url = expectUrl,
            ActivityId = expectActivityId,
            ApiKey = expectApiKey,
            RedmineUserId = expectUserId
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<WorkspaceSettingsRedmineDto>();
        Assert.Equal(expectActivityId, actual.ActivityId);
        Assert.Equal(expectUserId, actual.RedmineUserId);
        Assert.Equal(expectApiKey, actual.ApiKey);
        Assert.Equal(expectUrl, actual.Url);
    }
    
    [Fact]
    public async Task ShouldActivateSettings()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new SetRedmineSettingsRequest()
        {
            WorkspaceId = _workspace.Id,
            Url = "http://some.url/",
            ActivityId = 9,
            ApiKey = "someasdasdAPIKey",
            RedmineUserId = 8
        });
        response.EnsureSuccessStatusCode();

        var actual = await response.GetJsonDataAsync<WorkspaceSettingsRedmineDto>();
        Assert.True(actual.IsActive);

        await DbSessionProvider.CurrentSession.RefreshAsync(_workspace);
        var actualSettings = _workspace.GetRedmineSettings(_user.Id);
        Assert.True(actualSettings.IsActive);
    }
}
