using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Store.WorkspaceMembers;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Select;

public partial class MultipleUsersDropDown
{
    [Parameter] 
    public string Label { get; set; }
    
    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter]
    public IEnumerable<Guid> Value
    {
        get => _selectedIds;
        set => _selectedIds = value;
    }

    [Parameter]
    public EventCallback<IEnumerable<Guid>> ValueChanged { get; set; }

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
            else
            {
                _allowedIds = new List<Guid>();
            }
            UpdateList();
        }
    }
    
    [Inject] 
    private ISecurityManager _securityManager { get; set; }
    
    [Inject]
    public IState<WorkspaceMembersState> _state { get; set; }
    
    private ICollection<UserDto> _selectedItems => _list.Where(item => _selectedIds.Contains(item.Id)).ToList();
    private IEnumerable<Guid> _allowedIds { get; set; } = new List<Guid>();
    private IEnumerable<Guid> _selectedIds = new List<Guid>();
    private ICollection<UserDto> _list = new List<UserDto>();

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }
    
    private void OnValueChanged(IEnumerable<UserDto> selectedUsers)
    {
        _selectedIds = selectedUsers.Select(item => item.Id).ToList();
        SelectedItemChanged.InvokeAsync(_selectedItems);
        ValueChanged.InvokeAsync(_selectedIds);
    }

    private string ToStringFunc(Guid userId)
    {
        var item = _list.FirstOrDefault(item => item.Id == userId);
        return item?.Name ?? string.Empty;
    }
    
    private void UpdateList()
    {
        if (_allowedIds.Any())
        {
            _list = _state.Value.List
                .Select(item => item.User)
                .Where(
                    item => _allowedIds.Any(allowedId => allowedId == item.Id)
                ) 
                .ToList();
            return;
        }

        _list = _state.Value.List.Select(item => item.User).ToList();
    }
}
