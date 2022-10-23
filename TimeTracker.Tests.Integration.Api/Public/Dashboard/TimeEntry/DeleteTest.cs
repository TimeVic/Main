using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
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

    public DeleteTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _timeEntryDao = ServiceProvider.GetRequiredService<ITimeEntryDao>();
        _timeEntrySeeder = ServiceProvider.GetRequiredService<ITimeEntrySeeder>();
        (_jwtToken, _user) = UserSeeder.CreateAuthorizedAsync().Result;
        _defaultWorkspace = _user.Workspaces.First();

        _timeEntry = _timeEntryDao.StartNewAsync(_user, _defaultWorkspace, DateTime.Now, TimeSpan.Zero).Result;
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
                .AnyAsync(item => item.Id == expectedEntry.Id)
        );
    }
}
