using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Common;
using TimeTracker.Web.Core.Components;

namespace TimeTracker.Web.Shared.Components.Form.Select.Core;

public abstract class BaseSingleSelect<T>: BaseReactiveComponent where T : BaseDto
{
    [Parameter]
    public string? Label { get; set; }
    
    [Parameter] 
    public bool Disabled { get; set; }
    
    [Parameter] 
    public bool Clearable { get; set; }

    [Parameter]
    public Guid Value
    {
        get
        {
            Guid.TryParse(_selectedId, out var id);
            return id;
        }
        set
        {
            if (value.ToString() != _selectedId)
            {
                _selectedId = value.ToString();
                UpdateSelectedItem();
            }
        }
    }

    [Parameter]
    public string Placeholder { get; set; } = "Select client";
    
    [Parameter]
    public string Class { get; set; }
    
    [Parameter]
    public bool FullWidth { get; set; } = false;
    
    [Parameter]
    public EventCallback<T?> SelectedItemChanged { get; set; }
    
    [Parameter] 
    public bool Required { get; set; }
    
    [Inject]
    public ILogger<BaseSingleSelect<T>> _logger { get; set; }
    
    protected T? _selectedItem;
    protected ICollection<T> _list = new List<T>();
    protected string? _selectedId = null;
    protected string? _placeholder => _selectedItem is null ? Placeholder : null;

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

    protected void OnClear()
    {
        if (string.IsNullOrEmpty(_selectedId))
            return;
        _selectedId = null;
    }
    
    protected void OnValueChanged(T? client)
    {
        if (_selectedItem?.Id != client?.Id)
        {
            UpdateSelectedItem();
            SelectedItemChanged.InvokeAsync(_selectedItem);
        }
    }
    
    protected abstract void UpdateSelectedItem();
}
