using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Shared.Components.Storage;

public partial class FileUploader
{
    private const int MaxFiles = 3; 
    
    [Parameter]
    public long EntityId { get; set; }
    
    [Parameter]
    public StorageEntityType EntityType { get; set; }
    
    [Parameter]
    public StoredFileType FileType { get; set; }

    [Parameter]
    public EventCallback<StoredFileDto> FileUploaded { get; set; }
    
    [Parameter]
    public string Class { get; set; }
    
    [Inject]
    public IApiService _apiService { get; set; }
    
    [Inject]
    public NotificationService _toastService { get; set; }
    
    [Inject]
    public ILogger<FileUploader> _logger { get; set; }
    
    [Inject]
    public UiHelperService _uiHelperService { get; set; }

    public InputFile _fileInput { get; set; }
    public bool _isLoading = false;

    public string _acceptTypes
    {
        get => string.Join(",", FileType.GetAllowedMimeTypes());
    }

    public string _buttonLabel
    {
        get
        {
            if (_isLoading)
            {
                return "Uploading...";
            }

            return "Select files";
        }
    }

    private async Task OnInputFileChange(InputFileChangeEventArgs eventArguments)
    {
        if (eventArguments.FileCount > 0)
        {
            _isLoading = true;
            foreach (var file in eventArguments.GetMultipleFiles(MaxFiles))
            {
                try
                {
                    var uploadedFileDto = await ApiService.StorageUploadFileAsync(
                        EntityId,
                        EntityType,
                        FileType,
                        file
                    );
                    await InvokeAsync(() => FileUploaded.InvokeAsync(uploadedFileDto));
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
            _isLoading = false;
        }
    }

    private async Task OnClickSelectFile()
    {
        await _uiHelperService.SimulateClick(_fileInput.Element.Value);
    }
}
