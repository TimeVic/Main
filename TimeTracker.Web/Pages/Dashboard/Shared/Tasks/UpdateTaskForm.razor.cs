using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Store.Dashboard;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.WorkspaceMemberships;
using SetListItemAction = TimeTracker.Web.Store.Tasks.SetListItemAction;

namespace TimeTracker.Web.Pages.Dashboard.Shared.Tasks;

public partial class UpdateTaskForm
{
    [Parameter]
    public TaskDto Task { get; set; }
    
    [Inject]
    public IState<Store.TasksList.TasksListState> _tasksListState { get; set; }

    [Inject] 
    private ISecurityManager _securityManager { get; set; }

    [Inject]
    private IState<WorkspaceMembershipsState> _workspaceMembershipsState { get; set; }

    [CascadingParameter] 
    public MudDialogInstance MudDialog { get; set; }
    
    private UpdateRequest _model = new();
    private bool _isLoading = false;
    private MudForm? _form;
    
    private MudDatePicker? _startDatePicker;
    private MudDatePicker? _endDatePicker;
    private MudDatePicker? _reminderDatePicker;
    private MudTimePicker? _reminderTimePicker;
    
    private bool _isValid = false;
    
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
        _form.Validate();
        if (!_form.IsValid)
        {
            return;
        }
        Dispatcher.Dispatch(new UpdateListItemAction(_model, IsUpdateState: true));
        OnCloseModal();
    }

    private async Task OnChangedAssigned(WorkspaceMembershipDto membership)
    {
        _model.UserId = membership.User.Id;
    }

    private void OnFileUploaded(StoredFileDto uploadedFile)
    {
        Task.Attachments.Add(uploadedFile);
        Dispatcher.Dispatch(new SetAttachmentsAction(Task.TaskId, Task.Attachments));
    }

    private void OnTagsChanged(IEnumerable<long> selectedTagIds)
    {
        _model.TagIds = selectedTagIds.ToList();
    }

    private void AttachmentsListUpdated(ICollection<StoredFileDto> attachments)
    {
        Task.Attachments = attachments;
        Dispatcher.Dispatch(new SetAttachmentsAction(Task.TaskId, Task.Attachments));
    }

    private bool ValidateStartTime(DateTime? modelStartTime)
    {
        if (!modelStartTime.HasValue || !_model.EndTime.HasValue)
        {
            return true;
        }
        return modelStartTime < _model.EndTime;
    }

    private bool ValidateEndTime(DateTime? modelEndTime)
    {
        if (!modelEndTime.HasValue || !_model.StartTime.HasValue)
        {
            return true;
        }
        return modelEndTime > _model.StartTime;
    }

    private async Task OnReminderTimeChanged(TimeSpan? endTime)
    {
        _model.ReminderTime = _model.ReminderTime?.StartOfDay();
        if (endTime.HasValue)
        {
            _model.ReminderTime = _model.ReminderTime?.Add(endTime.Value);
        }
        _model.ReminderTime = _model.ReminderTime?.ToLocalTime();
        await ValidateDates();
    }
    
    private async Task OnStartDateChanged(DateTime? date)
    {
        _model.StartTime = date;
        if (await ValidateDates())
        {
            SetReminderTimeIfEmpty(_model.StartTime);
        }
    }
    
    private async Task OnEndDateChanged(DateTime? date)
    {
        _model.EndTime = date;
        if (await ValidateDates())
        {
            SetReminderTimeIfEmpty(_model.EndTime);
        }
    }

    private async Task<bool> ValidateDates()
    {
        await _startDatePicker!.Validate();
        await _endDatePicker!.Validate();
        await _reminderDatePicker!.Validate();
        await _reminderTimePicker!.Validate();

        return IsDatesValidValidateDates();
    }
    
    private bool IsDatesValidValidateDates()
    {
        return !_startDatePicker!.Error
            && !_endDatePicker!.Error
            && !_reminderDatePicker!.Error
            && !_reminderTimePicker!.Error;
    }
    
    private void SetReminderTimeIfEmpty(DateTime? time)
    {
        if (!time.HasValue)
            return;
        if (!_model.ReminderTime.HasValue)
        {
            _model.ReminderTime = time?.StartOfDay().AddHours(9);
        }
    }
    
    private void OnCloseModal()
    {
        MudDialog.Close();
    }
}
