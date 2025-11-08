using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Shared.Components.Storage;

public partial class FileViewModal
{
    public class Parameters
    {
        public required StoredFileDto File { get; set; }
    }
    
    [Parameter]
    public required Parameters Content { get; set; }

    [Inject]
    public UrlService _urlService { get; set; }
    
    [CascadingParameter] 
    public required FluentDialog MudDialog { get; set; }
    
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

    private bool IsMedia => _mediaMimeTypes.Contains(Content.File.MimeType);
    
    private string _fileUrl => _urlService.GetStorageUrl(Content.File.Url);
    
    private void OnCloseModal()
    {
        MudDialog.CloseAsync();
    }
}
