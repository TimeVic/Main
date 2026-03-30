using Blazored.LocalStorage;
using Fluxor;
using Majorsoft.Blazor.WebAssembly.Logging.Console;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Web;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Services.Validation;
using TimeTracker.Web.Services.Workspace;
using TimeTracker.Web.Services.Http.Auth;
using TimeTracker.Web.Services.Http.Client;
using TimeTracker.Web.Services.Http.Middleware;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using TimeTracker.Web.Services.Messaging;
using TimeTracker.Web.Services.Notification;
using ToastService = TimeTracker.Web.Services.UI.ToastService;
using LumexUI.Extensions;
using TimeTracker.Web.Services.UI.Modal;

var currentAssembly = typeof(Program).Assembly;    
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
builder.Services.AddScoped<RefreshJwtTokenService>();
builder.Services.AddScoped<HttpInterceptorService>();
builder.Services.AddScoped<CustomHttpClient>();
builder.Services.AddScoped<IReCaptchaService, ReCaptchaService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<ITimeParsingService, TimeParsingService>();
builder.Services.AddScoped<ISecurityManager, SecurityManager>();
builder.Services.AddScoped<UiHelperService>();
builder.Services.AddScoped<UrlService>();
builder.Services.AddScoped<MarkdownService>();
builder.Services.AddScoped<WorkspaceInitializationService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<FcmService>();
builder.Services.AddScoped<MessagingWebSocketClientService>();
builder.Services.AddScoped<ModalDialogService>();

// Lumex UI
builder.Services.AddLumexServices();

// Store
builder.Services.AddFluxor(
    options =>
    {
        options.ScanAssemblies(currentAssembly);
    }
);
            
#if DEBUG
// Init logger
builder.Logging.AddBrowserConsole()
    .SetMinimumLevel(LogLevel.Debug) //Setting LogLevel is optional
    .AddFilter("Microsoft", LogLevel.Information); //System logs can be filtered.
#endif

var host = builder.Build();
await host.RunAsync();
