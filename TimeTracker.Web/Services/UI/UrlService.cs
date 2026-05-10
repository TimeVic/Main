using System.Collections.Specialized;
using System.Web;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Services.UI;

public class UrlService
{
    private readonly IConfiguration _configuration;
    private readonly IState<AuthState> _authState;
    private readonly NavigationManager _navigationManager;

    private readonly string _apiUrl;
    private readonly string _baseUrl;

    private string _jwtToken => _authState?.Value.JwtToken ?? string.Empty;

    public UrlService(
        IConfiguration configuration,
        IState<AuthState> authState,
        NavigationManager navigationManager
    )
    {
        _configuration = configuration;
        _authState = authState;
        _navigationManager = navigationManager;

        _apiUrl = _configuration.GetValue<string>("ApiUrl") ?? string.Empty;
        _baseUrl = (_configuration.GetValue<string>("BaseUrl") ?? _navigationManager.BaseUri).TrimEnd('/');
    }

    public string ToAbsoluteUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath == "/")
        {
            return $"{_baseUrl}/";
        }

        relativePath = relativePath.StartsWith("/") ? relativePath : $"/{relativePath}";
        return $"{_baseUrl}{relativePath}";
    }

    public string GetStorageUrl(string url)
    {
        var query = new Dictionary<string, string>
        {
            { "api_token", _jwtToken }
        };
        var uri = new Uri(QueryHelpers.AddQueryString($"{_apiUrl}{url}", query));
        return uri.ToString();
    }
    
    public void NavigateToChangeWorkspace(Guid workspaceId, string subUrl)
    {
        subUrl = subUrl.StartsWith("/") ? subUrl : $"/{subUrl}";
        _navigationManager.NavigateTo($"/board-change/{workspaceId}{subUrl}", replace: true);
    }
}
