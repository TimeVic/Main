namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks.Parts;

public sealed class TaskEstimateAnalytics
{
    public TaskEstimateAnalytics(TimeSpan? plannedDuration, TimeSpan trackedDuration)
    {
        PlannedDuration = plannedDuration;
        TrackedDuration = trackedDuration;
    }

    public TimeSpan? PlannedDuration { get; }

    public TimeSpan TrackedDuration { get; }

    public bool HasEstimate => PlannedDuration.HasValue && PlannedDuration.Value > TimeSpan.Zero;

    public decimal ProgressPercent =>
        HasEstimate
            ? (decimal)TrackedDuration.TotalSeconds / (decimal)PlannedDuration!.Value.TotalSeconds * 100m
            : 0m;

    public decimal ProgressWidthPercent => Math.Min(Math.Max(ProgressPercent, 0m), 100m);

    public int RoundedProgressPercent => (int)Math.Round(ProgressPercent, MidpointRounding.AwayFromZero);

    public TaskEstimateStatus Status =>
        !HasEstimate
            ? TaskEstimateStatus.NoEstimate
            : ProgressPercent switch
            {
                < 75m => TaskEstimateStatus.OnTrack,
                < 100m => TaskEstimateStatus.NearEstimate,
                < 125m => TaskEstimateStatus.OverEstimate,
                _ => TaskEstimateStatus.HighOverrun
            };

    public TimeSpan DeltaDuration => HasEstimate ? TrackedDuration - PlannedDuration!.Value : TimeSpan.Zero;

    public TimeSpan AbsoluteDeltaDuration => DeltaDuration.Duration();

    public bool IsOverEstimate => HasEstimate && DeltaDuration > TimeSpan.Zero;

    public bool HasDelta => HasEstimate && DeltaDuration != TimeSpan.Zero;

    public TimeSpan RemainingDuration => !HasEstimate || DeltaDuration >= TimeSpan.Zero
        ? TimeSpan.Zero
        : DeltaDuration.Duration();

    public TimeSpan OverrunDuration => !HasEstimate || DeltaDuration <= TimeSpan.Zero
        ? TimeSpan.Zero
        : DeltaDuration;
}

public enum TaskEstimateStatus
{
    NoEstimate = 0,
    OnTrack = 1,
    NearEstimate = 2,
    OverEstimate = 3,
    HighOverrun = 4
}

public static class TaskEstimateStatusExtensions
{
    public static string ToLabel(this TaskEstimateStatus status)
    {
        return status switch
        {
            TaskEstimateStatus.OnTrack => "On track",
            TaskEstimateStatus.NearEstimate => "Near estimate",
            TaskEstimateStatus.OverEstimate => "Over estimate",
            TaskEstimateStatus.HighOverrun => "High overrun",
            _ => "No estimate"
        };
    }

    public static string ToTextClass(this TaskEstimateStatus status)
    {
        return status switch
        {
            TaskEstimateStatus.OnTrack => "text-emerald-700",
            TaskEstimateStatus.NearEstimate => "text-amber-700",
            TaskEstimateStatus.OverEstimate => "text-orange-700",
            TaskEstimateStatus.HighOverrun => "text-rose-700",
            _ => "text-slate-500"
        };
    }

    public static string ToBarClass(this TaskEstimateStatus status)
    {
        return status switch
        {
            TaskEstimateStatus.OnTrack => "bg-emerald-500",
            TaskEstimateStatus.NearEstimate => "bg-amber-500",
            TaskEstimateStatus.OverEstimate => "bg-orange-500",
            TaskEstimateStatus.HighOverrun => "bg-rose-500",
            _ => "bg-slate-300"
        };
    }

    public static string ToBadgeClass(this TaskEstimateStatus status)
    {
        return status switch
        {
            TaskEstimateStatus.OnTrack => "bg-emerald-50 text-emerald-700 ring-1 ring-emerald-200",
            TaskEstimateStatus.NearEstimate => "bg-amber-50 text-amber-700 ring-1 ring-amber-200",
            TaskEstimateStatus.OverEstimate => "bg-orange-50 text-orange-700 ring-1 ring-orange-200",
            TaskEstimateStatus.HighOverrun => "bg-rose-50 text-rose-700 ring-1 ring-rose-200",
            _ => "bg-slate-100 text-slate-600 ring-1 ring-slate-200"
        };
    }
}
