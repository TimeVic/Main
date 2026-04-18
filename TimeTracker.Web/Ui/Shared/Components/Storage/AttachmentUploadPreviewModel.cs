using Microsoft.AspNetCore.Components.Forms;

namespace TimeTracker.Web.Ui.Shared.Components.Storage;

public class AttachmentUploadPreviewModel
{
    public string Extension { get; set; } = string.Empty;
    public bool IsImage { get; set; }

    public static AttachmentUploadPreviewModel FromFile(IBrowserFile file)
    {
        return new AttachmentUploadPreviewModel
        {
            Extension = GetExtension(file.Name),
            IsImage = IsImageFile(file.ContentType, file.Name)
        };
    }

    private static bool IsImageFile(string? mimeType, string? fileName)
    {
        if (!string.IsNullOrWhiteSpace(mimeType) && mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var extension = GetExtension(fileName);
        return extension is "jpg" or "jpeg" or "png" or "gif" or "bmp" or "webp";
    }

    private static string GetExtension(string? fileName)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty).TrimStart('.');
        return string.IsNullOrWhiteSpace(extension) ? "FILE" : extension.ToLowerInvariant();
    }
}
