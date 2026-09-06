using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace.TimeEntry;
using TimeTracker.Business.Common.Constants.Import;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Web.Services.UI;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Import;

public partial class ImportBlock
{
    [Inject]
    private IApiService ApiService { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;


    [Inject]
    private ILogger<ImportBlock> Logger { get; set; } = null!;

    private TimeEntryImportSourceType? _selectedSource = TimeEntryImportSourceType.Clockify;
    private bool _isBillable;
    private string _hourlyRateString = string.Empty;
    private IBrowserFile? _selectedFile;
    private bool _isImporting;
    private ImportResponse? _importResult;

    private string GetSourceIcon(TimeEntryImportSourceType source) => source switch
    {
        TimeEntryImportSourceType.Clockify => "fa-solid fa-clock text-blue-500",
        TimeEntryImportSourceType.Toggl => "fa-solid fa-hourglass-half text-rose-500",
        _ => string.Empty
    };

    private async Task OnImportClick()
    {
        if (_selectedFile == null)
        {
            return;
        }

        _isImporting = true;
        try
        {
            decimal? hourlyRate = null;
            if (!string.IsNullOrWhiteSpace(_hourlyRateString)
                && decimal.TryParse(_hourlyRateString.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedRate))
            {
                hourlyRate = parsedRate;
            }

            var response = await ApiService.WorkspaceTimeEntryImportAsync(
                _selectedSource ?? TimeEntryImportSourceType.Clockify,
                _isBillable,
                hourlyRate,
                _selectedFile
            );

            if (response != null)
            {
                _importResult = response;
                ToastService.ShowSuccess(
                    string.Format(
                        DashboardLocalizer["WorkspaceSettings_Import_Success_Message"].Value,
                        response.ImportedCount,
                        response.SkippedCount
                    )
                );
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to import time entries CSV: {Message}", ex.Message);
            ToastService.ShowError(ex.Message);
        }
        finally
        {
            _isImporting = false;
        }
    }

    private void OnResetImport()
    {
        _selectedFile = null;
        _importResult = null;
    }
}
