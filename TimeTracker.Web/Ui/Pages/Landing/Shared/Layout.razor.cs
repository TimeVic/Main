namespace TimeTracker.Web.Ui.Pages.Landing.Shared;

public partial class Layout
{
    protected override async Task OnInitializedAsync()
    {
        IsRedirectIfNotLoggedIn = false;
        await base.OnInitializedAsync();
    }
}
