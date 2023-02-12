using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class UpdateTaskForm
{
    [Parameter]
    public TaskDto Task { get; set; }
    
    [Inject]
    public IState<TasksListState> TasksListState { get; set; }
    
    private UpdateRequest model = new();
    private bool _isLoading = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.Fill(Task);
    }

    private async Task HandleSubmit(UpdateRequest request)
    {
        _isLoading = true;
        try
        {
            var responseDto = await ApiService.TasksUpdateAsync(request);
            if (responseDto != null)
            {
                Dispatcher.Dispatch(new LoadListAction());
                model.Title = "";
                NotificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Task has been added"
                });
            }
        }
        catch (Exception)
        {
            NotificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Error,
                Summary = "Task adding error"
            });
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }
}
