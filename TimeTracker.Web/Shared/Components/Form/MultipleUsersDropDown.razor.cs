using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Store.WorkspaceMemberships;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class MultipleUsersDropDown
{
    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter]
    public IEnumerable<long> Value
    {
        get => _selectedIds;
        set => _selectedIds = value;
    }

    [Parameter]
    public EventCallback<IEnumerable<long>> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<IEnumerable<UserDto>> SelectedItemChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select user";
    
    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public bool Clearable { get; set; } = true;

    [Parameter]
    public ProjectDto? Project
    {
        set
        {
            if (value != null)
            {
                _allowedIds = _securityManager.GetMembersWhichHaveAccessToProject(value)
                    .Select(item => item.User.Id);    
            }
        }
    }
    
    [Inject] 
    private ISecurityManager _securityManager { get; set; }
    
    [Inject]
    public IState<WorkspaceMembershipsState> _state { get; set; }
    
    private IEnumerable<UserDto> _selectedItems = new List<UserDto>();
    private IEnumerable<long> _allowedIds { get; set; } = new List<long>();
    private IEnumerable<long> _selectedIds = new List<long>();

    private ICollection<UserDto> _list
    {
        get
        {
            if (_allowedIds.Any())
            {
                return _state.Value.List
                    .Select(item => item.User)
                    .Where(
                        item => _allowedIds.Any(allowedId => allowedId == item.Id)
                    ) 
                    .ToList();
            }

            return _state.Value.List.Select(item => item.User).ToList();
        }
    }

    private Task OnValueChanged(IEnumerable<long> selectedIds)
    {
        _selectedIds = selectedIds.ToList();
        _selectedItems = _list.Where(item => _selectedIds.Contains(item.Id));
        SelectedItemChanged.InvokeAsync(_selectedItems);
        ValueChanged.InvokeAsync(_selectedIds);
        return Task.CompletedTask;
    }

    private string ToStringFunc(long userId)
    {
        var item = _list.FirstOrDefault(item => item.Id == userId);
        return item?.Name ?? string.Empty;
    }
}
