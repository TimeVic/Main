using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Services.UI.Toast;

namespace TimeTracker.Client.Mobile.Services.UI;

public class MobileToastService : IToastService
{
    public event Action? OnToastsUpdated
    {
        add { }
        remove { }
    }

    public IReadOnlyList<ToastMessage> ActiveToasts => Array.Empty<ToastMessage>();

    public void Dismiss(Guid id)
    {
    }
    public void ShowError(string summary)
    {
    }

    public void ShowInfo(string summary)
    {
    }

    public void ShowSuccess(string summary)
    {
    }

    public void ShowWarning(string summary)
    {
    }
}
