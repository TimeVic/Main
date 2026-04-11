using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
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

public class GetReportByUserTest: BaseTest
{
    private readonly IUserSeeder _userSeeder;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ISummaryReportDao _reportsDao;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly IProjectSeeder _projectSeederSeeder;
    private readonly IPaymentDao _paymentDao;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IUserDao _userDao;

    public GetReportByUserTest(): base()
    {
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _projectSeederSeeder = Scope.Resolve<IProjectSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _paymentDao = Scope.Resolve<IPaymentDao>();
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
        var user1 = await _userSeeder.CreateActivatedAsync();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(user1, _workspace, new TimeEntryCreationDto()
            {
                StartTime = DateTime.UtcNow.AddDays(-1).AddHours(10),
                EndTime = DateTime.UtcNow.AddDays(-1).AddHours(15),
                IsBillable = true,
                HourlyRate = 12
            }, project1);
        }
        
        var project2 = await _projectSeederSeeder.CreateAsync(_workspace);
        var user2 = await _userSeeder.CreateActivatedAsync();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(user2, _workspace, new TimeEntryCreationDto()
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
        
        var result = await _reportsDao.GetReportByUserForOwnerOrManagerAsync(
            _workspace.Id,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)
        );
        Assert.Equal(3, result.Count);
        
        var mainUserItem = result.Single(item => item.UserId == _user.Id);
        var user1Item = result.Single(item => item.UserId == user1.Id);
        var user2Item = result.Single(item => item.UserId == user2.Id);

        AssertDurationHours(24, mainUserItem.Duration);
        AssertDurationHours(15, user1Item.Duration);
        AssertDurationHours(12, user2Item.Duration);
        
        Assert.Equal(360m, mainUserItem.Amount);
        Assert.Equal(180m, user1Item.Amount);
        Assert.Equal(120m, user2Item.Amount);
        
        result = await _reportsDao.GetReportByUserForOwnerOrManagerAsync(
            _workspace.Id,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)
        );
        Assert.Equal(3, result.Count);
        
        mainUserItem = result.Single(item => item.UserId == _user.Id);
        user1Item = result.Single(item => item.UserId == user1.Id);
        user2Item = result.Single(item => item.UserId == user2.Id);

        AssertDurationHours(24, mainUserItem.Duration);
        AssertDurationHours(15, user1Item.Duration);
        AssertDurationHours(12, user2Item.Duration);
        
        Assert.NotEmpty(mainUserItem.Email);
    }
    
    [Fact]
    public async Task ShouldReceiveReportForOther()
    {
        var projects = await _projectSeederSeeder.CreateSeveralAsync(_workspace, 2);
        await FlushDbChanges();
        var project1 = projects.First();
        var project2 = projects.Last();
        var user1 = await _userSeeder.CreateActivatedAsync();
        await _workspaceAccessService.ShareAccessAsync(
            _workspace,
            user1,
            MembershipAccessType.User,
            new List<ProjectAccessModel>()
            {
                new() { Project = project1 },
                new() { Project = project2 }
            }
        );
        
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(user1, _workspace, new TimeEntryCreationDto()
            {
                StartTime = DateTime.UtcNow.AddDays(-1).AddHours(10),
                EndTime = DateTime.UtcNow.AddDays(-1).AddHours(15),
                IsBillable = true,
                HourlyRate = 12
            }, project1);
        }
        
        var user2 = await _userSeeder.CreateActivatedAsync();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(user2, _workspace, new TimeEntryCreationDto()
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

        var result = await _reportsDao.GetReportByUserForOtherAsync(
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            user1.Id,
            new List<ProjectEntity> { project1, project2 }
        );
        Assert.Equal(2, result.Count);
        
        var user1Item = result.Single(item => item.UserId == user1.Id);
        var user2Item = result.Single(item => item.UserId == user2.Id);

        AssertDurationHours(15, user1Item.Duration);
        AssertDurationHours(12, user2Item.Duration);
        
        Assert.Equal(180, user1Item.Amount);
        Assert.Equal(0, user2Item.Amount);
    }

    private static void AssertDurationHours(double expectedHours, TimeSpan actualDuration)
    {
        Assert.Equal(
            (int)TimeSpan.FromHours(expectedHours).TotalSeconds,
            (int)Math.Round(actualDuration.TotalSeconds)
        );
    }
}
