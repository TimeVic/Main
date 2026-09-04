using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Models;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class LanguagesSelect : AppBaseSelect
{
    [Parameter]
    public string? Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                UpdateSelectedItem();
                if (CurrentEditContext != null && FieldIdentifier.Model != null)
                {
                    CurrentEditContext.NotifyFieldChanged(FieldIdentifier);
                }
            }
        }
    }

    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<string?> OnChanged { get; set; }

    [Parameter]
    public EventCallback<LanguageItem?> SelectedItemChanged { get; set; }

    [Parameter]
    public Expression<Func<string?>>? ValueExpression { get; set; }

    private string? _value;
    private LanguageItem? _selectedItem;
    private List<LanguageItem> _languages = new();

    protected string LocalizedPlaceholder =>
        string.IsNullOrWhiteSpace(Placeholder) ? DashboardLocalizer["SelectLanguage"].Value : Placeholder;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _languages = new List<LanguageItem>
        {
            new("en", DashboardLocalizer["English"].Value, "🇺🇸"),
            new("uk-UA", DashboardLocalizer["Ukrainian"].Value, "🇺🇦")
        };
        UpdateSelectedItem();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (CurrentEditContext != null && ValueExpression != null)
        {
            FieldIdentifier = FieldIdentifier.Create(ValueExpression);
            CurrentEditContext.OnValidationStateChanged += HandleValidationStateChanged;
        }
        UpdateSelectedItem();
    }

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        StateHasChanged();
    }

    private void UpdateSelectedItem()
    {
        _selectedItem = _languages.FirstOrDefault(
            item => string.Equals(item.Code, _value, StringComparison.OrdinalIgnoreCase)
        );
    }

    private async Task OnLanguageSelected(LanguageItem? item)
    {
        _selectedItem = item;
        _value = item?.Code;
        await ValueChanged.InvokeAsync(_value);
        await OnChanged.InvokeAsync(_value);
        await SelectedItemChanged.InvokeAsync(_selectedItem);

        if (CurrentEditContext != null && FieldIdentifier.Model != null)
        {
            CurrentEditContext.NotifyFieldChanged(FieldIdentifier);
        }
    }

    public override void Dispose()
    {
        if (CurrentEditContext != null)
        {
            CurrentEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
        }
        base.Dispose();
    }
}
