using Autofac;
using Autofac.Extensions.DependencyInjection;
using OffLogs.Api.Extensions;
using Serilog;
using TimeTracker.Api;
using TimeTracker.Api.Di.Autofac.Modules;
using TimeTracker.Api.Middleware;
using TimeTracker.Business;
using TimeTracker.Business.Helpers;

using var log = ApplicationHelper.BuildSerilogInstance();
Log.Logger = log;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddAutoMapper(typeof(ApiAssemblyMarker).Assembly);
builder.Services.InitControllers();
builder.Services.InitAuthServices(builder.Configuration);

// Register services directly with Autofac here. Don't
// call builder.Populate(), that happens in AutofacServiceProviderFactory.
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterModule<ApiModule>()
        .RegisterAssemblyModules(typeof(BusinessAssemblyMarker).Assembly);
});

builder.Host.UseSerilog(log)
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureAppConfiguration(config =>
    {
        config.ConfigureConfigurationProvider();
    });

var app = builder.Build();
var configuration = app.Services.GetRequiredService<IConfiguration>();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSerilogRequestLogging();
}

var isRequestResponseLoggingEnabled = configuration.GetValue("App:EnableRequestResponseLogging", false);
if (isRequestResponseLoggingEnabled)
{
    app.UseMiddleware<RequestResponseLoggerMiddleware>();
}

app.UseAuthentication();
app.UseRouting();
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .AllowCredentials()
);
app.UseAuthorization();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.MapGet("/sum", (int? n1, int? n2) => n1 + n2);


app.Run();
