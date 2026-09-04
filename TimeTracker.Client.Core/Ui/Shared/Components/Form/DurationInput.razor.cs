using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.TextField.Core;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form;

public partial class DurationInput : BaseInputField<TimeSpan?>
{
    [Parameter]
    public string? Hint { get; set; }

    [Inject]
    private ITimeParsingService TimeParsingService { get; set; } = null!;

    private string _displayString = string.Empty;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _displayString = TimeParsingService.TimeSpanToDurationString(Value) ?? string.Empty;
    }

    private async Task OnStringValueChanged(string? val)
    {
        _displayString = val ?? string.Empty;

        if (string.IsNullOrWhiteSpace(val))
        {
            await SetValueAsync(null);
            return;
        }

        if (TimeParsingService.TryParseDuration(val, out var parsed))
        {
            await SetValueAsync(parsed);
        }
    }

    public override async Task ClearAsync()
    {
        _displayString = string.Empty;
        await SetValueAsync(null);
    }
}
