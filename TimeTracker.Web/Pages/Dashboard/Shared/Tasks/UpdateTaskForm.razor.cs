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

namespace TimeTracker.Web.Pages.Dashboard.Shared.Tasks;

public partial class UpdateTaskForm
{   
    [Parameter]
    public required TaskDto Task { get; set; }
    
    [Inject]
    public IState<Store.TasksList.TasksListState> _tasksListState { get; set; }

    [Inject]
    public IState<TasksState> _tasksState { get; set; }
    
    [Inject] 
    private ISecurityManager _securityManager { get; set; }

    [Inject]
    private IState<WorkspaceMembershipsState> _workspaceMembershipsState { get; set; }

    [CascadingParameter] 
    public required FluentDialog MudDialog { get; set; }
    
    private UpdateRequest _model = new();
    private bool _isLoading = false;
    private EditForm? _form;
    
    private string _tabLabelAttachments
    {
        get
        {
            var label = "Attachments";
            if (Task.Attachments.Any())
            {
                label += $"({Task.Attachments.Count})";
            }

            return label;
        }
    }

    private ICollection<long> _allowedUserIds
    {
        get
        {
            return _securityManager.GetMembersWhichHaveAccessToProject(Task.TaskList.Project)
                .Select(item => item.Id)
                .ToList();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _model.Fill(Task);
    }

    private void SubmitForm()
    {
        Debug.Log("SubmitForm");
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
        Task.Attachments.Add(uploadedFile);
        Dispatcher.Dispatch(new SetAttachmentsAction(Task.TaskId, Task.Attachments));
    }

    private void OnTagsChanged(IEnumerable<long> selectedTagIds)
    {
        _model.TagIds = selectedTagIds.ToList();
        SubmitForm();
    }

    private void AttachmentsListUpdated(ICollection<StoredFileDto> attachments)
    {
        Task.Attachments = attachments;
        Dispatcher.Dispatch(new SetAttachmentsAction(Task.TaskId, Task.Attachments));
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
