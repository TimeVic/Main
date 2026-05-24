using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Parts;

public partial class TaskEstimateProgressBlock
{
    [Parameter]
    public TimeSpan? PlannedDuration { get; set; }

    [Parameter]
    public TimeSpan TrackedDuration { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public bool ShowLabels { get; set; } = true;

    [Parameter]
    public string HeightClass { get; set; } = "h-2.5";

    private TaskEstimateAnalytics Analytics => new(PlannedDuration, TrackedDuration);

    private string ProgressPercentText => Analytics.HasEstimate ? $"{Analytics.RoundedProgressPercent}%" : DashboardLocalizer["NoEstimate"].Value;

    private string ProgressWidthStyle =>
        $"{Analytics.ProgressWidthPercent.ToString("0.##", CultureInfo.InvariantCulture)}%";

    private string StatusLabel
    {
        get
        {
            var localized = DashboardLocalizer[$"TaskEstimateStatus_{Analytics.Status}"];
            return localized.ResourceNotFound ? Analytics.Status.ToLabel() : localized.Value;
        }
    }

    private string BarClass => Analytics.Status.ToBarClass();

    private string StatusTextClass => Analytics.Status.ToTextClass();

    private string PercentTextClass => StatusTextClass;
}
