using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.UserPayment;

public partial class ClientShareReportModal
{
    [Parameter]
    public Guid ClientId { get; set; }

    [Parameter]
    public string ClientName { get; set; } = string.Empty;

    [Parameter]
    public bool IsOpened { get; set; }

    [Parameter]
    public EventCallback<bool> IsOpenedChanged { get; set; }

    private Guid _loadedClientId;
    private bool _isLoading;
    private bool _isActive;
    private bool _isShowTasks = true;
    private string _shareUrl = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        if (!IsOpened || ClientId == Guid.Empty || _loadedClientId == ClientId)
        {
            return;
        }

        _loadedClientId = ClientId;
        _isLoading = true;
        try
        {
            var settings = await ApiService.ReportsSetClientShareSettingsAsync(new ClientShareReportSettingsRequest
            {
                ClientId = ClientId,
                IsActive = false,
                IsShowTasks = true
            });
            ApplySettings(settings);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task OnIsActiveChanged(bool isActive)
    {
        _isActive = isActive;
        await SaveAsync();
    }

    private async Task OnIsShowTasksChanged(bool isShowTasks)
    {
        _isShowTasks = isShowTasks;
        await SaveAsync();
    }

    private async Task RegenerateAsync()
    {
        await SaveAsync(isRegenerateToken: true);
        ToastService.ShowSuccess(DashboardLocalizer["ClientShareReport_Regenerated"].Value);
    }

    private async Task CopyAsync()
    {
        if (await UiHelperService.CopyToClipboard(_shareUrl))
        {
            ToastService.ShowSuccess(DashboardLocalizer["ClientShareReport_Copied"].Value);
        }
    }

    private async Task SaveAsync(bool isRegenerateToken = false)
    {
        var settings = await ApiService.ReportsSetClientShareSettingsAsync(new ClientShareReportSettingsRequest
        {
            ClientId = ClientId,
            IsActive = _isActive,
            IsShowTasks = _isShowTasks,
            IsUpdateSettings = true,
            IsRegenerateToken = isRegenerateToken
        });
        ApplySettings(settings);
    }

    private void ApplySettings(ClientShareReportSettingsResponse? settings)
    {
        if (settings == null)
        {
            return;
        }

        _isActive = settings.IsActive;
        _isShowTasks = settings.IsShowTasks;
        _shareUrl = settings.ShareUrl;
    }

    private async Task OnCloseModal()
    {
        _loadedClientId = Guid.Empty;
        await IsOpenedChanged.InvokeAsync(false);
    }
}
