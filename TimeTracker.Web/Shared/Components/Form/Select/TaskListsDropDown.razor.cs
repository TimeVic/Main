using System.Linq.Expressions;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Shared.Components.Form.Select;

public partial class TaskListsDropDown
{ 
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
    
    private IEnumerable<IGrouping<long, TaskListDto>> _groupedList => _list.GroupBy(item => item.Project.Id).AsQueryable();
    private long? _projectId = null;
    private string? _searchString = null;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Placeholder = "Select task list";

        _state.StateChanged += (sender, args) =>
        {
            UpdateList();
        };
        UpdateList();
    }
    
    private void UpdateList()
    {
        _list = _state.Value.List.ToList();
        if (ProjectId.HasValue)
        {
            _list = _list.Where(item => item.Project?.Id == ProjectId).ToList();
        }
        else if (ClientId.HasValue)
        {
            _list = _list.Where(item => item.Project?.Client?.Id == ClientId).ToList();
        }

        UpdateSelectedItem();
    }
    
    protected override void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
    }
}
