using System.Timers;
using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Services.Format;

namespace TimeTracker.Web.Ui.Pages.Dashboard.TimeEntry.Parts;

public partial class TimeCounterField: IDisposable
{
    [Parameter]
    public bool IsActive { get; set; } = true;

    [Parameter]
    public DateTime? Value
    {
        get => _startTime;
        set
        {
            _startTime = value ?? DateTime.Now;
        }
    }

    [Parameter]
    public string Class { get; set; }

    private DateTime? _startTime;
    private TimeSpan _currentDuration = TimeSpan.MinValue;
    private System.Timers.Timer _timer;

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
            _currentDuration = (DateTime.Now - _startTime.Value);
        }
        StateHasChanged();
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
