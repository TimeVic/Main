using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.List.Currency;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Select;

public partial class CurrenciesDropDown: IDisposable
{   
    [Parameter]
    public bool ShowProjectsWithoutClients { get; set; } = true;
    
    [Inject]
    public IState<CurrencyState> _state { get; set; }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Placeholder = "Select project";
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
    }
    
    protected override void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
    }

    public void Dispose()
    {
        _state.StateChanged -= UpdateList;
    }
}
