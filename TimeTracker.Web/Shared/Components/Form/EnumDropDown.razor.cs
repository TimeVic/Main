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
    public EventCallback<TItem> OnChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select item";
    
    [Parameter]
    public string? Label { get; set; } = null;
    
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

    [Parameter]
    public bool Disabled { get; set; }
    
    private List<TItem?> _list;
    private TItem _value;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _list = Enum.GetValues(typeof(TItem))
            .Cast<TItem?>()
            .Where(item => AllowedValues.Count == 0 || AllowedValues.Contains(item!.Value))
            .ToList();
    }
    
    private void OnItemSelected(string? itemValue)
    {
        if (!string.IsNullOrEmpty(itemValue))
        {
            _value = Enum.Parse<TItem>(itemValue);
        }
        else
        {
            _value = default;
        }
        ValueChanged.InvokeAsync(_value);
        OnChanged.InvokeAsync(_value);
    }
}
