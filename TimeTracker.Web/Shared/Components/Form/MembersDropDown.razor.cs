using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.Workspace;
using TimeTracker.Web.Store.WorkspaceMemberships;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class MembersDropDown
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
    public EventCallback<WorkspaceMembershipDto> SelectedItemChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select user";
    
    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public long UserId { get; set; }

    [Inject]
    public ILogger<MembersDropDown> _logger { get; set; }
    
    [Inject]
    public IApiService _apiService { get; set; }
    
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    [Inject]
    public IState<WorkspaceMembershipsState> _state { get; set; }
    
    private WorkspaceMembershipDto? _selectedItem;

    private long _selectedId = 0;
    
    private RadzenDropDown<long> _listReference;

    private ICollection<WorkspaceMembershipDto> _list
    {
        get
        {
            var list = _state.Value.List;
            if (UserId == 0)
            {
                return list;
            }

            return list.Where(item => item.User.Id == UserId).ToList();
        }
    }

    private Task OnValueChanged(long selectedId)
    {
        _selectedItem = _state.Value.List.FirstOrDefault(item => item.Id == selectedId);
        SelectedItemChanged.InvokeAsync(_selectedItem);
        ValueChanged.InvokeAsync(selectedId);
        return Task.CompletedTask;
    }
}
