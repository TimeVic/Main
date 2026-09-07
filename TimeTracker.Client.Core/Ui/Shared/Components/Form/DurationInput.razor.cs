using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
        await CommitValueAsync();
    }

    private async Task OnInputBlur(FocusEventArgs e)
    {
        await CommitValueAsync();
    }

    private async Task CommitValueAsync()
    {
        if (string.IsNullOrWhiteSpace(_displayString))
        {
            _displayString = string.Empty;
            await SetValueAsync(null);
            return;
        }

        if (TimeParsingService.TryParseDuration(_displayString, out var parsed))
        {
            _displayString = TimeParsingService.TimeSpanToDurationString(parsed) ?? string.Empty;
            await SetValueAsync(parsed);
        }
        else
        {
            // Reset to previously valid value if input is invalid
            _displayString = TimeParsingService.TimeSpanToDurationString(Value) ?? string.Empty;
        }
    }

    public override async Task ClearAsync()
    {
        _displayString = string.Empty;
        await SetValueAsync(null);
    }
}
