using Fluxor;
using LumexUI.Common;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Select;

public partial class TaskListsDropDown: IDisposable
{ 
    [Parameter]
    public Guid? ProjectId
    {
        get => _projectId;
        set
        {
            if (_projectId == value)
            {
                return;
            }

            _projectId = value;
            UpdateList();
            LoadList();
        }
    }
    
    [Parameter]
    public Guid? ClientId { get; set; }

    [Parameter]
    public bool IsExtendedInfo { get; set; } = true;

    [Parameter]
    public LabelPlacement LabelPlacement { get; set; } = LabelPlacement.Outside;

    [Parameter]
    public InputVariant Variant { get; set; } = InputVariant.Flat;
    
    [Inject]
    public IState<TasksListState> _state { get; set; }
    
    private IEnumerable<IGrouping<Guid, TaskListDto>> _groupedList => _list.GroupBy(item => item.Project.Id).AsQueryable();
    private Guid? _projectId = null;
    private string? _searchString = null;
    private bool _isInitialized = false;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Placeholder = "Select task list";
        _isInitialized = true;

        _state.StateChanged += OnStateChanged;

        LoadList();
        UpdateList();
    }

    private void LoadList()
    {
        if (!_isInitialized)
        {
            return;
        }

        if (!ProjectId.HasValue)
        {
            _list = new List<TaskListDto>();
            _selectedItem = null;
            return;
        }

        Dispatcher.Dispatch(new LoadListAction(ProjectId: ProjectId));
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

    private void OnStateChanged(object? sender, EventArgs e)
    {
        UpdateList();
        InvokeAsync(StateHasChanged);
    }
    
    protected override void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
    }

    public new void Dispose()
    {
        _state.StateChanged -= OnStateChanged;
        base.Dispose();
    }
}
