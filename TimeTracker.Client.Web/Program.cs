using System.Globalization;
using Blazored.LocalStorage;
using Fluxor;
using Majorsoft.Blazor.WebAssembly.Logging.Console;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Client.Web;
using TimeTracker.Client.Core.Services;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Web.Services;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Web.Services.Validation;
using TimeTracker.Client.Web.Services.Workspace;
using TimeTracker.Client.Core.Services.Http.Client;
using TimeTracker.Client.Core.Services.Http.Cookies;
using TimeTracker.Client.Core.Services.Http.Middleware;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using TimeTracker.Client.Core.Services.Messaging;
using TimeTracker.Client.Web.Services.Notification;
using ToastService = TimeTracker.Client.Web.Services.UI.ToastService;
using LumexUI.Extensions;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Services.DateTimes;
using TimeTracker.Client.Web.Services.UI;
using TimeTracker.Client.Web.Services.UI.Modal;
using TimeTracker.Client.Web.Services.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

var currentAssembly = typeof(Program).Assembly;
var defaultCulture = new CultureInfo("en");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var environment = builder.HostEnvironment.Environment;
Console.WriteLine($"Environment: {environment}");

// System services
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = builder.Configuration.GetValue<string>("ApiUrl");
builder.Services.AddScoped(sp => new HttpClient()
{
    BaseAddress = new Uri(apiUrl)
}.EnableIntercept(sp));

// Init Environment config file 
var webHttp = new HttpClient()
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
};
var configurationFile = "Debug";
#if IS_RELEASE_BUILD
    configurationFile = "Release";
#elif IS_DEVELOPMENT_BUILD
    configurationFile = "Development";
#elif IS_LOCAL_BUILD
    configurationFile = "Local";
#endif
Console.WriteLine($"Application loaded with {configurationFile} configuration");
using var response = await webHttp.GetAsync($"appsettings.{configurationFile}.json");
using var stream = await response.Content.ReadAsStreamAsync();
builder.Configuration.AddJsonStream(stream);

builder.Services.AddHttpClientInterceptor();
// Init local storage
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// // MudBlazor
// builder.Services.AddMudServices(config =>
// {
//     config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
//     config.SnackbarConfiguration.SnackbarVariant = MudBlazor.Variant.Filled;
//     config.SnackbarConfiguration.PreventDuplicates = false;
//     config.SnackbarConfiguration.NewestOnTop = false;
//     config.SnackbarConfiguration.ShowCloseIcon = true;
//     config.SnackbarConfiguration.VisibleStateDuration = 3000;
//     config.SnackbarConfiguration.HideTransitionDuration = 500;
//     config.SnackbarConfiguration.ShowTransitionDuration = 500;
// });

// Custom services
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<IApiService>(sp => sp.GetRequiredService<ApiService>());
builder.Services.AddScoped<HttpInterceptorService>();
builder.Services.AddScoped<CustomHttpClient>();
builder.Services.AddScoped<IAuthCookieConfigurator, WebAuthCookieConfigurator>();
builder.Services.AddScoped<IReCaptchaService, ReCaptchaService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<ITimeParsingService, TimeParsingService>();
builder.Services.AddScoped<ISecurityManager, SecurityManager>();
builder.Services.AddScoped<UiHelperService>();
builder.Services.AddScoped<UrlService>();
builder.Services.AddScoped<MarkdownService>();
builder.Services.AddScoped<WorkspaceInitializationService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<IToastService>(sp => sp.GetRequiredService<ToastService>());
builder.Services.AddScoped<FcmService>();
builder.Services.AddScoped<MessagingWebSocketClientService>();
builder.Services.AddScoped<ModalDialogService>();
builder.Services.AddScoped<UserDateTimeProviderService>();
builder.Services.AddScoped<ILocalizationUrlService, LocalizationUrlService>();
builder.Services.AddScoped<UserLocaleService>();
builder.Services.AddScoped<IUserLocaleService>(sp => sp.GetRequiredService<UserLocaleService>());
builder.Services.AddScoped<ISeoUrlService, SeoUrlService>();

// Lumex UI
builder.Services.AddLumexServices();

// Store
builder.Services.AddFluxor(
    options =>
    {
        options.ScanAssemblies(currentAssembly, typeof(AuthState).Assembly);
    }
);
            
#if DEBUG
// Init logger
builder.Logging.AddBrowserConsole()
    .SetMinimumLevel(LogLevel.Debug) //Setting LogLevel is optional
    .AddFilter("Microsoft", LogLevel.Information); //System logs can be filtered.
#endif

var host = builder.Build();

// Detect culture from URL path and apply it — /uk or /uk/* = uk-UA, otherwise = en
var navigationManager = host.Services.GetRequiredService<NavigationManager>();
var currentPath = new Uri(navigationManager.Uri).AbsolutePath;
var localizationUrlService = host.Services.GetRequiredService<ILocalizationUrlService>();
if (localizationUrlService.IsUkrainianPath(currentPath))
{
    localizationUrlService.ApplyCultureFromPath(currentPath);
}
else
{
    var storedCultureName = ILocalizationUrlService.EnglishCultureName;
    try
    {
        storedCultureName = await host.Services
            .GetRequiredService<IJSRuntime>()
            .InvokeAsync<string?>("localStorage.getItem", ILocalizationUrlService.LocalStorageKey) ?? ILocalizationUrlService.EnglishCultureName;
    }
    catch (Exception)
    {
        storedCultureName = ILocalizationUrlService.EnglishCultureName;
    }

    localizationUrlService.ApplyCulture(storedCultureName);
}

await host.RunAsync();
