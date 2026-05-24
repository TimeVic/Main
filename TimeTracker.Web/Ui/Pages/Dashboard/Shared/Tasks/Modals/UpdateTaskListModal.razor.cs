using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Client.Core.Store.TasksList;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals;

public partial class UpdateTaskListModal
{
    [Parameter]
    public required TaskListDto TaskList { get; set; }

    [Parameter]
    public required bool IsOpened { get; set; } = false;

    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }

    private UpdateRequest _model = new();
    private bool _isLoading = false;
    private EditForm _form = null!;
    private LumexModal _modal = null!;
    private bool _wasOpened = false;

    protected override void OnParametersSet()
    {
        if (_model.TaskListId != TaskList.Id || IsOpened && !_wasOpened)
        {
            _model = new UpdateRequest
            {
                TaskListId = TaskList.Id,
                ProjectId = TaskList.Project.Id,
                Name = TaskList.Name
            };
        }

        _wasOpened = IsOpened;
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        _isLoading = true;
        try
        {
            var responseDto = await ApiService.TaskListUpdateAsync(_model);
            if (responseDto != null)
            {
                Dispatcher.Dispatch(new SetListItemAction(responseDto));
                ToastService.ShowInfo(DashboardLocalizer["UpdateTaskListModal_TaskListUpdated"].Value);
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

    private async Task OnCloseModal()
    {
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }
}
