using Microsoft.AspNetCore.Components;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class EnumDropDown<TItem>
{
    [Parameter]
    public TItem Value
    {
        get => _value;
        set => _value = value;
    }

    [Parameter]
    public EventCallback<TItem> ValueChanged { get; set; }

    [Parameter]
    public string Placeholder { get; set; } = "Select item";
    
    [Parameter]
    public string Class { get; set; }
    
    [Parameter]
    public string Style { get; set; }

    [Parameter]
    public bool AllowClear { get; set; } = false;
    
    [Parameter]
    public string Name { get; set; }

    [Parameter]
    public ICollection<TItem> AllowedValues { get; set; } = new List<TItem>();
    
    private List<TItem> _list;
    private TItem _value;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _list = Enum.GetValues(typeof(TItem))
            .Cast<TItem>()
            .Where(item => AllowedValues.Count == 0 || AllowedValues.Contains(item))
            .ToList();
    }
    
    private void OnItemSelected(TItem level)
    {
        _value = level;
        ValueChanged.InvokeAsync(_value);
    }
}
