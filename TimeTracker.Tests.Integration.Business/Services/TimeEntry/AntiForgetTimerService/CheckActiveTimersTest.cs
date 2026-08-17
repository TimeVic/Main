using Autofac;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.TimeEntry.AntiForgetTimerService;

public class CheckActiveTimersTest : BaseTest
{
    private readonly IAntiForgetTimerService _antiForgetTimerService;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;
    private readonly IProjectSeeder _projectSeeder;
    private readonly UserEntity _user;
    private readonly WorkspaceEntity _workspace;
    private readonly SmtpClientServiceMock _smtpClientService;

    public CheckActiveTimersTest() : base()
    {
        var userSeeder = Scope.Resolve<IUserSeeder>();
        var userDao = Scope.Resolve<IUserDao>();
        _antiForgetTimerService = Scope.Resolve<IAntiForgetTimerService>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _userNotificationTokenDao = Scope.Resolve<IUserNotificationTokenDao>();
        _projectSeeder = Scope.Resolve<IProjectSeeder>();
        _smtpClientService = (Scope.Resolve<ISmtpClientService>() as SmtpClientServiceMock)!;
        _user = userSeeder.CreateActivatedAsync().Result;
        _workspace = userDao.GetUsersWorkspaces(_user, MembershipAccessType.Owner).Result.First();
        _smtpClientService.Reset();
    }

    [Fact]
    public async Task ShouldSendOnePushWarningAfterTenHours()
    {
        var currentTime = DateTime.UtcNow;
        var entry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            currentTime.AddHours(-10).AddMinutes(-1)
        );
        await _userNotificationTokenDao.Set(_user, FirebaseClientServiceMock.SuccessToken);
        await FlushDbChanges();

        await _antiForgetTimerService.CheckActiveTimersAsync(currentTime);
        await FlushAndRefreshEntity(entry);

        Assert.Null(entry.EndTime);
        Assert.NotNull(entry.AutoStopWarningSentAt);
        Assert.InRange(
            entry.AutoStopWarningSentAt.Value,
            currentTime.AddMilliseconds(-1),
            currentTime.AddMilliseconds(1)
        );
        var notification = Assert.Single(FirebaseClientService.SentMessages);
        Assert.EndsWith($"/board/{_workspace.Id}", notification.Data["url"]);

        await _antiForgetTimerService.CheckActiveTimersAsync(currentTime.AddMinutes(15));
        await FlushDbChanges();

        Assert.Single(FirebaseClientService.SentMessages);
    }

    [Fact]
    public async Task ShouldAutoStopAtTwelveHoursAndSendTransactionalEmail()
    {
        var currentTime = DateTime.UtcNow;
        var project = await _projectSeeder.CreateAsync(_workspace);
        var entry = await _timeEntryDao.StartNewAsync(
            _user,
            _workspace,
            currentTime.AddHours(-12).AddMinutes(-1),
            description: "Implementation work",
            projectId: project.Id
        );
        entry.Project = project;
        await DbSessionProvider.CurrentSession.SaveAsync(entry);
        await FlushDbChanges();

        await _antiForgetTimerService.CheckActiveTimersAsync(currentTime);
        await FlushAndRefreshEntity(entry);

        Assert.True(entry.IsAutostopped);
        Assert.Equal(entry.StartTime.AddHours(8), entry.EndTime);
        Assert.Equal(TimeSpan.FromHours(8), entry.Duration);
        Assert.Contains("[Auto-stopped]", entry.Description);

        await QueueProcess(QueueChannel.Notifications);

        var email = Assert.Single(
            _smtpClientService.SentMessages,
            item => item.Subject.Contains("Did you forget to stop your timer")
        );
        Assert.Contains(project.Name, email.Body);
        Assert.Contains("8 hours", email.Body);
    }
}
