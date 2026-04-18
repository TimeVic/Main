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
    public required TaskListDto TaskList { get; set; }
    
    [Parameter]
    public required bool IsOpened { get; set; } = false;
    
    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }
    
    [Parameter]
    public virtual EventCallback<TaskFullDto?> OnAdded { get; set; }
    
    private AddRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private EditForm _form;
    private LumexModal modal;
    
    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        _isLoading = true;
        try
        {
            model.TaskListId = TaskList.Id;
            var responseDto = await ApiService.TasksAddAsync(model);
            if (responseDto != null)
            {
                model = new AddRequest();
                await OnCloseModal();
                Dispatcher.Dispatch(new SetListItemAction(responseDto));
                Dispatcher.Dispatch(new SetOverdueTasksListItemAction(responseDto));
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
    
    private async Task OnCloseModal()
    {
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }
}
