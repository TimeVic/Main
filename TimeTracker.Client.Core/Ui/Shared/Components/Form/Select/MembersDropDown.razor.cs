using Fluxor;
using LumexUI.Common;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.WorkspaceMembers;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class MembersDropDown
{
    [Parameter]
    public Size Size { get; set; } = Size.Medium;

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

    [Parameter] 
    public Variant DropDownVariant { get; set; } = Variant.Outlined;

    [Parameter] 
    public MenuVariant DropDownMenuVariant { get; set; } = MenuVariant.Outlined;

    [Parameter] 
    public ThemeColor DropDownColor { get; set; } = ThemeColor.Default;

    [Parameter]
    public InputVariant SelectVariant { get; set; } = InputVariant.Outlined;

    [Inject]
    public IState<WorkspaceMembersState> _state { get; set; }
    
    private Guid? _userId;
    private bool _isOpen;
    
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
