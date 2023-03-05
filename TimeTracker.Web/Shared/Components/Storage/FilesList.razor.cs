using System.Timers;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Shared.Components.Storage;

public partial class FilesList: IDisposable
{
    [Parameter]
    public ICollection<StoredFileDto> Files { get; set; }

    [Parameter]
    public long? EntityId { get; set; }
    
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
    public NotificationService _toastService { get; set; }
    
    [Inject]
    protected DialogService _dialogService { get; set; }
    
    private System.Timers.Timer _timer;

    private bool _isReloadFiles
    {
        get
        {
            var isHasPending = Files.Any(item => item.Status == StoredFileStatus.Pending);
            return isHasPending && EntityId.HasValue && EntityType.HasValue;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _timer = new System.Timers.Timer();
        _timer.Interval = 3000;
        _timer.Elapsed += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        if (!_isReloadFiles)
        {
            return;
        }

        InvokeAsync(async () => await ReloadList());
    }

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
            var newList = Files.Where(item => item.Id != file.Id).ToList();
            await InvokeAsync(() => ListUpdated.InvokeAsync(newList));
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
    
    private async Task ReloadList()
    {
        try
        {
            var files = await _apiService.StorageGetListAsync(EntityId.Value, EntityType.Value);
            await InvokeAsync(() => ListUpdated.InvokeAsync(files.Items));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }
    
    public void Dispose()
    {
        _timer.Dispose();
    }
}
