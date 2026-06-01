using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Web.Constants;

namespace TimeTracker.Client.Web.Ui.Pages.Landing.User.Shared;

public partial class SocialLoginButtonsBlock
{
    [Inject]
    protected IConfiguration Configuration { get; set; } = null!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    protected UrlService UrlService { get; set; } = null!;

    [Inject]
    protected IStringLocalizer<SocialLoginButtonsBlock> Localizer { get; set; } = null!;

    [Parameter]
    public string? Class { get; set; }

    protected string ContainerClass => string.IsNullOrWhiteSpace(Class) ? "space-y-3" : $"{Class} space-y-3";

    protected string GoogleLoginUrl => BuildLoginUrl("Google");

    protected string FacebookLoginUrl => BuildLoginUrl("Facebook");

    private string BuildLoginUrl(string providerName)
    {
        var apiUrl = (Configuration.GetValue<string>("ApiUrl") ?? string.Empty).TrimEnd('/');
        var signInUrl = $"{apiUrl}/integration/social/SignIn/{providerName}";
        var dashboardUrl = UrlService.ToAbsoluteUrl(SiteUrl.DashboardBase);
        var currentUrl = NavigationManager.Uri;

        return QueryHelpers.AddQueryString(
            signInUrl,
            new Dictionary<string, string?>
            {
                ["returnUrl"] = dashboardUrl,
                ["registrationReturnUrl"] = dashboardUrl,
                ["errorReturnUrl"] = currentUrl
            });
    }
}
