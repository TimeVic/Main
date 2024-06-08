using System.Linq.Expressions;
using Fluxor;
using Microsoft.AspNetCore.Components;
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
    public Expression<Func<long>>? For { get; set; }
    
    [Parameter] 
    public bool Disabled { get; set; }
    
    [Parameter] 
    public bool Clearable { get; set; }

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
    public IState<ClientState> _state { get; set; }
    
    private long _selectedId = 0;
    private ClientDto? _selectedItem;
    
    private Task OnValueChanged(long id)
    {
        _selectedItem = _state.Value.List.FirstOrDefault(item => item.Id == id);
        SelectedItemChanged.InvokeAsync(_selectedItem);
        ValueChanged.InvokeAsync(_selectedItem?.Id ?? 0);
        return Task.CompletedTask;
    }
    
    private string ToStringFunc(long id)
    {
        var item = _state.Value.List.FirstOrDefault(item => item.Id == id);
        return item?.Name ?? string.Empty;
    }
}
