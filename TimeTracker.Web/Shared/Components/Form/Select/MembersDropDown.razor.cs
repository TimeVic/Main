using System.Linq.Expressions;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.WorkspaceMemberships;

namespace TimeTracker.Web.Shared.Components.Form.Select;

public partial class MembersDropDown
{
    [Parameter]
    public Guid? UserId
    {
        get => _userId;
        set
        {
            _userId = value;
            UpdateSelectedItem();
        }
    }
    
    [Parameter]
    public ICollection<Guid> AllowedIds { get; set; } = new List<Guid>();

    [Inject]
    public IState<WorkspaceMembershipsState> _state { get; set; }
    
    private Guid? _userId;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Placeholder = "Select user";

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }
    
    private void UpdateList()
    {
        if (AllowedIds.Any())
        {
            _list = _state.Value.List
                .Where(
                    item => AllowedIds.Any(allowedId => allowedId == item.Id)
                ) 
                .ToList();
        }
        else
        {
            _list = _state.Value.List;    
        }
        UpdateSelectedItem();
    }
    
    protected override void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => (_userId is not null && item.User.Id == _userId) || item.Id.ToString() == _selectedId
        );
    }
}
