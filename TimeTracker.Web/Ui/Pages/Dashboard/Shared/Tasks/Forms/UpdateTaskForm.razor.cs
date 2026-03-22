using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.WorkspaceMemberships;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks.Forms;

public partial class UpdateTaskForm: IDisposable
{
    [Parameter]
    public required Guid TaskId { get; set; }
    
    [Inject]
    public IState<TasksState> _tasksState { get; set; }
    
    [Inject] 
    private ISecurityManager _securityManager { get; set; }
    
    [Inject]
    private IState<WorkspaceMembershipsState> _workspaceMembershipsState { get; set; }
    
    [Inject]
    public ILogger<UpdateTaskForm> _logger { get; set; }
    
    private ICollection<Guid> _allowedUserIds
    {
        get
        {
            return _securityManager.GetMembersWhichHaveAccessToProject(_task.TaskList.Project)
                .Select(item => item.Id)
                .ToList();
        }
    }
    
    private UpdateRequest _model = new();
    private bool _isLoading = false;
    private EditForm? _form;
    public TaskFullDto _task { get; set; } = new();
    
    protected override async Task OnInitializedAsync()
    {
        _form?.EditContext?.OnFieldChanged += OnFormFieldChanged;
        
        _task = await ApiService.TasksGetOneAsync(TaskId);
        if (_task != null)
        {
            _model.Fill(_task);
            return;
        }
        _logger.LogError("Task with id {TaskId} not found", TaskId);
        await base.OnInitializedAsync();
    }

    public void Dispose()
    {
        _form?.EditContext?.OnFieldChanged -= OnFormFieldChanged;
    }
    
    private void OnFormFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        Debug.Log($"Field changed: {e.FieldIdentifier.FieldName}");
    }

    private void SubmitForm()
    {
        if (!_form!.EditContext!.Validate())
        {
            return;
        }
        Dispatcher.Dispatch(new UpdateTaskAction(_model, IsUpdateState: true));
    }

    private void OnAssignedChanged(WorkspaceMembershipDto? membership)
    {
        if (membership == null)
            return;
        _model.UserId = membership.Id;
    }

    private void OnStatusChanged(TaskStatus? status)
    {
        if (status == null)
            return;
        _model.Status = status.Value;
    }

    private void OnPriorityChanged(TaskPriority? priority)
    {
        if (priority == null)
            return;
        _model.Priority = priority.Value;
    }
}
