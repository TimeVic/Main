using Microsoft.AspNetCore.Components;

namespace TimeTracker.Web.Ui.Shared.Components.Form;

public record struct YesNoDropDownItem(bool? Value, string Name);

public partial class YesNoDropDown
{
    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter]
    public bool? Value { get; set; }

    [Parameter]
    public EventCallback<bool?> ValueChanged { get; set; }

    [Parameter]
    public string Placeholder { get; set; } = string.Empty;
    
    [Parameter]
    public string Class { get; set; }
    
    [Inject]
    public ILogger<YesNoDropDown> _logger { get; set; }
    
    private string LocalizedPlaceholder =>
        string.IsNullOrWhiteSpace(Placeholder) ? DashboardLocalizer["Select"].Value : Placeholder;

    private ICollection<YesNoDropDownItem> ListItems =>
    [
        new(null, DashboardLocalizer["NotSet"].Value),
        new(true, DashboardLocalizer["Yes"].Value),
        new(false, DashboardLocalizer["No"].Value)
    ];

    private void OnValueChanged(bool? selectedValue)
    {
        InvokeAsync(async () => await ValueChanged.InvokeAsync(selectedValue));
    }
    
    private string ToStringFunc(bool? itemValue)
    {
        var item = ListItems.FirstOrDefault(item => item.Value == itemValue);
        return item.Name;
    }
}
