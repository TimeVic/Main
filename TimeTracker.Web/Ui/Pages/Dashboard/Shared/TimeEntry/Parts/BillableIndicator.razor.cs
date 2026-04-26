using System.Timers;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.DateTimes;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.TimeEntry.Parts;

public partial class BillableIndicator: IDisposable
{
    [Inject]
    public UserDateTimeProviderService UserDateTimeProviderService { get; set; } = null!;

    [Inject]
    public ITimeParsingService TimeParsingService { get; set; } = null!;

    [Inject]
    public IState<AuthState> AuthState { get; set; } = null!;

    [Inject]
    public IState<TimeEntryState> TimeEntryState { get; set; } = null!;
    
    private System.Timers.Timer? _timer;
    private TimeSpan _currentTrackedDuration = TimeSpan.Zero;
    private string _currentAmountLabel = string.Empty;
    private string _hourlyRateLabel = string.Empty;
    private string _durationLabel = "0:00:00";

    private TimeEntryDto? Entry => TimeEntryState.Value?.ActiveEntry;
    private bool _isActive => Entry!= null && Entry!.IsActive;

    private decimal? _hourlyRate => Entry?.HourlyRate;

    private TimeSpan _trackedDuration
    {
        get
        {
            if (Entry == null)
            {
                return TimeSpan.Zero;
            }

            var duration = Entry.IsActive
                ? UserDateTimeProviderService.GetCurrentTime() - Entry.StartTimeOffset
                : Entry.Duration;

            return duration < TimeSpan.Zero
                ? TimeSpan.Zero
                : duration;
        }
    }

    private decimal _currentAmount
    {
        get
        {
            if (!_hourlyRate.HasValue)
            {
                return 0m;
            }

            return Math.Round((decimal)_currentTrackedDuration.TotalHours * _hourlyRate.Value, 2);
        }
    }

    private string? _currencySymbol => AuthState.Value.Workspace?.Currency.Symbol;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        RefreshDisplayState();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!_isActive)
        {
            return;
        }

        InvokeAsync(() =>
        {
            RefreshDisplayState();
            StateHasChanged();
        }).Wait();
    }

    private void RefreshDisplayState()
    {
        _currentTrackedDuration = _trackedDuration;
        _durationLabel = TimeParsingService.TimeSpanToTimeString(_currentTrackedDuration, true);
        _currentAmountLabel = _currentAmount.ToMoneyFormat(_currencySymbol);
        _hourlyRateLabel = _hourlyRate.HasValue
            ? _hourlyRate.Value.ToMoneyFormat(_currencySymbol)
            : string.Empty;
    }

    public void Dispose()
    {
        if (_timer == null)
        {
            return;
        }

        _timer.Elapsed -= OnTimerElapsed;
        _timer.Dispose();
    }
}
