namespace TimeTracker.Web.Ui.Pages.Dashboard.User.Settings.Components;

public partial class DangerZoneBlock
{
    private bool _isShowDeletionModal = false;

    private void OnRequestDeletionConfirmed()
    {
        // TODO: Wire to account deletion request API
        _isShowDeletionModal = false;
    }
}
