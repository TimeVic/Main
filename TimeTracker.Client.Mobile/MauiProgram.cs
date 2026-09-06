using System.Net;
using Fluxor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Client.Core.Services;
using TimeTracker.Client.Core.Services.DateTimes;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.Http.Client;
using TimeTracker.Client.Core.Services.Http.Cookies;
using TimeTracker.Client.Core.Services.Http.Middleware;
using TimeTracker.Client.Core.Services.Messaging;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Core.Services.UI.Toast;
using TimeTracker.Client.Core.Store.Auth;

using TimeTracker.Client.Mobile.Services;
using TimeTracker.Client.Mobile.Services.Http;
using TimeTracker.Client.Mobile.Services.UI;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace TimeTracker.Client.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
        AddDefaultConfiguration(builder.Configuration);

        builder.Services.AddHttpClientInterceptor();
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        builder.Services.AddSingleton(new CookieContainer());
        builder.Services.AddScoped(sp =>
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = sp.GetRequiredService<CookieContainer>()
            };

            return new HttpClient(handler)
            {
                BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApiUrl")!)
            }.EnableIntercept(sp);
        });

        builder.Services.AddScoped<ApiService>();
        builder.Services.AddScoped<IApiService>(sp => sp.GetRequiredService<ApiService>());
        builder.Services.AddScoped<HttpInterceptorService>();
        builder.Services.AddScoped<CustomHttpClient>();
        builder.Services.AddScoped<IAuthCookieConfigurator, MobileAuthCookieConfigurator>();
        builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
        builder.Services.AddScoped<ITimeParsingService, TimeParsingService>();
        builder.Services.AddScoped<ISecurityManager, SecurityManager>();
        builder.Services.AddScoped<UiHelperService>();
        builder.Services.AddScoped<UrlService>();
        builder.Services.AddScoped<MarkdownService>();
        builder.Services.AddScoped<MessagingWebSocketClientService>();
        builder.Services.AddScoped<UserDateTimeProviderService>();
        builder.Services.AddScoped<IUserLocaleService, MobileUserLocaleService>();
        builder.Services.AddScoped<IToastService, MobileToastService>();
        builder.Services.AddFluxor(options =>
        {
            options.ScanAssemblies(typeof(MauiProgram).Assembly, typeof(AuthState).Assembly);
        });

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void AddDefaultConfiguration(ConfigurationManager configuration)
    {
        var defaults = new Dictionary<string, string?>
        {
            ["ApiUrl"] = "https://dev-api.timevic.com",
            ["MaxFileSize"] = "50",
            ["Auth:ExcludedApiUrls:0"] = "/user/login",
            ["Auth:ExcludedApiUrls:1"] = "/user/refresh-token",
            ["Auth:ExcludedApiUrls:2"] = "/user/check-is-logged-in",
            ["Auth:ExcludedApiUrls:3"] = "/user/registration/step1",
            ["Auth:ExcludedApiUrls:4"] = "/user/registration/step2",
            ["Auth:ExcludedApiUrls:5"] = "/user/password/reset",
            ["Auth:ExcludedApiUrls:6"] = "/user/password/change"
        };

        configuration.AddInMemoryCollection(
            defaults.Where(pair => string.IsNullOrWhiteSpace(configuration[pair.Key]))
        );
    }
}
