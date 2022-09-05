using Blazored.LocalStorage;
using Fluxor;
using Majorsoft.Blazor.WebAssembly.Logging.Console;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using TimeTracker.Business.Common.Services.Web.ReCaptcha;
using TimeTracker.Web;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Http;

var currentAssembly = typeof(Program).Assembly;    
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = builder.Configuration.GetValue<string>("ApiUrl");
// System services
builder.Services.AddScoped(
    sp => new HttpClient
    {
        BaseAddress = new Uri(apiUrl)
    }
);
builder.Services.AddBlazoredLocalStorage();

// Radzen services
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

// Custom services
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IReCaptchaService, ReCaptchaService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

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
