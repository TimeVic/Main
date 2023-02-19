using System.Collections.Specialized;
using System.Web;
using Fluxor;
using Microsoft.AspNetCore.WebUtilities;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Services.UI;

public class UrlService
{
    private readonly IConfiguration _configuration;
    private readonly IState<AuthState> _authState;
    
    private readonly string _apiUrl;

    private string _jwtToken => _authState?.Value.Jwt ?? "";

    public UrlService(
        IConfiguration configuration,
        IState<AuthState> authState
    )
    {
        _configuration = configuration;
        _authState = authState;

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
}
