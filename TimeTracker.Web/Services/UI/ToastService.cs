using Radzen;

namespace TimeTracker.Web.Services.UI;

public class ToastService
{
    private readonly NotificationService _notificationService;

    public ToastService(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task Show(NotificationSeverity severity, string summary)
    {
        _notificationService.Notify(new NotificationMessage()
        {
            Severity = severity,
            Summary = summary,
            CloseOnClick = true,
            Duration = 50000
        });
        return Task.CompletedTask;
    }
    
    public async Task ShowError(string summary)
    {
        await Show(NotificationSeverity.Error, summary);
    }
    
    public async Task ShowInfo(string summary)
    {
        await Show(NotificationSeverity.Info, summary);
    }
    
    public async Task ShowSuccess(string summary)
    {
        await Show(NotificationSeverity.Success, summary);
    }
    
    public async Task ShowWarning(string summary)
    {
        await Show(NotificationSeverity.Warning, summary);
    }
}
