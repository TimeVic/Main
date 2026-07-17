using System.Globalization;
using ElectronNET;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Server;
using LumexUI.Extensions;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Web.Services;
using TimeTracker.Client.Web.Server.Components;

var defaultCulture = new CultureInfo("en");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<ILocalizationUrlService, LocalizationUrlService>();
builder.Services.AddScoped<ISeoUrlService, SeoUrlService>();
builder.Services.AddScoped<UrlService>();
builder.Services.AddLumexServices();
builder.Services.AddElectron();

builder.UseElectron(args, async () =>
{
    var options = new BrowserWindowOptions
    {
        Show = false,
        IsRunningBlazor = true
    };

    if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
    {
        options.AutoHideMenuBar = true;
    }

    var browserWindow = await Electron.WindowManager.CreateWindowAsync(options);
    browserWindow.OnReadyToShow += () => browserWindow.Show();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.Use(async (httpContext, next) =>
{
    var localizationUrlService = httpContext.RequestServices.GetRequiredService<ILocalizationUrlService>();
    var currentPath = httpContext.Request.Path.HasValue ? httpContext.Request.Path.Value! : "/";
    localizationUrlService.ApplyCultureFromPath(currentPath);

    await next();
});

app.MapRazorComponents<TimeTracker.Client.Web.Server.Components.App>()
    .AddAdditionalAssemblies(typeof(TimeTracker.Client.Web.App).Assembly)
    .AddInteractiveWebAssemblyRenderMode();

app.MapStaticAssets();

app.Run();
