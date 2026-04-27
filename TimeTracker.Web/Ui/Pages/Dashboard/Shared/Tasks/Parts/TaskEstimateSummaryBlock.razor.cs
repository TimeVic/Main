using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Services.Format;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks.Parts;

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
    public string EmptyEstimateText { get; set; } = "No estimate";

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Inject]
    private ITimeParsingService TimeParsingService { get; set; } = null!;

    private TaskEstimateAnalytics Analytics => new(PlannedDuration, TrackedDuration);

    private string PlannedDurationText =>
        Analytics.HasEstimate
            ? TimeParsingService.TimeSpanToDurationString(PlannedDuration)
            : EmptyEstimateText;

    private string TrackedDurationText => TimeParsingService.TimeSpanToDurationString(TrackedDuration);

    private string CompactSummaryText =>
        Analytics.HasEstimate
            ? $"{TrackedDurationText} / {PlannedDurationText} · {ProgressPercentText} · {StatusLabel}"
            : $"No estimate · {TrackedDurationText} tracked";

    private string ProgressPercentText => Analytics.HasEstimate ? $"{Analytics.RoundedProgressPercent}%" : "No estimate";

    private string StatusLabel => Analytics.Status.ToLabel();

    private string StatusTextClass => Analytics.Status.ToTextClass();

    private string StatusBadgeClass => Analytics.Status.ToBadgeClass();

    private string RemainingOrOverText
    {
        get
        {
            if (!Analytics.HasEstimate)
            {
                return "No estimate";
            }

            if (Analytics.IsOverEstimate)
            {
                return $"{FormatDuration(Analytics.OverrunDuration)} over";
            }

            if (Analytics.RemainingDuration > TimeSpan.Zero)
            {
                return $"{FormatDuration(Analytics.RemainingDuration)} remaining";
            }

            return "On estimate";
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
            ? "Original estimate came from Jira. Tracked time is compared against that value."
            : "Tracked time is compared against the planned time set in TimeVic.";

    private string FormatDuration(TimeSpan duration) => TimeParsingService.TimeSpanToDurationString(duration);
}
