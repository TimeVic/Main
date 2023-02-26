using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Tasks;

namespace TimeTracker.Web.Pages.Dashboard.Tasks;

public partial class TaskPage
{
    [Parameter]
    public long? TaskId
    {
        get => _taskId;
        set
        {
            _taskId = value;
            InvokeAsync(async () => await LoadTask());
        }
    }

    [Inject]
    private ILogger<TaskPage> _logger { get; set; }

    [Inject]
    private NavigationManager _navigationManager { get; set; }
    
    [Inject]
    private ModalDialogProviderService _dialogProviderService { get; set; }
    
    private long? _taskId = 0;
    private TaskDto? _task;
    private bool _isLoading = false;

    private async Task LoadTask()
    {
        if (!_taskId.HasValue)
        {
            NavigateToTasksPage();
            return;
        }

        _isLoading = true;
        try
        {
            _task = await ApiService.TasksGetAsync(_taskId.Value);
            if (_task == null)
            {
                NavigateToTasksPage();    
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            NotificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Error,
                Summary = "Task loading error"
            });
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }

    private void NavigateToTasksPage()
    {
        if (_task == null)
        {
            _navigationManager.NavigateTo(SiteUrl.Dashboard_Tasks_Default);
            NotificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Error,
                Summary = "Task not found"
            });
            return;
        }
        var clientId = _task.TaskList.Project.Client?.Id ?? 0;
        NavigationManager.NavigateTo(
            string.Format(SiteUrl.Dashboard_Tasks, clientId, _task.TaskList.Id)    
        );
    }
}
