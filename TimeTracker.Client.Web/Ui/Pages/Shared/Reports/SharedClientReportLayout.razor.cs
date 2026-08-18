namespace TimeTracker.Client.Web.Ui.Pages.Shared.Reports;

public partial class SharedClientReportLayout
{
    protected override async Task OnInitializedAsync()
    {
        IsRedirectIfNotLoggedIn = false;
        await base.OnInitializedAsync();
    }
}
