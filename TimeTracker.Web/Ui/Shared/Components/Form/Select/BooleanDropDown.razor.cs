using Microsoft.AspNetCore.Components;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Select;

public partial class BooleanDropDown
{
    [Parameter]
    public bool? Value { get; set; }

    [Parameter]
    public EventCallback<bool?> ValueChanged { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string Placeholder { get; set; } = "Select";

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public bool FullWidth { get; set; }

    private void OnValueChanged(bool? value)
    {
        InvokeAsync(async () => await ValueChanged.InvokeAsync(value));
    }
}
