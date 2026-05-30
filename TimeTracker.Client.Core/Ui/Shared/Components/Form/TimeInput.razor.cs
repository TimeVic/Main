using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Services.Format;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form;

public partial class TimeInput
{
    [Parameter]
    public string? Class { get; set; }
    
    [Parameter]
    public string? Label { get; set; }
    
    [Parameter]
    public string? Style { get; set; }
    
    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public TimeSpan Value
    {
        get => TimeSpan.Parse(_valueString);
        set
        {
            _valueString = _timeParsingService.TimeSpanToTimeString(value);
        }
    }

    [Parameter]
    public EventCallback<TimeSpan> ValueChanged { get; set; }

    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    private TimeSpan _value;
    private MudTimePicker _timePicker;

    private string _valueString;

    private void OnChangeValue(string timeString)
    {
        _valueString = _timeParsingService.FormatTime(timeString);
        ValueChanged.InvokeAsync(TimeSpan.Parse(_valueString));
    }
}
