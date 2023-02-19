using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Shared.Components.Storage;

public partial class FilesList
{
    [Parameter]
    public ICollection<StoredFileDto> Files { get; set; }

    [Parameter]
    public string Class { get; set; }
    
    [Inject]
    public UiHelperService _uiHelperService { get; set; }
    
    [Inject]
    public UrlService _urlService { get; set; }
    
    private async Task OnCLickDelete()
    {
        // TODO: Add deletion handler
        await Task.CompletedTask;
    }

    private string GetFullUrl(string url)
    {
        return _urlService.GetStorageUrl(url);
    }

    private async Task OnClickDownload(StoredFileDto storedFile)
    {
        await _uiHelperService.OpenFileInNewTab(
            storedFile.OriginalFileName,
            GetFullUrl(storedFile.Url)
        );
    }
}
