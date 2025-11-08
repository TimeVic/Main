using Microsoft.FluentUI.AspNetCore.Components;

namespace TimeTracker.Web.Services.UI;

public class ToastService
{
    private readonly int _timeout = 3000;
    
    private readonly IToastService _toastService;

    public ToastService(IToastService toastService)
    {
        _toastService = toastService;
    }
    
    public void ShowError(string summary)
    {
        _toastService.ShowError(summary, _timeout);
    }
    
    public void ShowInfo(string summary)
    {
        _toastService.ShowInfo(summary, _timeout);
    }
    
    public void ShowSuccess(string summary)
    {
        _toastService.ShowSuccess(summary, _timeout);
    }
    
    public void ShowWarning(string summary)
    {
        _toastService.ShowWarning(summary, _timeout);
    }
}
