using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.WorkspaceMembers;
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
    private IState<WorkspaceMembersState> _workspaceMembersState { get; set; }
    
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
    private EditContext? _editContext;
    private InputFile? _attachmentInput;
    private ElementReference _attachmentDropZone;
    private DotNetObjectReference<UpdateTaskForm>? _dotNetObjectReference;
    private string? _attachmentInteropId;
    private bool _isAttachmentInteropInitialized;
    private bool _isDragActive;
    public TaskFullDto _task { get; set; } = new();

    private string _attachmentAcceptTypes => string.Join(",", StoredFileType.Attachment.GetAllowedMimeTypes());
    private ICollection<StoredFileDto> TaskAttachments => _task?.Attachments ?? new List<StoredFileDto>();
    private string EstimateInputLabel => _task.ExternalSourceType == ExternalSourceType.Jira
        ? DashboardLocalizer["OriginalEstimate"].Value
        : DashboardLocalizer["UpdateTaskForm_PlannedTime"].Value;
    private string EstimateInputHint => _task.ExternalSourceType == ExternalSourceType.Jira
        ? DashboardLocalizer["UpdateTaskForm_JiraEstimateHint"].Value
        : DashboardLocalizer["UpdateTaskForm_ManualEstimateHint"].Value;
    
    protected override async Task OnInitializedAsync()
    {
        _editContext = new EditContext(_model);

        _isLoading = true;
        _task = await ApiService.TasksGetOneAsync(TaskId);
        if (_task != null)
        {
            _model.Fill(_task);
        }
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Tag.LoadListAction());
        await base.OnInitializedAsync();
        _editContext.OnFieldChanged += OnFormFieldChanged;
        _isLoading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (_isLoading || _isAttachmentInteropInitialized || _attachmentInput == null)
        {
            return;
        }

        _dotNetObjectReference = DotNetObjectReference.Create(this);
        _attachmentInteropId = await Js.InvokeAsync<string>(
            "taskAttachmentInput.attach",
            _attachmentDropZone,
            _attachmentInput.Element.Value,
            _dotNetObjectReference
        );
        _isAttachmentInteropInitialized = true;
    }

    public void Dispose()
    {
        _editContext?.OnFieldChanged -= OnFormFieldChanged;
        if (!string.IsNullOrWhiteSpace(_attachmentInteropId))
        {
            _ = Js.InvokeVoidAsync("taskAttachmentInput.detach", _attachmentInteropId);
        }
        _dotNetObjectReference?.Dispose();
    }
    
    private void OnFormFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        SubmitForm();
    }

    private void SubmitForm()
    {
        if (!_form!.EditContext!.Validate())
        {
            return;
        }
        Dispatcher.Dispatch(new UpdateTaskAction(_model, IsUpdateState: true));
    }

    private void OnAssignedChanged(WorkspaceMemberDto? member)
    {
        if (member == null)
            return;
        _model.UserId = member.Id;
        SubmitForm();
    }

    private void OnStatusChanged(TaskStatus? status)
    {
        if (status == null)
            return;
        _model.Status = status.Value;
        SubmitForm();
    }

    private void OnPriorityChanged(TaskPriority? priority)
    {
        if (priority == null)
            return;
        _model.Priority = priority.Value;
        SubmitForm();
    }
    
    private Task OnTagsChanged(IEnumerable<Guid> tagIds)
    {
        _model.TagIds = tagIds.ToList();
        SubmitForm();
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task SetAttachmentDragActive(bool isActive)
    {
        _isDragActive = isActive;
        return InvokeAsync(StateHasChanged);
    }

    private async Task OnAttachmentsInputFileChange(InputFileChangeEventArgs eventArguments)
    {
        if (eventArguments.FileCount == 0)
        {
            return;
        }

        foreach (var file in eventArguments.GetMultipleFiles(eventArguments.FileCount))
        {
            try
            {
                var uploadedFile = await ApiService.StorageUploadFileAsync(
                    _task.Id,
                    StorageEntityType.Task,
                    StoredFileType.Attachment,
                    file
                );
                if (uploadedFile == null)
                {
                    ToastService.ShowError(DashboardLocalizer["UpdateTaskForm_AttachmentUploadError"].Value);
                    continue;
                }

                _task.Attachments.Add(uploadedFile);
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ToastService.ShowError(e.Message);
            }
        }
    }

    private Task OnAttachmentsChanged(ICollection<StoredFileDto> attachments)
    {
        _task.Attachments = attachments;
        return Task.CompletedTask;
    }
}
