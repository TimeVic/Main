using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Ui.Pages.Landing.Shared;

public partial class MainHeaderBlock
{
    [Inject]
    protected IState<AuthState> AuthState { get; set; }
}
