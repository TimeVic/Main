namespace TimeTracker.Web.Services.UI;

public class ToastService
{
    private readonly int _timeout = 3000;
    public readonly string ToasterId = "tv-toaster-rich-colors-id";
    
    private readonly Blazor.Sonner.Services.ToastService _toastService;

    public ToastService(Blazor.Sonner.Services.ToastService toastService)
    {
        _toastService = toastService;
    }
    
    public void ShowError(string summary)
    {
        _toastService.Error(summary, options: model =>
        {
            model.ToasterId = ToasterId;
        });
    }
    
    public void ShowInfo(string summary)
    {
        _toastService.Info(summary, options: model =>
        {
            model.ToasterId = ToasterId;
        });
    }
    
    public void ShowSuccess(string summary)
    {
        _toastService.Success(summary, options: model =>
        {
            model.ToasterId = ToasterId;
        });
    }
    
    public void ShowWarning(string summary)
    {
        _toastService.Warning(summary, options: model =>
        {
            model.ToasterId = ToasterId;
        });
    }
}
