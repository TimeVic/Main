using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class TimeZonesSelect : AppBaseSelect
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
    public EventCallback<TimeZoneInfo?> SelectedItemChanged { get; set; }

    [Parameter]
    public Expression<Func<string?>>? ValueExpression { get; set; }

    private string? _value;
    private TimeZoneInfo? _selectedItem;
    private static readonly List<TimeZoneInfo> _timeZones = TimeZoneInfo.GetSystemTimeZones()
        .OrderBy(tz => tz.BaseUtcOffset)
        .ToList();

    protected string LocalizedPlaceholder =>
        string.IsNullOrWhiteSpace(Placeholder) ? DashboardLocalizer["SelectTimeZone"].Value : Placeholder;

    protected override void OnInitialized()
    {
        base.OnInitialized();
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
        _selectedItem = _timeZones.FirstOrDefault(
            item => string.Equals(item.Id, _value, StringComparison.OrdinalIgnoreCase)
        );
    }

    private async Task OnTimeZoneSelected(TimeZoneInfo? item)
    {
        _selectedItem = item;
        _value = item?.Id;
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
