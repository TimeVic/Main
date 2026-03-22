using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.WorkspaceMemberships;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks;

public partial class UpdateTaskForm
{   
    [Parameter]
    public required Guid TaskId { get; set; }
    
    [Inject]
    public IState<Store.TasksList.TasksListState> _tasksListState { get; set; }

    [Inject]
    public IState<TasksState> _tasksState { get; set; }
    
    [Inject] 
    private ISecurityManager _securityManager { get; set; }

    [Inject]
    private IState<WorkspaceMembershipsState> _workspaceMembershipsState { get; set; }

    [Inject]
    public ILogger<UpdateTaskForm> _logger { get; set; }
    
    [CascadingParameter] 
    public required FluentDialog MudDialog { get; set; }
    
    private UpdateRequest _model = new();
    private bool _isLoading = false;
    private EditForm? _form;
    public TaskFullDto _task { get; set; } = new();
    
    private string _tabLabelAttachments
    {
        get
        {
            var label = "Attachments";
            if (_task.Attachments.Any())
            {
                label += $"({_task.Attachments.Count})";
            }

            return label;
        }
    }

    private ICollection<Guid> _allowedUserIds
    {
        get
        {
            return _securityManager.GetMembersWhichHaveAccessToProject(_task.TaskList.Project)
                .Select(item => item.Id)
                .ToList();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _task = await ApiService.TasksGetOneAsync(TaskId);
        if (_task != null)
        {
            _model.Fill(_task);
            return;
        }
        _logger.LogError("Task with id {TaskId} not found", TaskId);
        await base.OnInitializedAsync();
    }

    private void SubmitForm()
    {
        if (!_form!.EditContext!.Validate())
        {
            return;
        }
        Dispatcher.Dispatch(new UpdateTaskAction(_model, IsUpdateState: true));
    }

    private void OnChangeTitle(string title)
    {
        _model.Title = title;
        SubmitForm();
    }
    
    private void OnChangedAssigned(WorkspaceMembershipDto membership)
    {
        _model.UserId = membership.User.Id;
        SubmitForm();
    }

    private void OnFileUploaded(StoredFileDto uploadedFile)
    {
        _task.Attachments.Add(uploadedFile);
    }

    private void OnTagsChanged(IEnumerable<Guid> selectedTagIds)
    {
        _model.TagIds = selectedTagIds.ToList();
        SubmitForm();
    }

    private void AttachmentsListUpdated(ICollection<StoredFileDto> attachments)
    {
        _task.Attachments = attachments;
    }

    private bool DisabledStartTime(DateTime modelStartTime)
    {
        if (!_model.EndTime.HasValue)
        {
            return false;
        }
        return modelStartTime.Date >= _model.EndTime.Value.Date;
    }

    private bool DisabledEndTime(DateTime modelEndTime)
    {
        if (!_model.StartTime.HasValue)
        {
            return false;
        }
        return modelEndTime.Date <= _model.StartTime.Value.Date;
    }

    private void OnReminderTimeChanged(DateTime? time)
    {
        _model.ReminderTime = time?.ToLocalTime();
        SubmitForm();
    }
    
    private void OnStartDateChanged(DateTime? date)
    {
        _model.StartTime = date;
        SubmitForm();
    }
    
    private void OnEndDateChanged(DateTime? date)
    {
        _model.EndTime = date;
        SubmitForm();
    }
    
    private void OnArchivedChanged(bool val)
    {
        _model.IsArchived = val;
        SubmitForm();
    }
    
    private void OnDescriptionChanged(string description)
    {
        _model.Description = description;
        SubmitForm();
    }
    
    private void OnCloseModal()
    {
        MudDialog.CloseAsync();
    }
}
