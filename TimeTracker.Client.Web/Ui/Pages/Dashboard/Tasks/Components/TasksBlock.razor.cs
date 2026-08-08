using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Client.Core.Store.Tasks;
using TimeTracker.Client.Core.Store.TasksList;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Tasks.Components;

public partial class TasksBlock: IDisposable
{   
    private const int SearchDebounceMilliseconds = 300;

    [Parameter]
    public bool IsEmbedded { get; set; }

    [Parameter]
    public ClientDto? ContextClient { get; set; }

    [Parameter]
    public ProjectDto? ContextProject { get; set; }

    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }
    
    [Inject]
    public IState<TasksState> TasksState { get; set; }
    
    private TaskListDto? _taskList => _tasksListState.Value.SelectedTaskList;
    private CancellationTokenSource? _searchDebounceCancellationTokenSource;
    private string? _searchString;
    private bool _isShowAddTaskModal;
    private bool _isShowAddTaskListModal;
    private bool _isShowArchived;

    private string? SearchString
    {
        get => _searchString;
        set
        {
            if (_searchString == value)
            {
                return;
            }

            _searchString = value;
            ScheduleSearchFilter();
        }
    }

    private string ContainerClass => IsEmbedded
        ? "flex min-h-[720px] w-full flex-col bg-white"
        : "space-y-4";

    private string HeaderClass => IsEmbedded
        ? "px-4 py-3"
        : "px-1 py-3";

    private string BoardClass => IsEmbedded
        ? "min-h-0 flex-1 p-3 sm:p-4"
        : string.Empty;

    private string EmptyStateClass => IsEmbedded
        ? "m-4 border border-dashed border-slate-200 bg-slate-50 px-5 py-6"
        : "border border-dashed border-slate-200 bg-slate-50 px-5 py-6";

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _searchString = TasksState.Value.Filter.SearchString;
        _isShowArchived = !TasksState.Value.Filter.IsArchived.HasValue;
    }

    public void Dispose()
    {
        CancelSearchDebounce();
    }

    private string GetTaskListContext() =>
        $"{_taskList?.Project.Client?.Name ?? DashboardLocalizer["NoClient"].Value} · {_taskList?.Project.Name}";

    private Task OnTaskListAdded(TaskListDto? taskList)
    {
        if (taskList != null)
        {
            NavigationManager.NavigateTo(UrlService.GetDashboardUrl($"tasks/{taskList.Id}"));
        }

        return Task.CompletedTask;
    }

    private void ScheduleSearchFilter()
    {
        CancelSearchDebounce();
        _searchDebounceCancellationTokenSource = new CancellationTokenSource();
        _ = ApplySearchFilterAfterDebounceAsync(_searchDebounceCancellationTokenSource.Token);
    }

    private async Task ApplySearchFilterAfterDebounceAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(SearchDebounceMilliseconds, cancellationToken);
            await InvokeAsync(() =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    ApplyFilter();
                }
            });
        }
        catch (OperationCanceledException)
        {
        }
    }

    private Task OnToggleShowArchived()
    {
        _isShowArchived = !_isShowArchived;
        CancelSearchDebounce();
        ApplyFilter();
        return Task.CompletedTask;
    }

    private void ApplyFilter()
    {
        var taskListId = _tasksListState.Value.SelectedTaskListId;
        if (!taskListId.HasValue)
        {
            return;
        }

        var filter = new GetListFilterRequest();
        filter.Fill(TasksState.Value.Filter);
        filter.SearchString = string.IsNullOrWhiteSpace(_searchString) ? null : _searchString.Trim();
        filter.IsArchived = _isShowArchived ? null : false;

        Dispatcher.Dispatch(new SetListFilterAction(filter));
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Tasks.LoadListAction(taskListId, filter));
    }

    private void CancelSearchDebounce()
    {
        _searchDebounceCancellationTokenSource?.Cancel();
        _searchDebounceCancellationTokenSource?.Dispose();
        _searchDebounceCancellationTokenSource = null;
    }
}
