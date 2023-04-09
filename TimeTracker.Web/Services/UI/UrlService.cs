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

    private string _jwtToken => _authState?.Value.Jwt ?? "";

    public UrlService(
        IConfiguration configuration,
        IState<AuthState> authState,
        NavigationManager navigationManager
    )
    {
        _configuration = configuration;
        _authState = authState;
        _navigationManager = navigationManager;

        _apiUrl = _configuration.GetValue<string>("ApiUrl") ?? "";
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
    
    public void NavigateToChangeWorkspace(long workspaceId, string subUrl)
    {
        subUrl = subUrl.StartsWith("/") ? subUrl : $"/{subUrl}";
        _navigationManager.NavigateTo($"/board-change/{workspaceId}{subUrl}");
    }
}
