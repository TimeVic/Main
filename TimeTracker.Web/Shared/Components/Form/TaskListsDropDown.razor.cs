using System.Linq.Expressions;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
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
    public bool Clearable { get; set; }
    
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
    
    [Parameter]
    public bool FullWidth { get; set; } = false;
    
    [Inject]
    public IState<TasksListState> _state { get; set; }
    
    private TaskListDto? _selectedItem;
    private ICollection<TaskListDto> _list = new List<TaskListDto>();
    private IEnumerable<IGrouping<long, TaskListDto>> _groupedList => _list.GroupBy(item => item.Project.Id).AsQueryable();
    private string? _selectedId = null;
    private long? _projectId = null;
    private string? _searchString = null;
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
    
    private void OnValueChanged(TaskListDto? taskList)
    {
        if (_selectedItem?.Id != taskList?.Id)
        {
            UpdateSelectedItem();
            SelectedItemChanged.InvokeAsync(_selectedItem);
        }
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
    
    private void OnClear()
    {
        if (string.IsNullOrEmpty(_selectedId))
            return;
        _selectedId = null;
    }
    
    private void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
    }
}
