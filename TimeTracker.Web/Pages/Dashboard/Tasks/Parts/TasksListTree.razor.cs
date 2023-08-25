using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Pages.Dashboard.Tasks.Parts.TasksList;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Common;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

internal enum TaskListAction
{
    Add = -1,
    Edit = -2,
    Delete = -3,
}

public partial class TasksListTree
{
    [CascadingParameter(Name = "ProjectId")]
    public long ProjectId
    {
        get => _projectId;
        set
        {
            _projectId = value;
            OnTasksListSelected(null);
        }
    }
    
    [CascadingParameter(Name = "TaskListId")]
    public long? TaskListId
    {
        get => _taskListId;
        set
        {
            _taskListId = value;
            OnTasksListSelected(_taskListId);
        }
    }
    
    [Inject]
    public IState<CommonState> _commonState { get; set; }
    
    [Inject]
    public IState<ProjectState> _projectState { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }
    
    [Inject]
    public ModalDialogProviderService _modalDialogProviderService { get; set; }

    private long _projectId = 0;
    private long? _taskListId = null;
    private MudTabs _tabsPanel;

    public long _selectedTaskListId
    {
        get => _tasksListState.Value.SelectedTaskListId ?? 0;
    }
    
    private ICollection<TaskListDto> _tasksList
    {
        get
        {
            var list = _tasksListState.Value.List;
            return list.Where(item => item.Project?.Id == _projectId).ToList();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _tasksListState.StateChanged += (sender, args) =>
        {
            _tabsPanel.ActivatePanel((object)_selectedTaskListId);
        };
    }

    private void ShowAddTaskListModal()
    {
        InvokeAsync(async () => await _modalDialogProviderService.ShowEditTaskListModal(projectId: ProjectId));
    }
    
    private void ShowUpdateTaskListModal()
    {
        var taskList = _tasksListState.Value.List.First(item => item.Id == _selectedTaskListId);
        InvokeAsync(async () => await _modalDialogProviderService.ShowEditTaskListModal(taskList));
    }

    private async Task OnDeleteTaskList()
    {
        var taskList = _tasksListState.Value.List.First(item => item.Id == _selectedTaskListId);
        var isOk = await _modalDialogProviderService.ShowDeleteConfirmationDialog(
            $"Are you sure you want to remove: {taskList.Name}?"
        );
        if (isOk.HasValue && isOk.Value)
        {
            Dispatcher.Dispatch(new ArchiveTaskListAction(taskList));
        }
    }
    
    private void OnSelectedTasksList(long tasksListId)
    {
        NavigationManager.NavigateTo(
            string.Format(SiteUrl.Dashboard_Tasks, ProjectId, tasksListId)    
        );
    }
    
    private void OnTasksListSelected(long? testsListId)
    {
        Dispatcher.Dispatch(new SetSelectedAction(testsListId));
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction());
    }

    private async Task OnClickSplitButton(RadzenSplitButtonItem? selectedItem)
    {
        if (selectedItem == null)
        {
            return;
        }

        var isAction = Enum.TryParse(selectedItem.Value, out TaskListAction action)
            && Enum.IsDefined(typeof(TaskListAction), action);
        if (isAction)
        {
            switch (action)
            {
                case TaskListAction.Add:
                    ShowAddTaskListModal();
                    break;
                case TaskListAction.Edit:
                    ShowUpdateTaskListModal();
                    break;
                case TaskListAction.Delete:
                    await OnDeleteTaskList();
                    break;
            }
        }
        else
        {
            OnSelectedTasksList(long.Parse(selectedItem.Value));
        }
    }
}
