using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Shared.Components.Storage;

public partial class FilesList
{
    [Parameter]
    public ICollection<StoredFileDto> Files { get; set; }

    [Inject]
    public UiHelperService _uiHelperService { get; set; }
    
    private async Task OpenFile(StoredFileDto storedFile)
    {
        await _uiHelperService.OpenFileInNewTab(storedFile.OriginalFileName, storedFile.Url);
    }
    
    private async Task OnCLickDelete()
    {
        // TODO: Add deletion handler
        await Task.CompletedTask;
    }
}
