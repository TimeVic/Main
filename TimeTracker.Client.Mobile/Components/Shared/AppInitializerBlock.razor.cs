using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Common;

namespace TimeTracker.Client.Mobile.Components.Shared;

public partial class AppInitializerBlock : ComponentBase, IDisposable
{
    private const string StartupPath = "/";
    private const string LoginPath = "/login";
    private const string BoardPath = "/board";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Inject]
    private Fluxor.IDispatcher Dispatcher { get; set; } = null!;

    [Inject]
    private IState<CommonState> CommonState { get; set; } = null!;

    [Inject]
    private IState<AuthState> AuthState { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private IStringLocalizer<AppInitializerBlock> Localizer { get; set; } = null!;

    private bool IsContentReady =>
        CommonState.Value.IsInitialized
        && (AuthState.Value.IsLoggedIn || IsLoginPath);

    private bool IsLoginPath => CurrentPath.Equals(LoginPath, StringComparison.OrdinalIgnoreCase);

    private bool IsStartupPath => CurrentPath.Equals(StartupPath, StringComparison.OrdinalIgnoreCase);

    private string CurrentPath
    {
        get
        {
            var path = new Uri(NavigationManager.Uri).AbsolutePath;
            return string.IsNullOrWhiteSpace(path) ? StartupPath : path.TrimEnd('/') switch
            {
                "" => StartupPath,
                var normalizedPath => normalizedPath
            };
        }
    }

    protected override void OnInitialized()
    {
        CommonState.StateChanged += OnStateChanged;
        AuthState.StateChanged += OnStateChanged;

        if (!CommonState.Value.IsInitialized)
        {
            Dispatcher.Dispatch(new InitializeAppAction());
        }
        else
        {
            NavigateByAuthState();
        }
    }

    protected override void OnParametersSet()
    {
        NavigateByAuthState();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(() =>
        {
            NavigateByAuthState();
            StateHasChanged();
        });
    }

    private void NavigateByAuthState()
    {
        if (!CommonState.Value.IsInitialized)
        {
            return;
        }

        if (AuthState.Value.IsLoggedIn)
        {
            if (IsLoginPath || IsStartupPath)
            {
                NavigationManager.NavigateTo(BoardPath, replace: true);
            }

            return;
        }

        if (!IsLoginPath)
        {
            NavigationManager.NavigateTo(LoginPath, replace: true);
        }
    }

    public void Dispose()
    {
        CommonState.StateChanged -= OnStateChanged;
        AuthState.StateChanged -= OnStateChanged;
    }
}
