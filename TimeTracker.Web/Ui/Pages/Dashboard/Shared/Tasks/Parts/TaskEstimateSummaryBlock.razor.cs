using Microsoft.AspNetCore.Components;
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
    public string EmptyEstimateText { get; set; } = "Not set";

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Inject]
    private ITimeParsingService TimeParsingService { get; set; } = null!;

    private decimal ProgressPercent =>
        PlannedDuration.HasValue && PlannedDuration.Value > TimeSpan.Zero
            ? (decimal)TrackedDuration.TotalSeconds / (decimal)PlannedDuration.Value.TotalSeconds * 100m
            : 0m;

    private string PlannedDurationText =>
        PlannedDuration.HasValue
            ? TimeParsingService.TimeSpanToDurationString(PlannedDuration)
            : EmptyEstimateText;

    private string TrackedDurationText => TimeParsingService.TimeSpanToDurationString(TrackedDuration);

    private string StatusLabel
    {
        get
        {
            if (!PlannedDuration.HasValue || PlannedDuration.Value <= TimeSpan.Zero)
            {
                return "No estimate";
            }

            return ProgressPercent switch
            {
                < 80m => "On track",
                <= 100m => "Close to limit",
                <= 130m => "Over estimate",
                _ => "Strong overrun"
            };
        }
    }

    private string StatusTextClass
    {
        get
        {
            if (!PlannedDuration.HasValue || PlannedDuration.Value <= TimeSpan.Zero)
            {
                return "text-slate-500";
            }

            return ProgressPercent switch
            {
                < 80m => "text-emerald-700",
                <= 100m => "text-amber-700",
                <= 130m => "text-orange-700",
                _ => "text-rose-700"
            };
        }
    }
}
