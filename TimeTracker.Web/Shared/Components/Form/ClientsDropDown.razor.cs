using System.Linq.Expressions;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Client;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class ClientsDropDown
{
    [Parameter]
    public string? Label { get; set; }
    
    [Parameter] 
    public bool Disabled { get; set; }
    
    [Parameter] 
    public bool Clearable { get; set; }

    [Parameter]
    public long Value
    {
        get
        {
            long.TryParse(_selectedId, out var id);
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
    public EventCallback<ClientDto> SelectedItemChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select client";
    
    [Parameter]
    public string Class { get; set; }
    
    [Parameter]
    public bool FullWidth { get; set; } = false;
    
    [Inject]
    public ILogger<ClientsDropDown> _logger { get; set; }
    
    [Inject]
    public IState<ClientState> _state { get; set; }
    
    private ClientDto? _selectedItem;
    private ICollection<ClientDto> _list = new List<ClientDto>();
    private string? _selectedId = null;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }
    
    private void OnValueChanged(ClientDto? client)
    {
        if (_selectedItem?.Id != client?.Id)
        {
            UpdateSelectedItem();
            SelectedItemChanged.InvokeAsync(_selectedItem);
        }
    }
    
    private void UpdateList()
    {
        _list = _state.Value.List;
    }
    
    private void OnClear()
    {
        if (string.IsNullOrEmpty(_selectedId))
            return;
        _selectedId = null;
    }
    
    private void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
    }
}
