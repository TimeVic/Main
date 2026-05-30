namespace TimeTracker.Client.Core.Services.UI;

public interface IToastService
{
    void ShowError(string summary);

    void ShowInfo(string summary);

    void ShowSuccess(string summary);

    void ShowWarning(string summary);
}
