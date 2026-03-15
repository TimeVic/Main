namespace TimeTracker.Web.Pages.Chat.Shared;

public partial class Layout
{
    protected override async Task OnInitializedAsync()
    {
        IsRedirectIfNotLoggedIn = false;
        await base.OnInitializedAsync();
    }
}
