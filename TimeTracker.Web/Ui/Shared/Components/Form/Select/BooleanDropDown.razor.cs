using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Constants.Ui;

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
    public bool Clearable { get; set; } = true;

    [Parameter]
    public bool FullWidth { get; set; }

    [Parameter]
    public DropDownType SelectType { get; set; } = DropDownType.DropDown;

    private bool _isOpen;

    private Task OnOpenChanged(bool isOpen)
    {
        _isOpen = isOpen;
        return Task.CompletedTask;
    }

    private async Task OnItemSelected(bool? value)
    {
        _isOpen = false;
        await InvokeAsync(StateHasChanged);
        await Task.Yield();
        await ValueChanged.InvokeAsync(value);
    }

    private Task OnSelectValueChanged(bool? value) => ValueChanged.InvokeAsync(value);
}
