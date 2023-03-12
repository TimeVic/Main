using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Shared.Components.Storage;

public partial class FileView
{
    [Parameter]
    public StoredFileDto File { get; set; }

    [Inject]
    public UrlService _urlService { get; set; }
    
    private static readonly ICollection<string> _mediaMimeTypes = new List<string>()
    {
        "image/jpeg",
        "image/png",
        "image/x-macpaint",
        "image/x-portable-anymap",
        "image/pict",
        "image/webp",
        "image/gif",
        // "video/mpeg",
        // "video/mp4",
        // "video/quicktime",
    };

    private bool IsMedia => _mediaMimeTypes.Contains(File.MimeType);
    
    private string _fileUrl => _urlService.GetStorageUrl(File.Url);
}
