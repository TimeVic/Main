using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks.Parts;

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

    private decimal ProgressPercent =>
        HasEstimate
            ? (decimal)TrackedDuration.TotalSeconds / (decimal)PlannedDuration!.Value.TotalSeconds * 100m
            : 0m;

    private string ProgressPercentText => HasEstimate ? $"{Math.Round(ProgressPercent)}%" : "No estimate";

    private string ProgressWidthStyle =>
        $"{Math.Min(Math.Max(ProgressPercent, 0m), 100m).ToString("0.##", CultureInfo.InvariantCulture)}%";

    private bool HasEstimate => PlannedDuration.HasValue && PlannedDuration.Value > TimeSpan.Zero;

    private string StatusLabel
    {
        get
        {
            if (!HasEstimate)
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

    private string BarClass
    {
        get
        {
            if (!HasEstimate)
            {
                return "bg-slate-300";
            }

            return ProgressPercent switch
            {
                < 80m => "bg-emerald-500",
                <= 100m => "bg-amber-500",
                <= 130m => "bg-orange-500",
                _ => "bg-rose-500"
            };
        }
    }

    private string StatusTextClass
    {
        get
        {
            if (!HasEstimate)
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

    private string PercentTextClass => StatusTextClass;
}
