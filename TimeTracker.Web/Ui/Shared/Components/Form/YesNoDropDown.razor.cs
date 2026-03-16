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
    public string Placeholder { get; set; } = "Select value";
    
    [Parameter]
    public string Class { get; set; }
    
    [Inject]
    public ILogger<YesNoDropDown> _logger { get; set; }
    
    private ICollection<YesNoDropDownItem> _listItems = new List<YesNoDropDownItem>()
    {
        new(null, "Not set"),
        new(true, "Yes"),
        new(false, "No"),
    };

    private void OnValueChanged(bool? selectedValue)
    {
        InvokeAsync(async () => await ValueChanged.InvokeAsync(selectedValue));
    }
    
    private string ToStringFunc(bool? itemValue)
    {
        var item = _listItems.FirstOrDefault(item => item.Value == itemValue);
        return item.Name;
    }
}
