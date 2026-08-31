using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Model.TimeEntry.Approval;
using TimeTracker.Business.Common.Services.Format;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Approvals.Components;

public partial class SubmittersListBlock
{
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; } = null!;

    [Parameter]
    public IReadOnlyList<TimeEntryApprovalSubmitterDto> Submitters { get; set; } = [];

    [Parameter]
    public TimeEntryApprovalSubmitterDto? SelectedSubmitter { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public EventCallback<TimeEntryApprovalSubmitterDto> SubmitterSelected { get; set; }

    private string _searchQuery = string.Empty;

    private IEnumerable<TimeEntryApprovalSubmitterDto> FilteredSubmitters =>
        string.IsNullOrWhiteSpace(_searchQuery)
            ? Submitters
            : Submitters.Where(s =>
                s.UserName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase)
                || s.Login.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));

    private async Task OnSelectSubmitter(TimeEntryApprovalSubmitterDto submitter)
    {
        await SubmitterSelected.InvokeAsync(submitter);
    }

    private string GetCardClasses(bool isSelected)
    {
        var baseClasses = "cursor-pointer rounded-xl border p-3 transition-all duration-150";
        if (isSelected)
        {
            return $"{baseClasses} border-blue-500 bg-blue-50/70 shadow-xs ring-1 ring-blue-500/20";
        }

        return $"{baseClasses} border-slate-200 bg-white hover:border-slate-300 hover:bg-slate-50/80 shadow-2xs";
    }

    private string GetPeriodLabel(TimeEntryApprovalSubmitterDto submitter)
    {
        var weekFormat = DashboardLocalizer["Approvals_Week"].Value;
        return string.Format(
            weekFormat,
            submitter.WeekNumber,
            submitter.PeriodStartDate.ToString("dd.MM"),
            submitter.PeriodEndDate.ToString("dd.MM")
        );
    }
}
