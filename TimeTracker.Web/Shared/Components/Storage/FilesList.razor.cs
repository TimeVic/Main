using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Shared.Components.Storage;

public partial class FilesList
{
    [Parameter]
    public ICollection<StoredFileDto> Files { get; set; }

    [Parameter]
    public string Class { get; set; }
    
    [Parameter]
    public EventCallback<long> FileDeleted { get; set; }
    
    [Inject]
    public UiHelperService _uiHelperService { get; set; }
    
    [Inject]
    public UrlService _urlService { get; set; }
    
    [Inject]
    public IApiService _apiService { get; set; }
    
    [Inject]
    public ILogger<FilesList> _logger { get; set; }
    
    [Inject]
    public NotificationService _toastService { get; set; }
    
    [Inject]
    protected DialogService _dialogService { get; set; }
    
    private async Task OnCLickDelete(StoredFileDto file)
    {
        var isOk = await _dialogService.Confirm(
            "Are you sure you want to delete this file?",
            "Delete confirmation",
            new ConfirmOptions()
            {
                OkButtonText = "Delete",
                CancelButtonText = "Cancel"
            }
        );
        if (!isOk.HasValue || !isOk.Value)
        {
            return;
        }
        
        try
        {
            await _apiService.StorageDeleteFileAsync(file.Id);
            await InvokeAsync(() => FileDeleted.InvokeAsync(file.Id));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            _toastService.Notify(new NotificationMessage()
            {
                Summary = e.Message,
                Severity = NotificationSeverity.Error,
                
            });
        }
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
