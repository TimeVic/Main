using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Business.Common.Constants.Storage;

namespace TimeTracker.Client.Web.Ui.Shared.Components.Storage;

public partial class FileDropzoneInput
{
    [Parameter]
    public string Accept { get; set; } = FileAcceptConstants.Default;

    [Parameter]
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10 MB

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public string MinHeightClass { get; set; } = "min-h-[120px]";

    [Parameter]
    public IBrowserFile? File { get; set; }

    [Parameter]
    public EventCallback<IBrowserFile?> FileChanged { get; set; }

    [Parameter]
    public EventCallback<InputFileChangeEventArgs> OnFilesSelected { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    private string? _errorMessage;

    private async Task OnInputFileChange(InputFileChangeEventArgs args)
    {
        _errorMessage = null;

        if (Multiple)
        {
            if (OnFilesSelected.HasDelegate)
            {
                await OnFilesSelected.InvokeAsync(args);
            }
            return;
        }

        var selected = args.File;
        if (selected == null)
        {
            return;
        }

        if (selected.Size > MaxFileSize)
        {
            var maxMb = MaxFileSize / (1024 * 1024);
            _errorMessage = string.Format(
                DashboardLocalizer["WorkspaceSettings_Import_FileSizeExceeded"].Value,
                maxMb
            );
            await FileChanged.InvokeAsync(null);
            return;
        }

        File = selected;
        await FileChanged.InvokeAsync(File);
    }

    private async Task OnRemoveFile()
    {
        _errorMessage = null;
        File = null;
        await FileChanged.InvokeAsync(null);
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }
        if (bytes < 1024 * 1024)
        {
            return $"{(bytes / 1024.0):F1} KB";
        }
        return $"{(bytes / (1024.0 * 1024.0)):F2} MB";
    }
}
