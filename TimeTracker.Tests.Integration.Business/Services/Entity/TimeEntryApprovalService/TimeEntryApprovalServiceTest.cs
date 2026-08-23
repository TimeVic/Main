using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Entity.TimeEntryApprovalService;

public class TimeEntryApprovalServiceTest : BaseTest
{
    private readonly ITimeEntryApprovalService _approvalService;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ITimeEntrySeeder _timeEntrySeeder;
    private readonly IUserSeeder _userSeeder;
    private readonly IWorkspaceSeeder _workspaceSeeder;
    private readonly IWorkspaceAccessService _workspaceAccessService;
    private readonly IQueueService _queueService;
    private new readonly IQueueDao _queueDao;

    private readonly UserEntity _owner;
    private readonly UserEntity _manager;
    private readonly UserEntity _developer;
    private readonly WorkspaceEntity _workspace;

    public TimeEntryApprovalServiceTest() : base()
    {
        _approvalService = Scope.Resolve<ITimeEntryApprovalService>();
        _timeEntryDao = Scope.Resolve<ITimeEntryDao>();
        _timeEntrySeeder = Scope.Resolve<ITimeEntrySeeder>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _workspaceSeeder = Scope.Resolve<IWorkspaceSeeder>();
        _workspaceAccessService = Scope.Resolve<IWorkspaceAccessService>();
        _queueService = Scope.Resolve<IQueueService>();
        _queueDao = Scope.Resolve<IQueueDao>();

        _owner = _userSeeder.CreateActivatedAsync().Result;
        _manager = _userSeeder.CreateActivatedAsync().Result;
        _developer = _userSeeder.CreateActivatedAsync().Result;
        _workspace = _workspaceSeeder.CreateSeveralAsync(_owner).Result.First();
        _workspace.IsApprovalsEnabled = true;
        _workspace.Mode = WorkspaceMode.Team;
        FlushDbChanges().Wait();

        _workspaceAccessService.ShareAccessAsync(_workspace, _manager, MembershipAccessType.Manager).Wait();
        _workspaceAccessService.ShareAccessAsync(_workspace, _developer, MembershipAccessType.User).Wait();

        _queueDao.CompleteAllPending().Wait();
    }

    [Fact]
    public async Task SubmitAsync_ShouldChangeStatusToPending()
    {
        var entry = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _developer, 1)).First();
        entry.EndTime = entry.StartTime.AddHours(2);
        entry.Status = TimeEntryStatus.Draft;
        await FlushDbChanges();

        var result = await _approvalService.SubmitAsync(_developer, entry);
        Assert.Equal(TimeEntryStatus.Pending, result.Status);
    }

    [Fact]
    public async Task SubmitAsync_ShouldThrowWhenActiveEntry()
    {
        var entry = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _developer, 1)).First();
        entry.EndTime = null;
        entry.Status = TimeEntryStatus.Draft;
        await FlushDbChanges();

        await Assert.ThrowsAsync<RecordCanNotBeModifiedException>(async () =>
        {
            await _approvalService.SubmitAsync(_developer, entry);
        });
    }

    [Fact]
    public async Task SubmitAsync_ShouldThrowWhenNotAuthor()
    {
        var entry = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _developer, 1)).First();
        entry.EndTime = entry.StartTime.AddHours(2);
        entry.Status = TimeEntryStatus.Draft;
        await FlushDbChanges();

        await Assert.ThrowsAsync<HasNoAccessException>(async () =>
        {
            await _approvalService.SubmitAsync(_manager, entry);
        });
    }

    [Fact]
    public async Task SubmitPeriodAsync_ShouldSubmitAllDraftAndRejectedEntries()
    {
        var entries = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _developer, 3)).ToList();
        var baseDate = DateTime.UtcNow.Date;

        entries[0].StartTime = baseDate.AddDays(-2).AddHours(9);
        entries[0].EndTime = entries[0].StartTime.AddHours(4);
        entries[0].Status = TimeEntryStatus.Draft;

        entries[1].StartTime = baseDate.AddDays(-1).AddHours(9);
        entries[1].EndTime = entries[1].StartTime.AddHours(4);
        entries[1].Status = TimeEntryStatus.Rejected;

        entries[2].StartTime = baseDate.AddDays(-10).AddHours(9);
        entries[2].EndTime = entries[2].StartTime.AddHours(4);
        entries[2].Status = TimeEntryStatus.Draft;

        await FlushDbChanges();

        var submitted = await _approvalService.SubmitPeriodAsync(
            _developer,
            _workspace,
            baseDate.AddDays(-5),
            baseDate
        );

        Assert.Equal(2, submitted.Count);
        Assert.All(submitted, e => Assert.Equal(TimeEntryStatus.Pending, e.Status));
    }

    [Fact]
    public async Task ApproveAsync_ShouldApproveAndCreateApprovalEntity()
    {
        var entry = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _developer, 1)).First();
        entry.EndTime = entry.StartTime.AddHours(2);
        entry.Status = TimeEntryStatus.Pending;
        await FlushDbChanges();

        var result = await _approvalService.ApproveAsync(_manager, entry);
        Assert.Equal(TimeEntryStatus.Approved, result.Status);

        await FlushDbChanges(true);
        var reloaded = await DbSessionProvider.CurrentSession.GetAsync<TimeEntryEntity>(entry.Id);
        Assert.NotEmpty(reloaded.Approvals);
        Assert.Equal(_manager.Id, reloaded.Approvals.First().User.Id);
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrowWhenRegularUser()
    {
        var entry = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _developer, 1)).First();
        entry.EndTime = entry.StartTime.AddHours(2);
        entry.Status = TimeEntryStatus.Pending;
        await FlushDbChanges();

        await Assert.ThrowsAsync<HasNoAccessException>(async () =>
        {
            await _approvalService.ApproveAsync(_developer, entry);
        });
    }

    [Fact]
    public async Task RejectAsync_ShouldRejectCreateEntityAndPushQueue()
    {
        var entry = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _developer, 1)).First();
        entry.EndTime = entry.StartTime.AddHours(2);
        entry.Status = TimeEntryStatus.Pending;
        await FlushDbChanges();

        var result = await _approvalService.RejectAsync(_manager, entry, "Wrong task description");
        Assert.Equal(TimeEntryStatus.Rejected, result.Status);

        await FlushDbChanges(true);
        var reloaded = await DbSessionProvider.CurrentSession.GetAsync<TimeEntryEntity>(entry.Id);
        Assert.NotEmpty(reloaded.Rejections);
        Assert.Equal("Wrong task description", reloaded.Rejections.First().Reason);
        Assert.Equal(_manager.Id, reloaded.Rejections.First().User.Id);
    }

    [Fact]
    public async Task UnapproveAsync_ShouldRevertToDraft()
    {
        var entry = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _developer, 1)).First();
        entry.EndTime = entry.StartTime.AddHours(2);
        entry.Status = TimeEntryStatus.Approved;
        await FlushDbChanges();

        var result = await _approvalService.UnapproveAsync(_owner, entry);
        Assert.Equal(TimeEntryStatus.Draft, result.Status);
    }

    [Fact]
    public async Task GetStatusSummaryAsync_ShouldCalculateDurationsAndAmounts()
    {
        var entries = (await _timeEntrySeeder.CreateSeveralAsync(_workspace, _developer, 3)).ToList();
        
        entries[0].HourlyRate = 50;
        entries[0].IsBillable = true;
        entries[0].StartTime = DateTime.UtcNow.AddHours(-4);
        entries[0].EndTime = entries[0].StartTime.AddHours(2);
        entries[0].Status = TimeEntryStatus.Draft;

        entries[1].HourlyRate = 50;
        entries[1].IsBillable = true;
        entries[1].StartTime = DateTime.UtcNow.AddHours(-2);
        entries[1].EndTime = entries[1].StartTime.AddHours(1);
        entries[1].Status = TimeEntryStatus.Pending;

        entries[2].HourlyRate = 50;
        entries[2].IsBillable = true;
        entries[2].StartTime = DateTime.UtcNow.AddHours(-1);
        entries[2].EndTime = entries[2].StartTime.AddHours(1);
        entries[2].Status = TimeEntryStatus.Rejected;

        await FlushDbChanges();

        await _approvalService.RejectAsync(_manager, entries[2], "Fix please");
        await FlushDbChanges();

        var summary = await _approvalService.GetStatusSummaryAsync(_developer, _workspace);

        Assert.Equal(1, summary.DraftCount);
        Assert.Equal(100m, summary.DraftAmount);
        Assert.Equal(TimeSpan.FromHours(2), summary.DraftDuration);

        Assert.Equal(1, summary.PendingCount);
        Assert.Equal(50m, summary.PendingAmount);
        Assert.Equal(TimeSpan.FromHours(1), summary.PendingDuration);

        Assert.Equal(1, summary.RejectedCount);
        Assert.Equal("Fix please", summary.LatestRejectionReason);
        Assert.True(summary.HasDraftEntries);
        Assert.True(summary.HasRejectedEntries);
        Assert.Equal(150m, summary.PendingAndDraftAmount);
    }

    [Fact]
    public async Task TimeEntriesCreatedByOwnerAndManager_ShouldHaveDraftStatusWhenApprovalsEnabled()
    {
        var ownerEntry = await _timeEntryDao.SetAsync(_owner, _workspace, new TimeTracker.Business.Orm.Dto.TimeEntry.TimeEntryCreationDto
        {
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow.AddHours(-1),
            Description = "Owner manual entry"
        });
        Assert.Equal(TimeEntryStatus.Draft, ownerEntry.Status);

        var managerEntry = await _timeEntryDao.SetAsync(_manager, _workspace, new TimeTracker.Business.Orm.Dto.TimeEntry.TimeEntryCreationDto
        {
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow.AddHours(-1),
            Description = "Manager manual entry"
        });
        Assert.Equal(TimeEntryStatus.Draft, managerEntry.Status);

        var ownerTimerEntry = await _timeEntryDao.StartNewAsync(_owner, _workspace, DateTime.UtcNow.AddMinutes(-30));
        Assert.Equal(TimeEntryStatus.Draft, ownerTimerEntry.Status);

        var managerTimerEntry = await _timeEntryDao.StartNewAsync(_manager, _workspace, DateTime.UtcNow.AddMinutes(-30));
        Assert.Equal(TimeEntryStatus.Draft, managerTimerEntry.Status);
    }

    [Fact]
    public async Task OwnerAndManagerTimeEntries_ShouldFollowApprovalLifecycle()
    {
        var ownerEntry = await _timeEntryDao.SetAsync(_owner, _workspace, new TimeTracker.Business.Orm.Dto.TimeEntry.TimeEntryCreationDto
        {
            StartTime = DateTime.UtcNow.AddHours(-3),
            EndTime = DateTime.UtcNow.AddHours(-1),
            Description = "Owner entry for approval"
        });
        Assert.Equal(TimeEntryStatus.Draft, ownerEntry.Status);

        // Owner submits own entry -> directly Approved
        var ownerSubmitted = await _approvalService.SubmitAsync(_owner, ownerEntry);
        Assert.Equal(TimeEntryStatus.Approved, ownerSubmitted.Status);

        // Manager submits own entry -> Pending
        var managerEntry = await _timeEntryDao.SetAsync(_manager, _workspace, new TimeTracker.Business.Orm.Dto.TimeEntry.TimeEntryCreationDto
        {
            StartTime = DateTime.UtcNow.AddHours(-3),
            EndTime = DateTime.UtcNow.AddHours(-1),
            Description = "Manager entry for approval"
        });
        var managerSubmitted = await _approvalService.SubmitAsync(_manager, managerEntry);
        Assert.Equal(TimeEntryStatus.Pending, managerSubmitted.Status);

        // Owner approves manager's entry
        var approved = await _approvalService.ApproveAsync(_owner, managerSubmitted);
        Assert.Equal(TimeEntryStatus.Approved, approved.Status);

        // Unapprove back to draft
        var unapproved = await _approvalService.UnapproveAsync(_owner, approved);
        Assert.Equal(TimeEntryStatus.Draft, unapproved.Status);
    }
}
