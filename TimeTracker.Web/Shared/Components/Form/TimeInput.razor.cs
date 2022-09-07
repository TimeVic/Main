using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class TimeInput
{
    [Parameter]
    public DateTimeOffset Value
    {
        get => DateTimeOffset.Parse(_valueString);
        set
        {
            _valueString = value.ToString("HH:mm");
        }
    }

    [Parameter]
    public EventCallback<DateTimeOffset> ValueChanged { get; set; }

    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    private string _valueString;

    private void OnChangeValue(string timeString)
    {
        _valueString = _timeParsingService.FormatTime(timeString);
        ValueChanged.InvokeAsync(DateTimeOffset.Parse(_valueString));
    }
}
