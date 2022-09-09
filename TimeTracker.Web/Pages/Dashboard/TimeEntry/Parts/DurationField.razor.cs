using System.Timers;
using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Web.Pages.Dashboard.TimeEntry.Parts;

public partial class DurationField
{
    [Parameter]
    public TimeSpan Value
    {
        get => _currentDuration;
        set
        {
            _currentDuration = value;
            StateHasChanged();
        }
    }

    [Parameter]
    public string Class { get; set; }

    private TimeSpan _currentDuration = TimeSpan.MinValue;

    private string _durationString
    {
        get
        {
            var duration = _currentDuration;
            return _timeParsingService.TimeSpanToTimeString(duration, true);
        }
    }

    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
}
