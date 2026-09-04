using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.List;
using TimeTracker.Client.Core.Store.List.Currency;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class CurrenciesSelect : IDisposable
{
    [Inject]
    public IState<CurrencyState> _state { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Placeholder = DashboardLocalizer["SelectCurrency"].Value;
        Dispatcher.Dispatch(new LoadListAction());
        _state.StateChanged += UpdateList;
        UpdateList();
    }

    private void UpdateList(object? sender, EventArgs e)
    {
        UpdateList();
    }

    private void UpdateList()
    {
        _list = _state.Value.List.ToList();
        UpdateSelectedItem();
        InvokeAsync(StateHasChanged);
    }

    protected override void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
        StateHasChanged();
    }

    private void OnCurrencySelected(CurrencyDto? item)
    {
        OnValueChanged(item);
    }

    public new void Dispose()
    {
        _state.StateChanged -= UpdateList;
        base.Dispose();
    }
}
