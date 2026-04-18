using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.WorkspaceMemberships;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks.Forms;

public partial class UpdateTaskForm: IDisposable
{
    private const int AttachmentsReloadIntervalMs = 3000;

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

    [Inject]
    public UrlService _urlService { get; set; }
    
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
    private System.Timers.Timer? _attachmentsReloadTimer;
    private string? _attachmentInteropId;
    private bool _isAttachmentInteropInitialized;
    private bool _isDragActive;
    private readonly ICollection<UploadingAttachmentModel> _uploadingAttachments = new List<UploadingAttachmentModel>();
    private readonly HashSet<Guid> _deletingAttachmentIds = new();
    public TaskFullDto _task { get; set; } = new();

    private string _attachmentAcceptTypes => string.Join(",", StoredFileType.Attachment.GetAllowedMimeTypes());
    private IEnumerable<StoredFileDto> _attachments => _task?.Attachments ?? Enumerable.Empty<StoredFileDto>();
    
    protected override async Task OnInitializedAsync()
    {
        _editContext = new EditContext(_model);

        _isLoading = true;
        _task = await ApiService.TasksGetOneAsync(TaskId);
        if (_task != null)
        {
            _model.Fill(_task);
        }
        await base.OnInitializedAsync();
        _editContext.OnFieldChanged += OnFormFieldChanged;
        _isLoading = false;

        StartAttachmentsReloadTimer();
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
        _attachmentsReloadTimer?.Dispose();
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

    private void OnAssignedChanged(WorkspaceMembershipDto? membership)
    {
        if (membership == null)
            return;
        _model.UserId = membership.Id;
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

        var files = eventArguments.GetMultipleFiles(eventArguments.FileCount).ToList();
        var uploadStates = files.Select(CreateUploadingAttachmentModel).ToList();
        foreach (var uploadState in uploadStates)
        {
            _uploadingAttachments.Add(uploadState);
        }
        await InvokeAsync(StateHasChanged);

        for (var fileIndex = 0; fileIndex < files.Count; fileIndex++)
        {
            var file = files[fileIndex];
            var uploadState = uploadStates[fileIndex];
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
                    ToastService.ShowError("Attachment upload error");
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
                _uploadingAttachments.Remove(uploadState);
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private UploadingAttachmentModel CreateUploadingAttachmentModel(IBrowserFile file)
    {
        return new UploadingAttachmentModel
        {
            Extension = GetExtension(file.Name),
            IsImage = IsImageAttachment(file.ContentType, file.Name)
        };
    }

    private bool IsImageAttachment(StoredFileDto attachment)
    {
        return IsImageAttachment(attachment.MimeType, attachment.OriginalFileName);
    }

    private bool IsImageAttachment(string? mimeType, string? fileName)
    {
        if (!string.IsNullOrWhiteSpace(mimeType) && mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var extension = GetExtension(fileName);
        return extension is "jpg" or "jpeg" or "png" or "gif" or "bmp" or "webp";
    }

    private bool IsAttachmentPending(StoredFileDto attachment)
    {
        return attachment.Status is StoredFileStatus.Pending or StoredFileStatus.Uploading;
    }

    private bool IsAttachmentDeleting(StoredFileDto attachment)
    {
        return _deletingAttachmentIds.Contains(attachment.Id);
    }

    private async Task OnDeleteAttachment(StoredFileDto attachment)
    {
        if (!_deletingAttachmentIds.Add(attachment.Id))
        {
            return;
        }

        await InvokeAsync(StateHasChanged);
        try
        {
            await ApiService.StorageDeleteFileAsync(attachment.Id);
            _task.Attachments = _task.Attachments
                .Where(item => item.Id != attachment.Id)
                .ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            ToastService.ShowError(e.Message);
        }
        finally
        {
            _deletingAttachmentIds.Remove(attachment.Id);
            await InvokeAsync(StateHasChanged);
        }
    }

    private string GetAttachmentUrl(StoredFileDto attachment)
    {
        return _urlService.GetStorageUrl(attachment.Url);
    }

    private string GetAttachmentPreviewUrl(StoredFileDto attachment)
    {
        return _urlService.GetStorageUrl(attachment.Url);
    }

    private string GetAttachmentExtension(StoredFileDto attachment)
    {
        if (!string.IsNullOrWhiteSpace(attachment.Extension))
        {
            return attachment.Extension.TrimStart('.').ToUpperInvariant();
        }

        return GetExtension(attachment.OriginalFileName).ToUpperInvariant();
    }

    private string GetExtension(string? fileName)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty).TrimStart('.');
        return string.IsNullOrWhiteSpace(extension) ? "FILE" : extension.ToLowerInvariant();
    }

    private void StartAttachmentsReloadTimer()
    {
        _attachmentsReloadTimer = new System.Timers.Timer(AttachmentsReloadIntervalMs);
        _attachmentsReloadTimer.Elapsed += OnAttachmentsReloadTimerTick;
        _attachmentsReloadTimer.Start();
    }

    private void OnAttachmentsReloadTimerTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (!_task.Attachments.Any(IsAttachmentPending))
        {
            return;
        }

        InvokeAsync(ReloadAttachments);
    }

    private async Task ReloadAttachments()
    {
        try
        {
            var files = await ApiService.StorageGetListAsync(
                _task.Id,
                StorageEntityType.Task
            );
            if (files != null)
            {
                _task.Attachments = files.Items;
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }

    private class UploadingAttachmentModel
    {
        public string Extension { get; set; } = string.Empty;
        public bool IsImage { get; set; }
    }
}
