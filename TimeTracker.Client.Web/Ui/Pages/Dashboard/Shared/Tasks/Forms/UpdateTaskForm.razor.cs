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
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Store.Tasks;
using TimeTracker.Client.Core.Store.WorkspaceMembers;
using TimeTracker.Client.Web.Ui.Shared.Components.Storage;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Forms;

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
    private readonly List<AttachmentsBlock.UploadingAttachment> _uploadingAttachments = new();
    private readonly HashSet<string> _uploadingAttachmentKeys = new();
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
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Tag.LoadListAction());
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
            "attachmentInput.attach",
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
            _ = Js.InvokeVoidAsync("attachmentInput.detach", _attachmentInteropId);
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

    private void OnTaskListChanged(TaskListDto? taskList)
    {
        if (taskList == null || taskList.Id == _model.TaskListId)
        {
            return;
        }

        _model.TaskListId = taskList.Id;
        _task.TaskList = taskList;
        SubmitForm();
    }

    private Task OnDescriptionChanged(string description)
    {
        _model.Description = description;
        SubmitForm();
        return Task.CompletedTask;
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

        var files = eventArguments
            .GetMultipleFiles(eventArguments.FileCount)
            // Prevent the same drag event from uploading an attachment twice.
            .Where(file => _uploadingAttachmentKeys.Add(GetAttachmentKey(file)))
            .ToList();
        if (files.Count == 0)
        {
            return;
        }
        var uploadingAttachments = files
            .Select(file => new AttachmentsBlock.UploadingAttachment(Guid.NewGuid(), file.Name, file.ContentType))
            .ToList();

        // Show all selected attachments immediately while their uploads are still running.
        _uploadingAttachments.AddRange(uploadingAttachments);
        await InvokeAsync(StateHasChanged);

        foreach (var fileInfo in files.Zip(uploadingAttachments))
        {
            try
            {
                var uploadedFile = await ApiService.StorageUploadFileAsync(
                    _task.Id,
                    StorageEntityType.Task,
                    StoredFileType.Attachment,
                    fileInfo.First
                );
                if (uploadedFile == null)
                {
                    ToastService.ShowError(DashboardLocalizer["UpdateTaskForm_AttachmentUploadError"].Value);
                    continue;
                }

                _task.Attachments.Add(uploadedFile);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ToastService.ShowError(e.Message);
            }
            finally
            {
                _uploadingAttachmentKeys.Remove(GetAttachmentKey(fileInfo.First));
                _uploadingAttachments.Remove(fileInfo.Second);
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private static string GetAttachmentKey(IBrowserFile file) =>
        $"{file.Name}|{file.Size}|{file.LastModified.ToUnixTimeMilliseconds()}";

    private Task OnAttachmentsChanged(ICollection<StoredFileDto> attachments)
    {
        _task.Attachments = attachments;
        return Task.CompletedTask;
    }
}
