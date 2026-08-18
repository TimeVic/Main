namespace TimeTracker.Client.Web.Ui.Pages.SharedReport;

public partial class SharedClientReportLayout
{
    protected override async Task OnInitializedAsync()
    {
        IsRedirectIfNotLoggedIn = false;
        await base.OnInitializedAsync();
    }
}
