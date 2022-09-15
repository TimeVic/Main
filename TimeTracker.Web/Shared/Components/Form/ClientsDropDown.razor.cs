using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Client;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class ClientsDropDown
{
    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter]
    public long Value
    {
        get => _selectedId;
        set => _selectedId = value;
    }
    
    [Parameter]
    public EventCallback<long> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<ClientDto> SelectedItemChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select client";
    
    [Parameter]
    public string Class { get; set; }
    
    [Inject]
    public ILogger<ClientsDropDown> _logger { get; set; }
    
    [Inject]
    public IApiService _apiService { get; set; }
    
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    [Inject]
    public IState<ClientState> _state { get; set; }
    
    private ClientDto? _selectedItem;

    private long _selectedId = 0;
    
    private RadzenDropDown<long> _listReference;

    private Task OnValueChanged(long selectedId)
    {
        _selectedItem = _state.Value.List.FirstOrDefault(item => item.Id == selectedId);
        SelectedItemChanged.InvokeAsync(_selectedItem);
        ValueChanged.InvokeAsync(selectedId);
        return Task.CompletedTask;
    }
}
