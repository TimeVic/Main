using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Store.TasksList;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class TaskListsSelect : BaseSingleSelect<TaskListDto>, IDisposable
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

    [Inject]
    public IState<TasksListState> _state { get; set; }

    private Guid? _projectId;
    private bool _isInitialized;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Placeholder = DashboardLocalizer["SelectTaskList"].Value;
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

        if (_state.Value.DropDownProjectId == ProjectId)
        {
            return;
        }

        Dispatcher.Dispatch(new LoadDropDownListAction(ProjectId));
    }

    private void UpdateList()
    {
        _list = _state.Value.DropDownList.ToList();
        if (ProjectId.HasValue)
        {
            _list = _list.Where(item => item.Project?.Id == ProjectId).ToList();
        }
        else if (ClientId.HasValue)
        {
            _list = _list.Where(item => item.Project?.Client?.Id == ClientId).ToList();
        }

        UpdateSelectedItem();
        InvokeAsync(StateHasChanged);
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        UpdateList();
    }

    protected override void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
    }

    private void OnTaskListSelected(TaskListDto? item)
    {
        OnValueChanged(item);
    }

    public new void Dispose()
    {
        _state.StateChanged -= OnStateChanged;
        base.Dispose();
    }
}
