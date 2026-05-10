using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Validation;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common;

namespace TimeTracker.Web.Ui.Shared;

public partial class BaseLayout
{
    [Inject]
    protected IAuthorizationService AuthService { get; set; }
    
    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    [Inject]
    protected IReCaptchaService ReCaptchaService { get; set; }

    [Inject]
    protected IState<AuthState> AuthState { get; set; }
    
    [Inject]
    protected IState<CommonState> CommonState { get; set; }
    
    [Inject]
    protected IDispatcher Dispatcher { get; set; }
    
    protected bool IsRedirectIfNotLoggedIn = true;

    protected bool IsShowMainMenu => AuthState.Value.IsLoggedIn
        && NavigationManager.GetPath().ToLower().StartsWith(SiteUrl.DashboardBase);

    protected bool IsShowReCaptcha = false;

    private bool IsSharedPage
    {
        get
        {
            var path = NavigationManager.GetPath();
            // Strip /uk prefix to normalize Ukrainian locale paths before matching
            var normalizedPath = path == SiteUrl.UkLocalePrefix || path == SiteUrl.UkLocalePrefix + "/" ? "/"
                : path.StartsWith(SiteUrl.UkLocalePrefix + "/") ? path[SiteUrl.UkLocalePrefix.Length..]
                : path;
            return normalizedPath.Equals("/")
                || normalizedPath.StartsWith(SiteUrl.UseCases)
                || normalizedPath.StartsWith(SiteUrl.Faq)
                || normalizedPath.StartsWith(SiteUrl.Pricing)
                || normalizedPath.StartsWith(SiteUrl.Login)
                || normalizedPath.StartsWith(SiteUrl.LoginAsDemo)
                || normalizedPath.StartsWith(SiteUrl.Registration_Step1)
                || normalizedPath.StartsWith("/registration")
                || normalizedPath.StartsWith("/documentation");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        CommonState.StateChanged += async (sender, args) =>
        {
            if (CommonState.Value.IsInitialized)
            {
                InitAppAsync();
                IsShowReCaptcha = ReCaptchaService.GetIsEnabled();
                StateHasChanged();
            }
        };
        ReCaptchaService.IsShowChanged += OnReCaptchaShowChanged;
        Dispatcher.Dispatch(new LoadPersistedDataAction());
    }

    public void Dispose()
    {
        ReCaptchaService.IsShowChanged -= OnReCaptchaShowChanged;
    }
    
    private void OnReCaptchaShowChanged(bool isShow)
    {
        IsShowReCaptcha = isShow;
    }
    
    private void InitAppAsync()
    {
        NavigationManager.LocationChanged += (sender, args) =>
        {
            CheckIsLoggedInAndRedirect();
        };
        AuthState.StateChanged += async (sender, args) =>
        {
            CheckIsLoggedInAndRedirect();
            if (AuthState.Value.IsLoggedIn)
            {
                await OnLoggedInAsync();
            }
        };
        CommonState.StateChanged += async (sender, args) =>
        {
            if (CommonState.Value.IsWorkspaceInitialized)
            {
                // await AuthService.CheckIsLoggedInAsync();
                CheckIsLoggedInAndRedirect();    
            }
        };
        
        
        if (AuthState.Value.IsLoggedIn)
        {
            OnLoggedInAsync();
        }
    }
    
    private void CheckIsLoggedInAndRedirect()
    {
        if (!IsRedirectIfNotLoggedIn)
        {
            return;
        }

        if (!AuthState.Value.IsLoggedIn && !IsSharedPage)
        {
            NavigationManager.NavigateTo(SiteUrl.Login);
        }
    }
    
    protected virtual Task OnLoggedInAsync()
    {
        return Task.CompletedTask;
    }
}
