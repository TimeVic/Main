using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.WorkspaceMembers;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class MembersSelect
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

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Placeholder = DashboardLocalizer["SelectUser"].Value;

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
                .Where(item => AllowedIds.Any(allowedId => allowedId == item.Id))
                .ToList();
        }
        else
        {
            _list = _state.Value.List;
        }

        UpdateSelectedItem();
        InvokeAsync(StateHasChanged);
    }

    protected override void UpdateSelectedItem()
    {
        if (!string.IsNullOrEmpty(_selectedId) && _selectedId != Guid.Empty.ToString())
        {
            _selectedItem = _list.FirstOrDefault(item => item.Id.ToString() == _selectedId);
            if (_selectedItem != null)
            {
                _userId = _selectedItem.User.Id;
            }
        }
        else if (_userId.HasValue && _userId.Value != Guid.Empty)
        {
            _selectedItem = _list.FirstOrDefault(item => item.User.Id == _userId.Value);
            if (_selectedItem != null)
            {
                _selectedId = _selectedItem.Id.ToString();
            }
        }
        else
        {
            _selectedItem = null;
        }
    }

    private void OnMemberSelected(WorkspaceMemberDto? member)
    {
        _userId = member?.Id == Guid.Empty ? null : member?.User.Id;
        OnValueChanged(member);
    }
}
