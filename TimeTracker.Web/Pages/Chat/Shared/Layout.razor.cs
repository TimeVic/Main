using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Store.Common;

namespace TimeTracker.Web.Pages.Chat.Shared;

public partial class Layout
{
    [Inject]
    protected IState<CommonState> CommonState { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        IsRedirectIfNotLoggedIn = false;
        await base.OnInitializedAsync();
    }
}
