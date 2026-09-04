using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Constants.Ui;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class BooleanSelect
{
    [Parameter]
    public bool? Value { get; set; }

    [Parameter]
    public EventCallback<bool?> ValueChanged { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string Placeholder { get; set; } = string.Empty;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public bool Clearable { get; set; } = true;

    [Parameter]
    public bool FullWidth { get; set; }

    [Parameter]
    public DropDownType SelectType { get; set; } = DropDownType.DropDown;

    private readonly List<bool?> _items = new() { true, false };

    protected SelectVariant ResolvedVariant => SelectType switch
    {
        DropDownType.DropDown => SelectVariant.Button,
        _ => SelectVariant.Input
    };

    private string GetDisplayText(bool? item)
    {
        if (!item.HasValue)
        {
            return string.Empty;
        }

        return item.Value ? DashboardLocalizer["Yes"] : DashboardLocalizer["No"];
    }
}
