using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Extensions.Autofac.DependencyInjection;
using TimeTracker.Business;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Notifications;

namespace TimeTracker.WorkerServices;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var log = ApplicationHelper.BuildSerilogInstance();
        Log.Logger = log;

        Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

        var configuration = ApplicationHelper.BuildConfiguration();
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterAssemblyModules(typeof(BusinessAssemblyMarker).Assembly);
        containerBuilder.RegisterAssemblyModules(typeof(BusinessNotificationsAssemblyMarker).Assembly);
        containerBuilder
            .RegisterInstance(ApplicationHelper.BuildConfiguration())
            .As<IConfiguration>()
            .SingleInstance();
        // Serilog
        var serilogConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration);
        containerBuilder.RegisterSerilog(serilogConfiguration);

        var container = containerBuilder.Build();

        var host1 = CreateHostBuilder<Services.QueueProcessingHostedService>(
            args,
            container,
            "queue_processing_scope"
        );
        var host2 = CreateHostBuilder<Services.ImageUploadingHostedService>(
            args,
            container,
            "storage_processing_scope"
        );
        try
        {
            await Task.WhenAll(
                host1.RunAsync(),
                host2.RunAsync()
            );
        }
        catch (Exception e)
        {
            log.Error(e, "Start application failed");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IHost CreateHostBuilder<THostedService>(
        string[] args,
        ILifetimeScope container,
        string scopeName
    ) where THostedService : class, IHostedService
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog(Log.Logger)
            .UseServiceProviderFactory(
                new AutofacChildLifetimeScopeServiceProviderFactory(container.BeginLifetimeScope(scopeName))
            )
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services => { services.AddHostedService<THostedService>(); });
                webBuilder.Configure(appBuilder => { });
            })
            .Build();
    }
}
