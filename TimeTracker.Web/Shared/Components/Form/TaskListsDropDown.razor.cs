using System.Linq.Expressions;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Shared.Components.Form;

public partial class TaskListsDropDown
{
    [Parameter]
    public Expression<Func<long>>? For { get; set; }
    
    [Parameter] 
    public bool Disabled { get; set; }
    
    [Parameter] 
    public bool Required { get; set; }

    [Parameter]
    public bool AllowClear { get; set; } = true;
    
    [Parameter]
    public long Value
    {
        get => _selectedId;
        set
        {
            if (_selectedId != value)
                _selectedId = value;
        }
    }
    
    [Parameter]
    public EventCallback<TaskListDto?> SelectedItemChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; } = "Select task list";
    
    [Parameter]
    public string? Label { get; set; }
    
    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public long? ProjectId
    {
        get => _projectId;
        set
        {
            _projectId = value;
            UpdateList();
        }
    }
    
    [Parameter]
    public long? ClientId { get; set; }

    [Parameter]
    public bool IsExtendedInfo { get; set; } = true;
    
    [Inject]
    public IState<TasksListState> _state { get; set; }
    
    private TaskListDto? _selectedItem => _list.FirstOrDefault(item => item.Id == _selectedId);
    private ICollection<TaskListDto> _list = new List<TaskListDto>();
    private long _selectedId = 0;
    private long? _projectId = null;
    private string? _searchString = null;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }
    
    private Task OnValueChanged(string? project)
    {
        long.TryParse(project, out long selectedId);
        if (selectedId != (_selectedItem?.Id ?? 0))
        {
            _selectedId = selectedId;
            SelectedItemChanged.InvokeAsync(_selectedItem);    
        }
        return Task.CompletedTask;
    }

    private string ToStringFunc(long taskListId)
    {
        var item = _state.Value.List.FirstOrDefault(item => item.Id == taskListId);
        return item?.Name ?? string.Empty;
    }
    
    private void UpdateList()
    {
        _list = _state.Value.List;
        if (ProjectId.HasValue)
        {
            _list = _list.Where(item => item.Project?.Id == ProjectId).ToList();
        }
        else if (ClientId.HasValue)
        {
            _list = _list.Where(item => item.Project?.Client?.Id == ClientId).ToList();
        }
    }
}
