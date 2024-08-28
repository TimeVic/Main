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
        get => _selectedId;
        set
        {
            if (_selectedId != value)
            {
                _selectedId = value;
            }
        }
    }

    [Parameter]
    public EventCallback<ClientDto> SelectedItemChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select client";
    
    [Parameter]
    public string Class { get; set; }
    
    [Inject]
    public ILogger<ClientsDropDown> _logger { get; set; }
    
    [Inject]
    public IState<ClientState> _state { get; set; }
    
    private ClientDto? _selectedItem => _list.FirstOrDefault(item => item.Id == _selectedId);
    private ICollection<ClientDto> _list = new List<ClientDto>();
    private long _selectedId = 0;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }
    
    private void OnValueChanged(string? project)
    {
        long.TryParse(project, out _selectedId);
        SelectedItemChanged.InvokeAsync(_selectedItem);
    }
    
    private void UpdateList()
    {
        _list = _state.Value.List;
    }
}
