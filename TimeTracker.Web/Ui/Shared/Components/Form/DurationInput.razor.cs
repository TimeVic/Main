using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using LumexUI.Common;
using TimeTracker.Business.Common.Services.Format;

namespace TimeTracker.Web.Ui.Shared.Components.Form;

public partial class DurationInput : InputBase<TimeSpan?>
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public InputVariant Variant { get; set; } = InputVariant.Outlined;

    [Parameter]
    public ThemeColor Color { get; set; } = ThemeColor.Default;

    [Parameter]
    public bool FullWidth { get; set; }

    [Parameter]
    public string Placeholder { get; set; } = string.Empty;

    [Parameter]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string WrapperClass { get; set; } = string.Empty;

    [Parameter]
    public string? Hint { get; set; }

    [Parameter]
    public Size Size { get; set; } = Size.Medium;

    [Parameter]
    public bool Clearable { get; set; } = true;

    [Parameter]
    public RenderFragment? StartContent { get; set; }

    [Parameter]
    public RenderFragment? EndContent { get; set; }

    [Parameter]
    public LabelPlacement LabelPlacement { get; set; } = LabelPlacement.Outside;
    
    [Inject]
    private ITimeParsingService TimeParsingService { get; set; } = null!;

    private bool HasError => EditContext?.GetValidationMessages(FieldIdentifier).Any() ?? false;

    private string FirstError =>
        EditContext?.GetValidationMessages(FieldIdentifier).FirstOrDefault() ?? string.Empty;

    private Task OnValueChanged(string? value)
    {
        CurrentValueAsString = value ?? string.Empty;
        return Task.CompletedTask;
    }

    protected override string? FormatValueAsString(TimeSpan? value)
    {
        return TimeParsingService.TimeSpanToDurationString(value);
    }

    protected override bool TryParseValueFromString(
        string? value,
        out TimeSpan? result,
        out string? validationErrorMessage)
    {
        if (TimeParsingService.TryParseDuration(value, out result))
        {
            validationErrorMessage = null;
            return true;
        }

        validationErrorMessage = DashboardLocalizer["DurationInput_InvalidFormat"];
        return false;
    }
}
