namespace TimeTracker.Client.Web.Ui.Pages.Landing.Shared;

public partial class LandingLayout
{
    protected override async Task OnInitializedAsync()
    {
        IsRedirectIfNotLoggedIn = false;
        await base.OnInitializedAsync();
    }
}
