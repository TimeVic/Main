using Microsoft.AspNetCore.Components;
using LumexUI;
using LumexUI.Common;
using TimeTracker.Client.Core.Constants.Ui;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class EnumDropDown<TItem>
{
    [Parameter]
    public DropDownType SelectType { get; set; } = DropDownType.Select;
    
    [Parameter]
    public TItem? Value
    {
        get => _value;
        set => _value = value;
    }
    
    [Parameter]
    public EventCallback<TItem?> ValueChanged { get; set; }
    
    [Parameter]
    public EventCallback<TItem?> OnChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = string.Empty;
    
    [Parameter]
    public string? Label { get; set; } = null;
    
    [Parameter]
    public string Class { get; set; }
    
    [Parameter]
    public Size Size { get; set; } = Size.Medium;
    
    [Parameter]
    public string Style { get; set; }

    [Parameter]
    public bool AllowClear { get; set; } = false;
    
    [Parameter]
    public string Name { get; set; }

    [Parameter]
    public ICollection<TItem> AllowedValues { get; set; } = new List<TItem>();

    [Parameter]
    public bool Clearable { get; set; } = false;
    
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool FullWidth { get; set; } = false;

    [Parameter] 
    public Variant DropDownVariant { get; set; } = Variant.Outlined;
    
    [Parameter] 
    public MenuVariant DropDownMenuVariant { get; set; } = MenuVariant.Outlined;
    
    [Parameter] 
    public ThemeColor DropDownColor { get; set; } = ThemeColor.Default;

    [Parameter]
    public InputVariant SelectVariant { get; set; } = InputVariant.Outlined;
    
    
    private List<TItem?> _list;
    private TItem? _value;
    public string? _placeholder => _value is null ? LocalizedPlaceholder : null;

    private string LocalizedPlaceholder =>
        string.IsNullOrWhiteSpace(Placeholder) ? DashboardLocalizer["Select"].Value : Placeholder;

    private string GetLocalizedDisplayName(TItem? item)
    {
        if (item is null)
        {
            return string.Empty;
        }

        var key = $"{typeof(TItem).Name}_{item}";
        var localized = DashboardLocalizer[key];
        return localized.ResourceNotFound ? EnumHelpers.GetDisplayName(typeof(TItem), item) : localized.Value;
    }

    protected string SelectClass
    {
        get
        {
            if (FullWidth && Clearable)
                return "w-select-w-100";
            if (FullWidth)
                return "w-100";
            return "";        }
    }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _list = Enum.GetValues(typeof(TItem))
            .Cast<TItem?>()
            .Where<TItem?>(item => AllowedValues.Count == 0 || AllowedValues.Contains(item!.Value))
            .ToList();
    }
    
    private void OnItemSelected(TItem? itemValue)
    {
        if (itemValue != null)
        {
            _value = itemValue;
        }
        else
        {
            _value = default;
        }
        ValueChanged.InvokeAsync(_value);
        OnChanged.InvokeAsync(_value);
    }
    
    private void OnClear()
    {
        if (!_value.HasValue)
            return;
        _value = null;
    }
}
