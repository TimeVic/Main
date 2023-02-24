using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Tag;

namespace TimeTracker.Web.Shared.Components.Form;

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
    
    private RadzenDropDown<bool?> _listReference;

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
}
