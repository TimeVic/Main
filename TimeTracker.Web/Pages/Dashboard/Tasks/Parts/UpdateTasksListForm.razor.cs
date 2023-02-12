using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class UpdateTasksListForm
{
    private TaskListDto? _TaskList;
    private UpdateRequest model = new();
    private bool _isLoading = false;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
    
    private async Task HandleSubmit()
    {
        _isLoading = true;
        // try
        // {
        //     var membershipDto = await ApiService.WorkspaceMembershipUpdateAsync(model);
        //     if (membershipDto != null)
        //     {
        //         Dispatcher.Dispatch(new LoadListAction(true));
        //         NotificationService.Notify(new NotificationMessage()
        //         {
        //             Severity = NotificationSeverity.Info,
        //             Summary = "Member access has been changed"
        //         });
        //         DialogService.Close();
        //     }
        // }
        // catch (Exception)
        // {
        //     NotificationService.Notify(new NotificationMessage()
        //     {
        //         Severity = NotificationSeverity.Error,
        //         Summary = "Member access saving error"
        //     });
        // }
        // finally
        // {
        //     _isLoading = false;
        // }
        StateHasChanged();
    }
    
    private void CloseModal()
    {
        DialogService.Close();
    }
}
