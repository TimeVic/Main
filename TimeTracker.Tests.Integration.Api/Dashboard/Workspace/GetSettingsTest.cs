using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Dashboard.Workspace;

public class GetSettingsTest: BaseTest
{
    private readonly string Url = "/dashboard/workspace/settings/get";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly WorkspaceEntity _workspace;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
    
    private readonly string? _redmineApiKey;
    private readonly long _redmineUserId;
    private readonly string? _redmineTaskId;
    private readonly long _redmineActivityId;
    private readonly string? _redmineUrl;
    private readonly string? _clickUpsecurityKey;
    private readonly string? _clickUpteamId;
    private readonly string? _clickUptaskId;

    public GetSettingsTest(ApiCustomWebApplicationFactory factory) : base(factory)
    { 
        _workspaceSeeder = ServiceProvider.GetRequiredService<IWorkspaceSeeder>();
        _workspaceSettingsDao = ServiceProvider.GetRequiredService<IWorkspaceSettingsDao>();
        _userSeeder = ServiceProvider.GetRequiredService<IUserSeeder>();
        (_jwtToken, _user, _workspace) = UserSeeder.CreateAuthorizedAsync().Result;
        _workspace = _workspaceSeeder.CreateSeveralAsync(_user, 1).Result.First();
        
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        _redmineApiKey = configuration.GetValue<string>("Integration:Redmine:ApiKey");
        _redmineUserId = configuration.GetValue<long>("Integration:Redmine:UserId");
        _redmineTaskId = configuration.GetValue<string>("Integration:Redmine:TaskId");
        _redmineActivityId = configuration.GetValue<long>("Integration:Redmine:ActivityId");
        _redmineUrl = configuration.GetValue<string>("Integration:Redmine:Url");
        _workspaceSettingsDao.SetRedmineAsync(
            _user,
            _workspace,
            _redmineUrl,
            _redmineApiKey,
            _redmineUserId,
            _redmineActivityId
        ).Wait();
        
        _clickUpsecurityKey = configuration.GetValue<string>("Integration:ClickUp:SecurityKey");
        _clickUpteamId = configuration.GetValue<string>("Integration:ClickUp:TeamId");
        _clickUptaskId = configuration.GetValue<string>("Integration:ClickUp:TaskId");
        _workspaceSettingsDao.SetClickUpAsync(
            _user,
            _workspace,
            _clickUpsecurityKey,
            _clickUpteamId,
            true,
            true
        ).Wait();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new GetSettingsRequest()
        {
            WorkspaceId = _workspace.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldReceive()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new GetSettingsRequest()
        {
            WorkspaceId = _workspace.Id
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetSettingsResponse>();
        Assert.NotNull(actualResponse.IntegrationRedmine);
        Assert.Equal(_redmineUrl, actualResponse.IntegrationRedmine.Url);
        Assert.Equal(_redmineApiKey, actualResponse.IntegrationRedmine.ApiKey);
        Assert.Equal(_redmineActivityId, actualResponse.IntegrationRedmine.ActivityId);
        Assert.Equal(_redmineUserId, actualResponse.IntegrationRedmine.RedmineUserId);
        
        Assert.NotNull(actualResponse.IntegrationClickUp);
        Assert.Equal(_clickUpsecurityKey, actualResponse.IntegrationClickUp.SecurityKey);
        Assert.Equal(_clickUpteamId, actualResponse.IntegrationClickUp.TeamId);
    }
    
    [Fact]
    public async Task ShouldNotReceiveForOtherUser()
    {
        var (otherJwt, otherUser, workspace) = await _userSeeder.CreateAuthorizedAndShareAsync(_workspace);
        await CommitDbChanges();
        var response = await PostRequestAsync(Url, otherJwt, new GetSettingsRequest()
        {
            WorkspaceId = _workspace.Id
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<GetSettingsResponse>();
        Assert.Null(actualResponse.IntegrationRedmine);
        Assert.Null(actualResponse.IntegrationClickUp);
    }
}
