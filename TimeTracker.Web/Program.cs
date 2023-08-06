using Blazored.LocalStorage;
using Fluxor;
using Majorsoft.Blazor.WebAssembly.Logging.Console;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Web;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Services.Validation;
using TimeTracker.Web.Services.Workspace;

var currentAssembly = typeof(Program).Assembly;    
var builder = WebAssemblyHostBuilder.CreateDefault(args);

#if RE
    Console.WriteLine($"This is release build!");
#else
    Console.WriteLine($"This is other build!");
#endif

var environment = builder.HostEnvironment.Environment;
// System services
Console.WriteLine($"Application loaded in {environment} mode!");

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = builder.Configuration.GetValue<string>("ApiUrl");
builder.Services.AddScoped(sp => new HttpClient()
{
    BaseAddress = new Uri(apiUrl)
});

// Init Environment config file 
var webHttp = new HttpClient()
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
};
using var response = await webHttp.GetAsync($"appsettings.{environment}.json");
using var stream = await response.Content.ReadAsStreamAsync();
builder.Configuration.AddJsonStream(stream);

// Init local storage
builder.Services.AddBlazoredLocalStorage();

// Radzen services
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

// Custom services
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<IReCaptchaService, ReCaptchaService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<ITimeParsingService, TimeParsingService>();
builder.Services.AddScoped<ISecurityManager, SecurityManager>();
builder.Services.AddScoped<ModalDialogProviderService>();
builder.Services.AddScoped<UiHelperService>();
builder.Services.AddScoped<UrlService>();
builder.Services.AddScoped<MarkdownService>();
builder.Services.AddScoped<WorkspaceInitializationService>();
builder.Services.AddScoped<ToastService>();

// Store
builder.Services.AddFluxor(
    options =>
    {
        options.ScanAssemblies(currentAssembly);
#if DEBUG
        options.UseReduxDevTools();
#endif
    }
);
            
#if DEBUG
// Init logger
builder.Logging.AddBrowserConsole()
    .SetMinimumLevel(LogLevel.Debug) //Setting LogLevel is optional
    .AddFilter("Microsoft", LogLevel.Information); //System logs can be filtered.
#endif

await builder.Build().RunAsync();
