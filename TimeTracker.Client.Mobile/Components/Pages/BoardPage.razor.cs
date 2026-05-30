using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Mobile.Components.Pages;

public partial class BoardPage
{
    [Inject]
    private IState<AuthState> AuthState { get; set; } = null!;

    private string UserInitials => AuthState.Value.User?.Initials ?? "T";

    private string Greeting => string.Format(
        Localizer["Greeting_User"],
        AuthState.Value.User?.Name ?? Localizer["Greeting_DefaultUser"].Value
    );
}
