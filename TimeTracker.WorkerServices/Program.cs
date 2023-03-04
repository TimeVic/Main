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

        var containerBuilder = new ContainerBuilder();
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
                host2.RunAsync(),
                host1.RunAsync()
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
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup<THostedService>>();
            })
            .Build();
    }
}
