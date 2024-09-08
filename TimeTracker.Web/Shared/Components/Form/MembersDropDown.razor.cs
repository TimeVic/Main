using System.Linq.Expressions;
using Fluxor;
using Microsoft.AspNetCore.Components;
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
    public string? Label { get; set; }
    
    [Parameter]
    public Expression<Func<long>>? For { get; set; }
    
    [Parameter] 
    public bool Disabled { get; set; }

    [Parameter]
    public long Value
    {
        get
        {
            long.TryParse(_selectedId, out var id);
            return id;
        }
        set
        {
            if (value.ToString() != _selectedId)
            {
                _selectedId = value.ToString();
                UpdateSelectedItem();
            }
        }
    }

    [Parameter]
    public long? UserId
    {
        get => _userId;
        set
        {
            _userId = value;
            UpdateSelectedItem();
        }
    }
    
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
    public bool Required { get; set; }

    [Inject]
    public IState<WorkspaceMembershipsState> _state { get; set; }
    
    private ICollection<WorkspaceMembershipDto> _list = new List<WorkspaceMembershipDto>();
    private WorkspaceMembershipDto? _selectedItem;
    private string? _selectedId;
    private long? _userId;
    public string? _placeholder => _selectedItem is null ? Placeholder : null;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }
    
    private void OnValueChanged(WorkspaceMembershipDto? membership)
    {
        if (_selectedItem?.Id != membership?.Id)
        {
            UpdateSelectedItem();
            SelectedItemChanged.InvokeAsync(_selectedItem);
        }
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
    
    private void OnClear()
    {
        if (string.IsNullOrEmpty(_selectedId))
            return;
        _selectedId = null;
        _userId = null;
    }
    
    private void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => (_userId is not null && item.User.Id == _userId) || item.Id.ToString() == _selectedId
        );
    }
}
