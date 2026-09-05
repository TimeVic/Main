using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Core.Components;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Store.WorkspaceMembers;
using TimeTracker.Client.Core.Ui.Shared.Components.Enums;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class MultipleUsersSelect : BaseReactiveComponent, IDisposable
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public IEnumerable<Guid> Value
    {
        get => _selectedIds;
        set => _selectedIds = value ?? Enumerable.Empty<Guid>();
    }

    [Parameter]
    public EventCallback<IEnumerable<Guid>> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<IEnumerable<UserDto>> SelectedItemChanged { get; set; }

    [Parameter]
    public string Placeholder { get; set; } = string.Empty;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public bool Clearable { get; set; } = true;

    [Parameter]
    public bool FullWidth { get; set; } = true;

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

    private IEnumerable<Guid> _allowedIds = new List<Guid>();
    private IEnumerable<Guid> _selectedIds = new List<Guid>();
    private ICollection<UserDto> _list = new List<UserDto>();

    private ICollection<UserDto> _selectedUserList =>
        _list.Where(item => _selectedIds.Contains(item.Id)).ToList();

    protected string LocalizedPlaceholder =>
        string.IsNullOrWhiteSpace(Placeholder) ? DashboardLocalizer["SelectUser"].Value : Placeholder;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }

    private void UpdateList()
    {
        var users = _state.Value.List
            .Where(item => !_allowedIds.Any() || _allowedIds.Contains(item.User.Id))
            .Select(item => item.User)
            .DistinctBy(u => u.Id)
            .ToList();
        _list = users;
        InvokeAsync(StateHasChanged);
    }

    private async Task OnUsersChanged(ICollection<UserDto> users)
    {
        _selectedIds = users.Select(u => u.Id).ToList();
        await ValueChanged.InvokeAsync(_selectedIds);
        await SelectedItemChanged.InvokeAsync(users);
    }

    public void Dispose()
    {
    }
}
