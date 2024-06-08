using MudBlazor;

namespace TimeTracker.Web.Services.UI;

public class ToastService
{
    private readonly ISnackbar _snackbar;

    public ToastService(ISnackbar snackbar)
    {
        _snackbar = snackbar;
    }

    public Task Show(Severity severity, string summary)
    {
        _snackbar.Add(summary, severity, options =>
        {
            options.VisibleStateDuration = 3000;
        });
        return Task.CompletedTask;
    }
    
    public async Task ShowError(string summary)
    {
        await Show(Severity.Error, summary);
    }
    
    public async Task ShowInfo(string summary)
    {
        await Show(Severity.Info, summary);
    }
    
    public async Task ShowSuccess(string summary)
    {
        await Show(Severity.Success, summary);
    }
    
    public async Task ShowWarning(string summary)
    {
        await Show(Severity.Warning, summary);
    }
}
