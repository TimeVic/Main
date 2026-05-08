using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.WorkspaceMembers;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Select;

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
    public IState<WorkspaceMembersState> _state { get; set; }
    
    private Guid? _userId;
    private bool _isOpen;
    
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

    private Task OnOpenChanged(bool isOpen)
    {
        _isOpen = isOpen;
        return Task.CompletedTask;
    }

    private async Task OnMemberSelected(WorkspaceMemberDto? member)
    {
        _isOpen = false;
        await InvokeAsync(StateHasChanged);
        await Task.Yield();
        OnMemberValueChanged(member);
    }

    private void OnMemberValueChanged(WorkspaceMemberDto? member)
    {
        _userId = member?.Id == Guid.Empty ? null : member?.User.Id;
        OnValueChanged(member);
    }
}
