using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Extensions;

namespace TimeTracker.Client.Web.Ui.Shared.Components.Storage;

public partial class AttachmentsUploadBlock : IDisposable
{
    [Parameter] public Guid EntityId { get; set; }
    [Parameter] public StorageEntityType EntityType { get; set; }
    [Parameter] public ICollection<StoredFileDto> Files { get; set; } = new List<StoredFileDto>();
    [Parameter] public EventCallback<ICollection<StoredFileDto>> FilesChanged { get; set; }
    [Parameter] public string Hint { get; set; } = string.Empty;
    [Parameter] public string Class { get; set; } = string.Empty;

    [Inject] private ILogger<AttachmentsUploadBlock> Logger { get; set; } = null!;

    private readonly List<AttachmentsBlock.UploadingAttachment> _uploadingFiles = [];
    private bool _isDisposed;
    private string AcceptTypes => string.Join(",", StoredFileType.Attachment.GetAllowedMimeTypes());

    private async Task OnFilesSelected(InputFileChangeEventArgs args)
    {
        var selectedFiles = args.GetMultipleFiles(args.FileCount).ToList();
        var uploadingFiles = selectedFiles.Select(file => new AttachmentsBlock.UploadingAttachment(Guid.NewGuid(), file.Name, file.ContentType)).ToList();
        _uploadingFiles.AddRange(uploadingFiles);
        foreach (var item in selectedFiles.Zip(uploadingFiles))
        {
            try
            {
                var uploadedFile = await ApiService.StorageUploadFileAsync(EntityId, EntityType, StoredFileType.Attachment, item.First);
                if (_isDisposed)
                {
                    return;
                }

                if (uploadedFile != null)
                {
                    Files.Add(uploadedFile);
                    await FilesChanged.InvokeAsync(Files);
                }
            }
            catch (Exception exception)
            {
                if (_isDisposed)
                {
                    return;
                }

                Logger.LogError(exception, "Failed to upload attachment");
                ToastService.ShowError(exception.Message);
            }
            finally
            {
                _uploadingFiles.Remove(item.Second);
                if (!_isDisposed)
                {
                    await InvokeAsync(StateHasChanged);
                }
            }
        }
    }

    private Task OnFilesChanged(ICollection<StoredFileDto> files) => FilesChanged.InvokeAsync(files);

    public void Dispose()
    {
        // Prevent a completed upload from updating a modal that has already been closed.
        _isDisposed = true;
    }
}
