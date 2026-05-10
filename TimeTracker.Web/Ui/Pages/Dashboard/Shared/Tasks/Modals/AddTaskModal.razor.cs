using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Store.Tasks;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals;

public partial class AddTaskModal
{
    [Parameter]
    public TaskListDto? TaskList { get; set; }

    [Parameter]
    public Guid? TaskListId { get; set; }

    [Parameter]
    public Guid? ProjectId { get; set; }
    
    [Parameter]
    public required bool IsOpened { get; set; } = false;
    
    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }
    
    [Parameter]
    public virtual EventCallback<TaskFullDto?> OnAdded { get; set; }

    [Parameter]
    public Guid? TimeEntryId { get; set; }

    [Inject]
    private IState<TimeTracker.Web.Store.TasksList.TasksListState> _tasksListState { get; set; }
    
    private AddRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private EditForm _form;
    private LumexModal modal;
    private TaskListDto? _selectedTaskList;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (TaskList != null)
        {
            _selectedTaskList = TaskList;
            return;
        }

        if (TaskListId.HasValue)
        {
            _selectedTaskList = _tasksListState.Value.List.FirstOrDefault(item => item.Id == TaskListId.Value);
        }
    }
    
    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }
        if (_selectedTaskList == null)
        {
            ToastService.ShowError(DashboardLocalizer["AddTaskModal_TaskListRequired"].Value);
            return;
        }

        _isLoading = true;
        try
        {
            model.TaskListId = _selectedTaskList.Id;
            model.TimeEntryId = TimeEntryId;
            var responseDto = await ApiService.TasksAddAsync(model);
            if (responseDto != null)
            {
                model = new AddRequest();
                Dispatcher.Dispatch(new SetListItemAction(responseDto));
                Dispatcher.Dispatch(new SetOverdueTasksListItemAction(responseDto));
                await OnAdded.InvokeAsync(responseDto);
                await OnCloseModal();
            }
        }
        catch (Exception e)
        {
            ToastService.ShowError(e.Message);
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();    
    }

    private void OnTaskListSelected(TaskListDto? taskList)
    {
        _selectedTaskList = taskList;
    }
    
    private async Task OnCloseModal()
    {
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }
}
