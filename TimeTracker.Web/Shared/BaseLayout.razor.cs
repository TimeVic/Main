using System;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Validation;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common;
using TimeTracker.Web.Store.Common.Actions;

namespace TimeTracker.Web.Shared;

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
            return path.Equals("/") 
                   || path.StartsWith("/login")
                   || path.StartsWith("/registration")
                   || path.StartsWith("/documentation");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        CommonState.StateChanged += async (sender, args) =>
        {
            if (CommonState.Value.IsInitialized)
            {
                await InitAppAsync();
            }
        };
        Dispatcher.Dispatch(new LoadPersistedDataAction());
    }

    private async Task InitAppAsync()
    {
        NavigationManager.LocationChanged += (sender, args) =>
        {
            CheckIsLoggedInAndRedirect();
        };
        AuthState.StateChanged += (sender, args) =>
        {
            CheckIsLoggedInAndRedirect();
        };
        ReCaptchaService.IsShowChanged += OnReCaptchaShowChanged;
        IsShowReCaptcha = ReCaptchaService.GetIsEnabled();
        
        await AuthService.CheckIsLoggedInAsync();
        CheckIsLoggedInAndRedirect();
    }

    public void Dispose()
    {
        ReCaptchaService.IsShowChanged -= OnReCaptchaShowChanged;
    }

    private void OnReCaptchaShowChanged(bool isShow)
    {
        IsShowReCaptcha = isShow;
    }
    
    private void CheckIsLoggedInAndRedirect()
    {
        if (!IsRedirectIfNotLoggedIn)
        {
            return;
        }

        if (!AuthState.Value.IsLoggedIn && !IsSharedPage)
        {
            NavigationManager.NavigateTo("/login");
        }
        StateHasChanged();
    }
}
