using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.Report.ClientSharedReport;

public class SettingsTest : BaseTest
{
    private readonly string _url;
    private readonly string _ownerToken;
    private readonly WorkspaceEntity _workspace;
    private readonly ClientEntity _client;
    private readonly IUserSeeder _userSeeder;

    public SettingsTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        var projectSeeder = ServiceProvider.GetRequiredService<IProjectSeeder>();
        (_ownerToken, _, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _client = projectSeeder.CreateAsync(_workspace).Result.Client;
        _url = $"/{ApiUrl.ReportClientShareSettings}";
    }

    [Fact]
    public async Task OwnerCanCreateAndRegenerateClientReportLink()
    {
        var initialResponse = await PostRequestAsync(_url, _ownerToken, new ClientShareReportSettingsRequest
        {
            ClientId = _client.Id,
            IsActive = true,
            IsShowTasks = true,
            IsUpdateSettings = true
        }, _workspace.Id);
        initialResponse.EnsureSuccessStatusCode();
        var initialSettings = await initialResponse.GetJsonDataAsync<ClientShareReportSettingsResponse>();

        var regeneratedResponse = await PostRequestAsync(_url, _ownerToken, new ClientShareReportSettingsRequest
        {
            ClientId = _client.Id,
            IsActive = true,
            IsShowTasks = false,
            IsUpdateSettings = true,
            IsRegenerateToken = true
        }, _workspace.Id);
        regeneratedResponse.EnsureSuccessStatusCode();
        var regeneratedSettings = await regeneratedResponse.GetJsonDataAsync<ClientShareReportSettingsResponse>();

        Assert.True(initialSettings.IsActive);
        Assert.True(initialSettings.IsShowTasks);
        Assert.True(initialSettings.Token.Length >= 40);
        Assert.Contains($"/shared/report/client/{initialSettings.Token}", initialSettings.ShareUrl);
        Assert.NotEqual(initialSettings.Token, regeneratedSettings.Token);
        Assert.False(regeneratedSettings.IsShowTasks);
    }

    [Fact]
    public async Task OwnerCanManageClientReportLinkInSoloWorkspace()
    {
        _workspace.Mode = WorkspaceMode.Solo;
        await DbSessionProvider.CurrentSession.UpdateAsync(_workspace);
        await FlushDbChanges();

        var response = await PostRequestAsync(_url, _ownerToken, new ClientShareReportSettingsRequest
        {
            ClientId = _client.Id,
            IsActive = true,
            IsShowTasks = true,
            IsUpdateSettings = true
        }, _workspace.Id);
        response.EnsureSuccessStatusCode();

        var settings = await response.GetJsonDataAsync<ClientShareReportSettingsResponse>();
        Assert.True(settings.IsActive);
        Assert.True(settings.Token.Length >= 40);
    }

    [Fact]
    public async Task WorkspaceUserCannotManageClientReportLink()
    {
        var (userToken, _, _) = await _userSeeder.CreateAuthorizedAndShareAsync(_workspace, MembershipAccessType.User);

        var response = await PostRequestAsync(_url, userToken, new ClientShareReportSettingsRequest
        {
            ClientId = _client.Id,
            IsActive = true,
            IsShowTasks = true,
            IsUpdateSettings = true
        }, _workspace.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
