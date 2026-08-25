using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.TimeEntry.Approval;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Approvals.Components;

public partial class ApprovalDetailsBlock
{
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; } = null!;

    [Parameter]
    public GetApprovalDetailsResponse? Details { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool IsActionProcessing { get; set; }

    [Parameter]
    public EventCallback<ICollection<Guid>> ApproveRequested { get; set; }

    [Parameter]
    public EventCallback<ICollection<Guid>> RejectRequested { get; set; }

    [Parameter]
    public EventCallback<ICollection<Guid>> UnapproveRequested { get; set; }

    private readonly HashSet<string> _expandedTaskKeys = [];
    private Guid? _lastLoadedUserId;
    private DateTime? _lastLoadedPeriodStart;

    private ICollection<Guid> AllEntryIds =>
        Details?.Projects.SelectMany(p => p.Tasks).SelectMany(t => t.Entries).Select(e => e.Id).ToList() ?? [];

    private ICollection<Guid> ApprovedEntryIds =>
        Details?.Projects.SelectMany(p => p.Tasks).SelectMany(t => t.Entries)
            .Where(e => e.Status == TimeEntryStatus.Approved)
            .Select(e => e.Id).ToList() ?? [];

    private bool HasPendingOrDraftEntries =>
        Details?.Projects.SelectMany(p => p.Tasks).SelectMany(t => t.Entries)
            .Any(e => e.Status is TimeEntryStatus.Pending or TimeEntryStatus.Draft or TimeEntryStatus.Rejected) ?? false;

    private bool HasApprovedEntries =>
        Details?.Projects.SelectMany(p => p.Tasks).SelectMany(t => t.Entries)
            .Any(e => e.Status == TimeEntryStatus.Approved) ?? false;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (Details != null)
        {
            var isDifferentSubmitterOrPeriod = Details.UserId != _lastLoadedUserId
                || Details.PeriodStartDate != _lastLoadedPeriodStart;

            if (isDifferentSubmitterOrPeriod)
            {
                _lastLoadedUserId = Details.UserId;
                _lastLoadedPeriodStart = Details.PeriodStartDate;
                _expandedTaskKeys.Clear();

                foreach (var project in Details.Projects)
                {
                    foreach (var task in project.Tasks)
                    {
                        _expandedTaskKeys.Add($"{project.ProjectId}_{task.TaskId}_{task.Title}");
                    }
                }
            }
        }
    }

    private bool IsTaskExpanded(string key) => _expandedTaskKeys.Contains(key);

    private void ToggleTaskExpansion(string key)
    {
        if (!_expandedTaskKeys.Add(key))
        {
            _expandedTaskKeys.Remove(key);
        }
    }

    private string GetPeriodLabel()
    {
        if (Details == null) return string.Empty;
        var weekFormat = DashboardLocalizer["Approvals_Week"].Value;
        var weekNumber = Details.PeriodStartDate.GetIso8601WeekOfYear();
        return string.Format(
            weekFormat,
            weekNumber,
            Details.PeriodStartDate.ToString("dd.MM"),
            Details.PeriodEndDate.ToString("dd.MM")
        );
    }

    private string GetBulkApproveButtonLabel()
    {
        if (Details == null) return DashboardLocalizer["Approvals_BulkApprove"].Value;
        var template = DashboardLocalizer["Approvals_BulkApprove"].Value;
        var hoursStr = _timeParsingService.TimeSpanToTimeString(Details.TotalDuration, true);
        return string.Format(template, hoursStr);
    }

    private async Task OnApproveEntries(ICollection<Guid> entryIds)
    {
        if (IsActionProcessing) return;
        if (entryIds.Any())
        {
            await ApproveRequested.InvokeAsync(entryIds);
        }
    }

    private async Task OnOpenRejectModal(ICollection<Guid> entryIds)
    {
        if (IsActionProcessing) return;
        if (entryIds.Any())
        {
            await RejectRequested.InvokeAsync(entryIds);
        }
    }

    private async Task OnUnapproveEntries(ICollection<Guid> entryIds)
    {
        if (IsActionProcessing) return;
        if (entryIds.Any())
        {
            await UnapproveRequested.InvokeAsync(entryIds);
        }
    }
}
