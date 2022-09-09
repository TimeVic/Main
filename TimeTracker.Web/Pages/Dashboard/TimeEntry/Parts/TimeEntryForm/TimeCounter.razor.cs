using System.Timers;
using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Pages.Dashboard.TimeEntry.Parts.TimeEntryForm;

public partial class TimeCounter: IDisposable
{
    [Parameter]
    public bool IsActive { get; set; } = true;

    [Parameter]
    public DateTime? Value
    {
        get => _startTime;
        set
        {
            _startTime = value ?? DateTime.UtcNow.StartOfDay();
            _compensationDiff = DateTime.UtcNow < _startTime.Value ? (_startTime.Value - DateTime.UtcNow).Seconds : 0;
        }
    }

    [Parameter]
    public string Class { get; set; }

    private DateTime? _startTime;
    private TimeSpan _currentDuration = TimeSpan.MinValue;
    private System.Timers.Timer _timer;
    
    //compensation for the difference between server and client time
    private int _compensationDiff = 0;

    private string _durationString
    {
        get
        {
            var duration = _currentDuration;
            if (!IsActive)
            {
                duration = TimeSpan.Zero;
            }
            return _timeParsingService.TimeSpanToTimeString(duration, true);
        }
    }

    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _timer = new System.Timers.Timer();
        _timer.Interval = 300;
        _timer.Elapsed += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        if (_startTime != null)
        {
            _currentDuration = (DateTime.UtcNow - _startTime.Value).Add(TimeSpan.FromSeconds(_compensationDiff));
        }
        StateHasChanged();
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
