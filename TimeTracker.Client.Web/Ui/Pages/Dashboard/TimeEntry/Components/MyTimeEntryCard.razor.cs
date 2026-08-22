using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Store.Auth;
using Fluxor;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.TimeEntry.Components;

public partial class MyTimeEntryCard
{
    [Parameter]
    public string CurrentTimeZone { get; set; } = string.Empty;

    [Parameter]
    public TimeEntryDto Entry { get; set; } = new();

    [Parameter]
    public EventCallback<TimeEntryDto> OnEdit { get; set; }

    [Parameter]
    public EventCallback<TimeEntryDto> OnDelete { get; set; }

    [Parameter]
    public EventCallback<TimeEntryDto> OnClone { get; set; }

    [Parameter]
    public EventCallback<TimeEntryDto> OnOpenTask { get; set; }

    [Parameter]
    public EventCallback<TimeEntryDto> OnSubmitForApproval { get; set; }

    [Inject]
    private ITimeParsingService _timeParsingService { get; set; } = null!;

    [Inject]
    private IState<AuthState> _authState { get; set; } = null!;

    private string? _currencySymbol => _authState.Value.Workspace?.Currency.Symbol;

    private bool IsApprovalsEnabled => _authState.Value.Workspace?.IsApprovalsEnabled ?? false;

    private bool CanEditOrDelete => !IsApprovalsEnabled || Entry.Status is not (TimeEntryStatus.Approved or TimeEntryStatus.Pending);

    private bool CanSubmitForApproval => IsApprovalsEnabled && Entry.Status is TimeEntryStatus.Draft or TimeEntryStatus.Rejected;

    private string GetProjectLabel()
    {
        return string.IsNullOrWhiteSpace(Entry.Project?.Name) ? DashboardLocalizer["NoProject"].Value : Entry.Project.Name;
    }

    private string GetDescriptionOrTaskTitle()
    {
        if (!string.IsNullOrWhiteSpace(Entry.Description))
            return Entry.Description.TruncateAndAddDots(120);

        if (!string.IsNullOrWhiteSpace(Entry.Task?.Title))
            return Entry.Task.Title.TruncateAndAddDots(120);

        return string.Empty;
    }

    private string GetBillableAmountLabel()
    {
        if (!Entry.HourlyRate.HasValue)
            return string.Empty;

        var amount = Math.Round((decimal)Entry.Duration.TotalHours * Entry.HourlyRate.Value, 2);
        return amount.ToMoneyFormat(_currencySymbol);
    }

    private string GetHourlyRateLabel()
    {
        if (!Entry.HourlyRate.HasValue)
            return string.Empty;

        return Entry.HourlyRate.Value.ToMoneyFormat(_currencySymbol);
    }
}
