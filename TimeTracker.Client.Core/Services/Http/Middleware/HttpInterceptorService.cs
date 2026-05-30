using System.Net;
using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Client.Core.Store.Auth;
using Toolbelt.Blazor;

namespace TimeTracker.Client.Core.Services.Http.Middleware;

public class HttpInterceptorService
{
    private readonly HttpClientInterceptor _interceptor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HttpInterceptorService> _logger;
    private readonly NavigationManager _navigationManager;
    private readonly IDispatcher _dispatcher;
    private readonly IState<AuthState> _authState;

    private string[] ExcludedUrls => _configuration.GetSection("Auth:ExcludedApiUrls").Get<string[]>() ?? [];

    public HttpInterceptorService(
        HttpClientInterceptor interceptor,
        IConfiguration configuration,
        ILogger<HttpInterceptorService> logger,
        NavigationManager navigationManager,
        IDispatcher dispatcher,
        IState<AuthState> authState
    )
    {
        _interceptor = interceptor;
        _configuration = configuration;
        _logger = logger;
        _navigationManager = navigationManager;
        _dispatcher = dispatcher;
        _authState = authState;
    }

    public void Register()
    {
        _interceptor.BeforeSendAsync += RefreshAuthTokenAsync;
        _interceptor.AfterSendAsync += CheckResponseAsync;
    }

    public void Unregister()
    {
        _interceptor.BeforeSendAsync -= RefreshAuthTokenAsync;
        _interceptor.AfterSendAsync -= CheckResponseAsync;
    }

    private async Task RefreshAuthTokenAsync(object sender, HttpClientInterceptorEventArgs e)
    {
        var absPath = e.Request.RequestUri!.AbsolutePath;
        var isExcludedUrl = ExcludedUrls.Any(excludedUrl => absPath.StartsWith(excludedUrl));
        if (!isExcludedUrl)
        {
            if (_authState.Value.Workspace != null)
            {
                e.Request.Headers.Remove(AuthConstants.WorkspaceIdHeaderName);
                e.Request.Headers.Add(AuthConstants.WorkspaceIdHeaderName, _authState.Value.Workspace.Id.ToString());
            }
        }
    }

    private async Task CheckResponseAsync(object sender, HttpClientInterceptorEventArgs e)
    {
        if (e.Response.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (_authState.Value.IsLoggedIn)
            {
                _dispatcher.Dispatch(new LogoutAction());
            }
            if (!_navigationManager.Uri.EndsWith(ClientSiteUrl.Login))
            {
                _navigationManager.NavigateTo(ClientSiteUrl.Login);
            }
        }
        await Task.CompletedTask;
    }
}
