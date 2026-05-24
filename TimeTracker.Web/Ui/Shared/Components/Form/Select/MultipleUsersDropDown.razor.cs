using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Services.Security;
using TimeTracker.Client.Core.Store.WorkspaceMembers;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Select;

public partial class MultipleUsersDropDown: IDisposable
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
    public string Placeholder { get; set; } = string.Empty;

    private string LocalizedPlaceholder =>
        string.IsNullOrWhiteSpace(Placeholder) ? DashboardLocalizer["SelectUser"].Value : Placeholder;
    
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
    private bool _isOpen;
    private string SelectedText => _selectedItems.Any()
        ? string.Join(", ", _selectedItems.Select(item => item.Name))
        : LocalizedPlaceholder;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _state.StateChanged += OnWorkspaceMembersChanged;
        UpdateList();
    }
    
    public void Dispose()
    {
        _state.StateChanged -= OnWorkspaceMembersChanged;
    }

    private void OnWorkspaceMembersChanged(object? sender, EventArgs args)
    {
        UpdateList();
        InvokeAsync(StateHasChanged);
    }

    private Task OnOpenChanged(bool isOpen)
    {
        _isOpen = isOpen;
        return Task.CompletedTask;
    }

    private async Task ToggleUser(UserDto user)
    {
        var selectedIds = _selectedIds.ToHashSet();
        if (!selectedIds.Add(user.Id))
        {
            selectedIds.Remove(user.Id);
        }

        await OnValueChanged(_list.Where(item => selectedIds.Contains(item.Id)));
    }

    private async Task ClearSelection()
    {
        await OnValueChanged(Array.Empty<UserDto>());
    }

    private async Task OnValueChanged(IEnumerable<UserDto> selectedUsers)
    {
        _selectedIds = selectedUsers.Select(item => item.Id).ToList();
        await SelectedItemChanged.InvokeAsync(_selectedItems);
        await ValueChanged.InvokeAsync(_selectedIds);
    }

    private string GetCheckboxClass(bool isSelected)
    {
        return string.Join(
            " ",
            "flex h-4 w-4 shrink-0 items-center justify-center rounded border text-white",
            isSelected
                ? "border-blue-600 bg-blue-600"
                : "border-slate-300 bg-white"
        );
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
            _selectedIds = _selectedIds.Where(id => _list.Any(item => item.Id == id)).ToList();
            return;
        }

        _list = _state.Value.List.Select(item => item.User).ToList();
        _selectedIds = _selectedIds.Where(id => _list.Any(item => item.Id == id)).ToList();
    }
}
