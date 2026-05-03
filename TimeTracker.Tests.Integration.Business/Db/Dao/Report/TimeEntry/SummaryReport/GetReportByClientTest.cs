using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Services.Security.Model;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db.Dao.Report.TimeEntry.SummaryReport;

public class GetReportByClientTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ISummaryReportDao _reportsDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectSeeder _projectSeederSeeder;
    private readonly IMemberPaymentDao _paymentDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private IUserDao _userDao;

    public GetReportByClientTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeederSeeder = Scope.Resolve<IProjectSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _paymentDao = Scope.Resolve<IMemberPaymentDao>();
        _reportsDao = Scope.Resolve<ISummaryReportDao>();
        _userDao = Scope.Resolve<IUserDao>();

        _user = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
    }

    [Fact]
    public async Task ShouldReceiveReportForOwnerOrManager()
    {
        await FlushDbChanges();
        var project1 = await _projectSeederSeeder.CreateAsync(_workspace);
        var client1 = project1.Client;
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = DateTime.UtcNow.AddDays(-1).AddHours(10),
                EndTime = DateTime.UtcNow.AddDays(-1).AddHours(15),
                IsBillable = true,
                HourlyRate = 12
            }, project1);
        }
        
        var project2 = await _projectSeederSeeder.CreateAsync(_workspace);
        var client2 = project2.Client;
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(5),
                IsBillable = true,
                HourlyRate = 10
            }, project2);
        }

        for (int i = 0; i < 4; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = DateTime.UtcNow.AddDays(1).AddHours(5),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(11),
                IsBillable = true,
                HourlyRate = 15
            });
        }
        await FlushDbChanges();
        
        var result = await _reportsDao.GetReportByClientForOwnerOrManagerAsync(
            _workspace.Id,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)
        );
        Assert.Equal(3, result.Count);
        
        Assert.NotNull(client1);
        Assert.NotNull(client2);

        var client1Item = result.Single(item => item.ClientId == client1.Id);
        var client2Item = result.Single(item => item.ClientId == client2.Id);
        var noClientItem = result.Single(item => item.ClientId == null);

        AssertDurationHours(15, client1Item.Duration);
        AssertDurationHours(12, client2Item.Duration);
        AssertDurationHours(24, noClientItem.Duration);
        
        Assert.Equal(180m, client1Item.Amount);
        Assert.Equal(120m, client2Item.Amount);
        Assert.Equal(360m, noClientItem.Amount);
        
        result = await _reportsDao.GetReportByClientForOwnerOrManagerAsync(
            _workspace.Id,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)
        );
        Assert.Equal(3, result.Count);
        
        client1Item = result.Single(item => item.ClientId == client1.Id);
        client2Item = result.Single(item => item.ClientId == client2.Id);
        noClientItem = result.Single(item => item.ClientId == null);

        AssertDurationHours(15, client1Item.Duration);
        AssertDurationHours(12, client2Item.Duration);
        AssertDurationHours(24, noClientItem.Duration);
        
        Assert.Equal(180m, client1Item.Amount);
        Assert.Equal(120m, client2Item.Amount);
        Assert.Equal(360m, noClientItem.Amount);
    }
    
    [Fact]
    public async Task ShouldReceiveReportForOther()
    {
        await FlushDbChanges();
        var project1 = await _projectSeederSeeder.CreateAsync(_workspace);
        var project2 = await _projectSeederSeeder.CreateAsync(_workspace);
        var otherUser = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            otherUser,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
            {
                new() { Project = project1 },
                new() { Project = project2 }
            }
        );

        var client1 = project1.Client;
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(otherUser, _workspace, new TimeEntryCreationDto()
            {
                StartTime = DateTime.UtcNow.AddDays(-1).AddHours(10),
                EndTime = DateTime.UtcNow.AddDays(-1).AddHours(15),
                IsBillable = true,
                HourlyRate = 12
            }, project1);
        }
        
        var client2 = project2.Client;
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(5),
                IsBillable = true,
                HourlyRate = 10
            }, project2);
        }

        for (int i = 0; i < 4; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                StartTime = DateTime.UtcNow.AddDays(1).AddHours(5),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(11),
                IsBillable = true,
                HourlyRate = 15
            });
        }

        await FlushDbChanges();

        var result = await _reportsDao.GetReportByClientForOtherAsync(
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            otherUser.Id,
            new List<ProjectEntity> { project1, project2 }
        );
        Assert.Equal(2, result.Count);
        
        Assert.NotNull(client1);
        Assert.NotNull(client2);

        // The "for other" SQL returns all entries in accessible projects (not just otherUser's),
        // but Amount is computed only for otherUser's billable hours.
        var client1Item = result.Single(item => item.ClientId == client1.Id);
        var client2Item = result.Single(item => item.ClientId == client2.Id);

        // client1: 3 x 5h by otherUser → Duration=15h, Amount=3*5*12=180
        AssertDurationHours(15, client1Item.Duration);
        Assert.Equal(180, client1Item.Amount);

        // client2: 3 x 4h by _user (not otherUser) → Duration=12h, Amount=0
        AssertDurationHours(12, client2Item.Duration);
        Assert.Equal(0, client2Item.Amount);
    }

    private static void AssertDurationHours(double expectedHours, TimeSpan actualDuration)
    {
        Assert.Equal(
            (int)TimeSpan.FromHours(expectedHours).TotalSeconds,
            (int)Math.Round(actualDuration.TotalSeconds)
        );
    }
}
