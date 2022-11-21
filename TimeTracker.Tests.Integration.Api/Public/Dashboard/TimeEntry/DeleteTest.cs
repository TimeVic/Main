using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Workspace;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Redmine;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Public.Dashboard.TimeEntry;

public class DeleteTest: BaseTest
{
    private readonly string Url = "/dashboard/time-entry/delete";
    
    private readonly UserEntity _user;
    private readonly string _jwtToken;
    private readonly WorkspaceEntity _defaultWorkspace;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly TimeEntryEntity _timeEntry;
    private readonly IQueueService _queueService;
    private readonly ClickUpClientMock _clickUpClient;
    private readonly RedmineClientMock _redmineClient;
    private readonly IQueueDao _queueDao;
    private readonly IWorkspaceSettingsDao _workspaceSettingsDao;
    private readonly string? _redmineApiKey;
    private readonly long _redmineUserId;
    private readonly string? _redmineTaskId;
    private readonly long _redmineActivityId;
    private readonly string? _redmineUrl;
    private readonly string? _clickUpsecurityKey;
    private readonly string? _clickUpteamId;
    private readonly string? _clickUptaskId;

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _queueDao = ServiceProvider.GetRequiredService<IQueueDao>();
        _clickUpClient = ServiceProvider.GetRequiredService<IClickUpClient>() as ClickUpClientMock;
        _redmineClient = ServiceProvider.GetRequiredService<IRedmineClient>() as RedmineClientMock;
        _workspaceSettingsDao = ServiceProvider.GetRequiredService<IWorkspaceSettingsDao>();
        _queueService = ServiceProvider.GetRequiredService<IQueueService>();
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        (_jwtToken, _user, _defaultWorkspace) = UserSeeder.CreateAuthorizedAsync().Result;
        
        _timeEntry = _timeEntryDao.StartNewAsync(_user, _defaultWorkspace, DateTime.Now, TimeSpan.Zero).Result;

        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        _redmineApiKey = configuration.GetValue<string>("Integration:Redmine:ApiKey");
        _redmineUserId = configuration.GetValue<long>("Integration:Redmine:UserId");
        _redmineTaskId = configuration.GetValue<string>("Integration:Redmine:TaskId");
        _redmineActivityId = configuration.GetValue<long>("Integration:Redmine:ActivityId");
        _redmineUrl = configuration.GetValue<string>("Integration:Redmine:Url");
        _workspaceSettingsDao.SetRedmineAsync(
            _user,
            _defaultWorkspace,
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
            _defaultWorkspace,
            _clickUpsecurityKey,
            _clickUpteamId,
            true,
            true
        ).Wait();
        
        _queueDao.CompleteAllPending().Wait();
        _clickUpClient.Reset();
        _redmineClient.Reset();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new DeleteRequest()
        {
            TimeEntryId = _timeEntry.Id
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldDeleteActiveEntry()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            TimeEntryId = _timeEntry.Id
        });
        response.EnsureSuccessStatusCode();

        await DbSessionProvider.CurrentSession.RefreshAsync(_timeEntry);
        Assert.True(_timeEntry.IsMarkedToDelete);

        await _queueService.ProcessAsync(QueueChannel.ExternalClient);
        Assert.False(
            await DbSessionProvider.CurrentSession.Query<TimeEntryEntity>()
                .AnyAsync(item => item.Id == _timeEntry.Id)
        );
    }
    
    [Fact]
    public async Task ShouldDeleteOldEntry()
    {
        var expectedEntry = (await _timeEntrySeeder.CreateSeveralAsync(_defaultWorkspace, _user)).First();
        await CommitDbChanges();
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            TimeEntryId = expectedEntry.Id
        });
        response.EnsureSuccessStatusCode();

        Assert.False(
            await DbSessionProvider.CurrentSession.Query<TimeEntryEntity>()
                .AnyAsync(item => item.Id == _timeEntry.Id && item.IsMarkedToDelete)
        );
    }
    
    [Fact]
    public async Task ShouldDeleteAndSendRequestToClickUp()
    {
        _timeEntry.TaskId = "#aa44gg"; // ClickUp ID
        await DbSessionProvider.CurrentSession.SaveAsync(_timeEntry);
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            TimeEntryId = _timeEntry.Id
        });
        response.EnsureSuccessStatusCode();
        
        await DbSessionProvider.CurrentSession.RefreshAsync(_timeEntry);
        Assert.True(_timeEntry.IsMarkedToDelete);

        await _queueService.ProcessAsync(QueueChannel.ExternalClient);
        Assert.True(_clickUpClient.IsSent);

        Assert.False(
            await DbSessionProvider.CurrentSession.Query<TimeEntryEntity>()
                .AnyAsync(item => item.Id == _timeEntry.Id)
        );
    }
    
    [Fact]
    public async Task ShouldDeleteAndSendRequestToRedmine()
    {
        _timeEntry.TaskId = "123123"; // Redmine ID
        await DbSessionProvider.CurrentSession.SaveAsync(_timeEntry);
        var response = await PostRequestAsync(Url, _jwtToken, new DeleteRequest()
        {
            TimeEntryId = _timeEntry.Id
        });
        response.EnsureSuccessStatusCode();
        
        await DbSessionProvider.CurrentSession.RefreshAsync(_timeEntry);
        Assert.True(_timeEntry.IsMarkedToDelete);

        await _queueService.ProcessAsync(QueueChannel.ExternalClient);
        Assert.True(_redmineClient.IsSent);
        
        Assert.False(
            await DbSessionProvider.CurrentSession.Query<TimeEntryEntity>()
                .AnyAsync(item => item.Id == _timeEntry.Id)
        );
    }
}
