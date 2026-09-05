using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Web.Services.UI;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Web.Ui.Shared.Components.Storage;

public partial class FilesList
{
    [Parameter]
    public ICollection<StoredFileDto> Files { get; set; }

    [Parameter]
    public Guid? EntityId { get; set; }
    
    [Parameter]
    public StorageEntityType? EntityType { get; set; }
    
    [Parameter]
    public string Class { get; set; }
    
    [Parameter]
    public EventCallback<ICollection<StoredFileDto>> ListUpdated { get; set; }
    
    [Inject]
    public UiHelperService _uiHelperService { get; set; }
    
    [Inject]
    public UrlService _urlService { get; set; }
    
    [Inject]
    public ApiService _apiService { get; set; }
    
    [Inject]
    public ILogger<FilesList> _logger { get; set; }
    
    [Inject]
    public IToastService _toastService { get; set; }

    
    [Inject]
    protected IState<AuthState> _authState { get; set; }
    private async Task OnCLickDelete(StoredFileDto file)
    {
        // var isOk = await _dialogProvider.ShowDeleteConfirmationDialog(
        //     "Are you sure you want to delete this file?"
        // );
        // if (!isOk.HasValue || !isOk.Value)
        // {
        //     return;
        // }
        //
        // try
        // {
        //     await _apiService.StorageDeleteFileAsync(file.Id);
        //     var newList = Files.Where(item => item.Id != file.Id).ToList();
        //     await InvokeAsync(() => ListUpdated.InvokeAsync(newList));
        // }
        // catch (Exception e)
        // {
        //     _logger.LogError(e, e.Message);
        //     _toastService.ShowError(e.Message);
        // }
    }

    private string GetFullUrl(StoredFileDto file)
    {
        return _urlService.GetStorageFileUrl(file);
    }

    private async Task OnClickDownload(StoredFileDto storedFile)
    {
        await _uiHelperService.OpenFileInNewTab(
            storedFile.OriginalFileName,
            GetFullUrl(storedFile)
        );
    }
    
    private async Task OnClickView(StoredFileDto storedFile)
    {
        // await _dialogProvider.ShowFileView(storedFile);
    }
}
