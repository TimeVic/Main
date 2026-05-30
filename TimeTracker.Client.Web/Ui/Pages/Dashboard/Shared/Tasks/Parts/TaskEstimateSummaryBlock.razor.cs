using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Services.Format;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Parts;

public partial class TaskEstimateSummaryBlock
{
    [Parameter]
    public TimeSpan? PlannedDuration { get; set; }

    [Parameter]
    public TimeSpan TrackedDuration { get; set; }

    [Parameter]
    public bool IsCompact { get; set; }

    [Parameter]
    public bool ShowProgress { get; set; } = true;

    [Parameter]
    public ExternalSourceType ExternalSourceType { get; set; } = ExternalSourceType.Manual;

    [Parameter]
    public string EmptyEstimateText { get; set; } = string.Empty;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Inject]
    private ITimeParsingService TimeParsingService { get; set; } = null!;

    private TaskEstimateAnalytics Analytics => new(PlannedDuration, TrackedDuration);

    private string PlannedDurationText =>
        Analytics.HasEstimate
            ? TimeParsingService.TimeSpanToDurationString(PlannedDuration)
            : NoEstimateText;

    private string TrackedDurationText => TimeParsingService.TimeSpanToDurationString(TrackedDuration);

    private string CompactSummaryText =>
        Analytics.HasEstimate
            ? $"{TrackedDurationText} / {PlannedDurationText} · {ProgressPercentText} · {StatusLabel}"
            : string.Format(DashboardLocalizer["TaskEstimateSummaryBlock_NoEstimateTracked"], TrackedDurationText);

    private string ProgressPercentText => Analytics.HasEstimate ? $"{Analytics.RoundedProgressPercent}%" : NoEstimateText;

    private string StatusLabel => LocalizeStatus(Analytics.Status);

    private string StatusTextClass => Analytics.Status.ToTextClass();

    private string StatusBadgeClass => Analytics.Status.ToBadgeClass();

    private string RemainingOrOverText
    {
        get
        {
            if (!Analytics.HasEstimate)
            {
                return NoEstimateText;
            }

            if (Analytics.IsOverEstimate)
            {
                return string.Format(DashboardLocalizer["TaskEstimateSummaryBlock_Over"], FormatDuration(Analytics.OverrunDuration));
            }

            if (Analytics.RemainingDuration > TimeSpan.Zero)
            {
                return string.Format(DashboardLocalizer["TaskEstimateSummaryBlock_Remaining"], FormatDuration(Analytics.RemainingDuration));
            }

            return DashboardLocalizer["TaskEstimateStatus_OnTrack"].Value;
        }
    }

    private string RemainingOrOverTextClass =>
        !Analytics.HasEstimate
            ? "text-slate-500"
            : Analytics.IsOverEstimate
                ? "text-rose-700"
                : "text-emerald-700";

    private string SectionDescription =>
        ExternalSourceType == ExternalSourceType.Jira && Analytics.HasEstimate
            ? DashboardLocalizer["TaskEstimateSummaryBlock_JiraEstimateDescription"].Value
            : DashboardLocalizer["TaskEstimateSummaryBlock_ManualEstimateDescription"].Value;

    private string FormatDuration(TimeSpan duration) => TimeParsingService.TimeSpanToDurationString(duration);

    private string NoEstimateText =>
        string.IsNullOrWhiteSpace(EmptyEstimateText) ? DashboardLocalizer["NoEstimate"].Value : EmptyEstimateText;

    private string LocalizeStatus(TaskEstimateStatus status)
    {
        var localized = DashboardLocalizer[$"TaskEstimateStatus_{status}"];
        return localized.ResourceNotFound ? status.ToLabel() : localized.Value;
    }
}
