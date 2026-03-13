using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Pages.Dashboard.Tasks;

public partial class TaskPage
{
    [Parameter]
    public Guid? TaskId
    {
        get => _taskId;
        set
        {
            _taskId = value;
            InvokeAsync(async () => await LoadTask());
        }
    }

    [Parameter]
    public Guid WorkspaceId { get; set; }

    [Inject]
    private ILogger<TaskPage> _logger { get; set; }

    [Inject]
    private NavigationManager _navigationManager { get; set; }
    
    [Inject]
    private ModalDialogProviderService _dialogProviderService { get; set; }
    
    [Inject]
    private UrlService _urlService { get; set; }
    
    [Inject]
    private IState<AuthState> _authState { get; set; }
    
    private Guid? _taskId = Guid.Empty;
    private TaskFullDto? _task;
    private bool _isLoading = false;

    private async Task LoadTask()
    {
        if (!_taskId.HasValue)
        {
            NavigateToTasksPage();
            return;
        }
        _isLoading = true;
        
        if (WorkspaceId != _authState.Value.Workspace!.Id)
        {
            _urlService.NavigateToChangeWorkspace(
                WorkspaceId,
                string.Format(SiteUrl.Dashboard_Task, TaskId)
            );
            return;
        }
        
        try
        {
            _task = await ApiService.TasksGetOneAsync(_taskId.Value);
            if (_task == null)
            {
                NavigateToTasksPage();    
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            ToastService.ShowError("Task loading error");
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
            InvokeAsync(() =>
            {
                ToastService.ShowError("Task not found");
            });
            return;
        }
        var clientId = _task.TaskList.Project.Client?.Id ?? Guid.Empty;
        NavigationManager.NavigateTo(
            string.Format(SiteUrl.Dashboard_Tasks, clientId, _task.TaskList.Id)    
        );
    }
}
