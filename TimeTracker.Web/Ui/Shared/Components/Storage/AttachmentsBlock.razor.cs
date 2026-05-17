using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Ui.Shared.Components.Storage;

public partial class AttachmentsBlock
{
    public sealed record UploadingAttachment(Guid Id, string FileName, string? MimeType);

    [Parameter]
    public ICollection<StoredFileDto> Files { get; set; } = new List<StoredFileDto>();

    [Parameter]
    public IReadOnlyCollection<UploadingAttachment> UploadingFiles { get; set; } = new List<UploadingAttachment>();

    [Parameter]
    public EventCallback<ICollection<StoredFileDto>> FilesChanged { get; set; }

    [Parameter]
    public Guid? EntityId { get; set; }

    [Parameter]
    public StorageEntityType? EntityType { get; set; }

    [Parameter]
    public bool AllowDelete { get; set; } = true;

    [Parameter]
    public bool HideWhenEmpty { get; set; } = true;

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Class { get; set; } = "rounded-2xl border border-slate-200 p-4 mt-3";

    [Parameter]
    public string ListClass { get; set; } = "grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4";

    [Parameter]
    public string PreviewItemClass { get; set; } = "relative flex aspect-square items-center justify-center overflow-hidden rounded-xl border border-slate-200 bg-slate-50 transition";

    [Inject]
    public UrlService _urlService { get; set; }

    [Inject]
    public ILogger<AttachmentsBlock> _logger { get; set; }

    private readonly HashSet<Guid> _deletingAttachmentIds = new();

    private bool IsVisible => !HideWhenEmpty || Files.Any() || UploadingFiles.Any();

    private string LocalizedTitle =>
        string.IsNullOrWhiteSpace(Title) ? DashboardLocalizer["Attachments"].Value : Title;

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

    private bool IsAttachmentDeleting(StoredFileDto attachment)
    {
        return _deletingAttachmentIds.Contains(attachment.Id);
    }

    private string GetUploadingAttachmentExtension(UploadingAttachment attachment)
    {
        return GetExtension(attachment.FileName).ToUpperInvariant();
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
            var updatedFiles = Files
                .Where(item => item.Id != attachment.Id)
                .ToList();
            Files = updatedFiles;
            await FilesChanged.InvokeAsync(updatedFiles);
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
        return _urlService.GetStorageFileUrl(attachment);
    }

    private string GetAttachmentPreviewUrl(StoredFileDto attachment)
    {
        return _urlService.GetStorageImageUrl(attachment, StorageImageSize.S_256);
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

}
