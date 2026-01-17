using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.TasksList;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TasksGrid: IDisposable
{
    [Parameter]
    public ICollection<TaskStatus> Statuses { get; set; }
 
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
    
    [Inject]
    public ModalDialogProviderService ModalDialogProviderService { get; set; }

    [Inject]
    public IActionSubscriber ActionSubscriber { get; set; }

    [Inject]
    public IState<TasksState> TasksState { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }
    
    private readonly Subject<ICollection<TaskDto>> _tasksSubject = new();
    private ICollection<TaskDto> _tasks = new List<TaskDto>();
    private ICollection<TaskDto> _selectedTasks = new List<TaskDto>();
    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        TasksState.StateChanged += OnTaskStateChanged;

        _tasksSubject
            .Select(items =>
            {
                return items.Where(item => Statuses.Contains(item.Status))
                    .OrderByDescending(item => item.UpdateTime)
                    .ToArray();
            })
            .Subscribe(results =>
            {
                _tasks = results;
                StateHasChanged();
            });
        
        ActionSubscriber.SubscribeToAction<TimeTracker.Web.Store.Tasks.SetIsListLoading>(this, action =>
        {
            _isLoading = action.IsLoading;
            StateHasChanged();
        });
    }

    private void OnTaskStateChanged(object? sender, EventArgs e)
    {
        _tasksSubject.OnNext(TasksState.Value.List);
    }

    private async Task OnAddTask()
    {
        await ModalDialogProviderService.ShowAddTaskModal(
            taskListId: _tasksListState.Value.SelectedTaskListId,
            taskStatus: TaskStatus.Backlog
        );
    }
    
    private async Task OnEditTask(TaskDto? task)
    {
        if (task == null)
            return;
        await ModalDialogProviderService.ShowEditTaskModal(task);
    }
    
    private void OnSelectTask(TaskDto task)
    {
        if (IsSelectedTask(task))
        {
            _selectedTasks.Remove(task);
        }
        else
        {
            _selectedTasks.Add(task);
        }
    }
    
    private bool IsSelectedTask(TaskDto task)
    {
        return _selectedTasks.Contains(task);
    }
    
    public void Dispose()
    {
        TasksState.StateChanged -= OnTaskStateChanged;
        ActionSubscriber.UnsubscribeFromAllActions(this);
        _tasksSubject.Dispose();
    }

    private void ArchiveTasks()
    {
        foreach (var selectedTask in _selectedTasks)
        {
            selectedTask.IsArchived = true;
            var updateRequest = new UpdateRequest();
            updateRequest.Fill(selectedTask);
            Dispatcher.Dispatch(new UpdateTaskAction(updateRequest, IsUpdateState: true));
        }
        _selectedTasks.Clear();
    }
}
