using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Store.TimeEntry;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.TimeEntry.Components;

public partial class TimeEntryApprovalBannerBlock
{
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; } = null!;

    [Parameter]
    public EventCallback OnSubmitted { get; set; }

    private TimeEntryApprovalStatusSummaryDto? _summary;
    private bool _isSubmitting;

    private bool IsApprovalsEnabled => AuthState.Value.Workspace?.IsApprovalsEnabled ?? false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await RefreshStatusAsync();
    }

    public async Task RefreshStatusAsync()
    {
        if (!IsApprovalsEnabled) return;

        try
        {
            _summary = await ApiService.TimeEntryApprovalGetStatusAsync();
            StateHasChanged();
        }
        catch
        {
            // Ignore background error
        }
    }

    private string GetBannerText()
    {
        if (_summary == null) return string.Empty;

        var template = DashboardLocalizer["Approvals_Banner_Text"].Value;
        var durationStr = _timeParsingService.TimeSpanToTimeString(_summary.DraftDuration, true);
        var symbol = AuthState.Value.Workspace?.Currency?.Symbol ?? "$";
        var amountStr = $"{symbol}{_summary.DraftAmount:0.00}";

        return string.Format(template, durationStr, amountStr);
    }

    private async Task OnSubmitForApproval()
    {
        _isSubmitting = true;
        try
        {
            var now = DateTime.UtcNow;
            var startDate = now.StartOfWeek();
            var endDate = startDate.AddDays(6).EndOfDay();

            await ApiService.TimeEntryApprovalSubmitPeriodAsync(startDate, endDate);
            await RefreshStatusAsync();
            Dispatcher.Dispatch(new LoadListAction());
            await OnSubmitted.InvokeAsync();
        }
        catch
        {
            // Handled
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}
