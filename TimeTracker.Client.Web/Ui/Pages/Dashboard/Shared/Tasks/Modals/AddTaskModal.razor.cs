using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Tasks;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals;

public partial class AddTaskModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public TaskListDto? TaskList { get; set; }

    [Parameter]
    public Guid? TaskListId { get; set; }

    [Parameter]
    public Guid? ProjectId { get; set; }
    
    [Parameter]
    public virtual EventCallback<TaskFullDto?> OnAdded { get; set; }

    [Parameter]
    public Guid? TimeEntryId { get; set; }

    [Inject]
    private IState<TimeTracker.Client.Core.Store.TasksList.TasksListState> _tasksListState { get; set; } = default!;
    
    private AddRequest model = new();
    private bool _isLoading = false;
    private bool _isShowMoreOptions = false;
    private EditForm _form = default!;
    private TaskListDto? _selectedTaskList;

    private void ToggleMoreOptions()
    {
        _isShowMoreOptions = !_isShowMoreOptions;
    }

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
                if (ModalInstance != null)
                {
                    await ModalInstance.Close(AppModalResult.Ok(responseDto));
                }
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
}
