using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.TasksList;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals;

public partial class UpdateTaskListModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public required TaskListDto TaskList { get; set; }

    private UpdateRequest _model = new();
    private bool _isLoading = false;
    private EditForm _form = null!;

    protected override void OnParametersSet()
    {
        if (_model.TaskListId != TaskList.Id)
        {
            _model = new UpdateRequest
            {
                TaskListId = TaskList.Id,
                ProjectId = TaskList.Project.Id,
                Name = TaskList.Name
            };
        }
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
}
