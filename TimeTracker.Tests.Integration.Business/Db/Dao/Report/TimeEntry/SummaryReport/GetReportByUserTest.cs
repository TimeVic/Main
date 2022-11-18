using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Dto.TimeEntry;
using TimeTracker.Business.Orm.Entities;
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
        await DbSessionProvider.PerformCommitAsync();
        var project1 = await _projectSeederSeeder.CreateAsync(_workspace);
        var user1 = await _userSeeder.CreateActivatedAsync();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(user1, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow.AddDays(-1),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(15),
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
                Date = DateTime.UtcNow,
                StartTime = TimeSpan.FromHours(1),
                EndTime = TimeSpan.FromHours(5),
                IsBillable = true,
                HourlyRate = 10
            }, project2);
        }

        for (int i = 0; i < 4; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow.AddDays(1),
                StartTime = TimeSpan.FromHours(5),
                EndTime = TimeSpan.FromHours(11),
                IsBillable = true,
                HourlyRate = 15
            });
        }

        await CommitDbChanges();
        
        var result = await _reportsDao.GetReportByUserForOwnerOrManagerAsync(
            _workspace.Id,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)
        );
        Assert.Equal(3, result.Count);
        
        var firstReportItem = result.First();
        var secondReportItem = result.Skip(1).First();
        var thirdReportItem = result.Last();
        Assert.Equal(_user.Id, firstReportItem.UserId);
        Assert.Equal(user1.Id, secondReportItem.UserId);
        Assert.Equal(user2.Id, thirdReportItem.UserId);
        
        Assert.Equal(TimeSpan.FromHours(24), firstReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(15), secondReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(12), thirdReportItem.Duration);
        
        Assert.Equal(360m, firstReportItem.Amount);
        Assert.Equal(180m, secondReportItem.Amount);
        Assert.Equal(120m, thirdReportItem.Amount);
        
        result = await _reportsDao.GetReportByUserForOwnerOrManagerAsync(
            _workspace.Id,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)
        );
        Assert.Equal(3, result.Count);
        
        firstReportItem = result.First();
        secondReportItem = result.Skip(1).First();
        thirdReportItem = result.Last();
        Assert.Equal(_user.Id, firstReportItem.UserId);
        Assert.Equal(user1.Id, secondReportItem.UserId);
        Assert.Equal(user2.Id, thirdReportItem.UserId);
        
        Assert.Equal(TimeSpan.FromHours(24), firstReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(15), secondReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(12), thirdReportItem.Duration);
        
        Assert.NotEmpty(firstReportItem.Email);
        Assert.NotEmpty(firstReportItem.UserName);
    }
    
    [Fact]
    public async Task ShouldReceiveReportForOther()
    {
        var projects = await _projectSeederSeeder.CreateSeveralAsync(_workspace, 2);
        await DbSessionProvider.PerformCommitAsync();
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
                Date = DateTime.UtcNow.AddDays(-1),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(15),
                IsBillable = true,
                HourlyRate = 12
            }, project1);
        }
        
        var user2 = await _userSeeder.CreateActivatedAsync();
        for (int i = 0; i < 3; i++)
        {
            await _timeEntryDao.SetAsync(user2, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow,
                StartTime = TimeSpan.FromHours(1),
                EndTime = TimeSpan.FromHours(5),
                IsBillable = true,
                HourlyRate = 10
            }, project2);
        }

        for (int i = 0; i < 4; i++)
        {
            await _timeEntryDao.SetAsync(_user, _workspace, new TimeEntryCreationDto()
            {
                Date = DateTime.UtcNow.AddDays(1),
                StartTime = TimeSpan.FromHours(5),
                EndTime = TimeSpan.FromHours(11),
                IsBillable = true,
                HourlyRate = 15
            });
        }

        await CommitDbChanges();

        var result = await _reportsDao.GetReportByUserForOtherAsync(
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            user1.Id,
            new List<ProjectEntity> { project1, project2 }
        );
        Assert.Equal(2, result.Count);
        
        var firstReportItem = result.First();
        var secondReportItem = result.Last();

        Assert.Equal(user1.Id, firstReportItem.UserId);
        Assert.Equal(user2.Id, secondReportItem.UserId);
        
        Assert.Equal(TimeSpan.FromHours(15), firstReportItem.Duration);
        Assert.Equal(TimeSpan.FromHours(12), secondReportItem.Duration);
        
        Assert.Equal(180, firstReportItem.Amount);
        Assert.Equal(0, secondReportItem.Amount);
    }
}
