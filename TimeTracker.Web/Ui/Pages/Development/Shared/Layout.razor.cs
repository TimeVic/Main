using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Store.Common;

namespace TimeTracker.Web.Ui.Pages.Development.Shared;

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
