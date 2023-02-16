using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Core.Helpers;
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
    public bool Clearable { get; set; } = true;

    [Parameter]
    public ICollection<long> AllowedIds { get; set; } = new List<long>();

    [Parameter]
    public long? UserId
    {
        get => _selectedItem?.User.Id;
        set
        {
            _selectedItem = _state.Value.List.FirstOrDefault(item => item.User.Id == value);
            _selectedId = _selectedItem?.Id ?? 0;
        }
    }
    
    [Inject]
    public IState<WorkspaceMembershipsState> _state { get; set; }
    
    private WorkspaceMembershipDto? _selectedItem;

    private long _selectedId = 0;
    
    private RadzenDropDown<long> _listReference;

    private ICollection<WorkspaceMembershipDto> _list
    {
        get
        {
            if (AllowedIds.Any())
            {
                return _state.Value.List
                    .Where(
                        item => AllowedIds.Any(allowedId => allowedId == item.Id)
                    ) 
                    .ToList();
            }

            return _state.Value.List;
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
