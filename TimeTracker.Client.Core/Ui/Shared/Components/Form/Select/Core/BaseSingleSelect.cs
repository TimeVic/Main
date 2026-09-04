using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Common;
using TimeTracker.Client.Core.Constants.Ui;
using TimeTracker.Client.Core.Core.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

public abstract class BaseSingleSelect<T>: BaseReactiveComponent, IDisposable where T : BaseDto, new()
{
    [CascadingParameter]
    protected EditContext? CurrentEditContext { get; set; }
    
    [Parameter]
    public DropDownType Type { get; set; } = DropDownType.DropDown;

    [Parameter]
    public SelectSize Size { get; set; } = SelectSize.Medium;

    [Parameter]
    public SelectVariant Variant { get; set; } = SelectVariant.Input;

    protected SelectVariant ResolvedVariant => Type switch
    {
        DropDownType.DropDown => SelectVariant.Button,
        DropDownType.Select => SelectVariant.Input,
        _ => Variant
    };
    
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
                // Notify EditContext that the field has changed
                if (CurrentEditContext != null && FieldIdentifier.Model != null)
                {
                    CurrentEditContext.NotifyFieldChanged(FieldIdentifier);
                }
            }
        }
    }

    [Parameter]
    public Expression<Func<Guid>>? ValueExpression { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select item";
    
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
    protected readonly T _clearSentinel = new T { Id = Guid.Empty };
    
    protected FieldIdentifier FieldIdentifier;
    
    protected bool IsInvalid => CurrentEditContext != null 
                                 && FieldIdentifier.Model != null 
                                 && CurrentEditContext.GetValidationMessages(FieldIdentifier).Any();

    protected string SelectClass
    {
        get
        {
            var classList = new List<string>();
            if (FullWidth && Clearable)
                classList.Add("w-select-w-100");
            else if (FullWidth)
                classList.Add("w-100");
            
            if (IsInvalid)
            {
                classList.Add("invalid");
            }
            
            return string.Join(" ", classList);
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (CurrentEditContext != null && ValueExpression != null)
        {
            FieldIdentifier = FieldIdentifier.Create(ValueExpression);
            CurrentEditContext.OnValidationStateChanged += HandleValidationStateChanged;
        }
    }

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        StateHasChanged();
    }

    protected void OnClear()
    {
        OnValueChanged(null);
    }
    
    protected void OnValueChanged(T? item)
    {
        if (item?.Id == Guid.Empty)
            item = null;
        _selectedId = item?.Id.ToString();
        if (_selectedItem != item)
        {
            UpdateSelectedItem();
            SelectedItemChanged.InvokeAsync(_selectedItem);
            
            // Notify EditContext that the field has changed
            if (CurrentEditContext != null && FieldIdentifier.Model != null)
            {
                CurrentEditContext.NotifyFieldChanged(FieldIdentifier);
            }
        }
    }
    
    protected abstract void UpdateSelectedItem();

    public void Dispose()
    {
        if (CurrentEditContext != null)
        {
            CurrentEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
        }
    }
}
