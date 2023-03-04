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
        try
        {
            await CreateHostBuilder(args).RunAsync();
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

    private static IHost CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog(Log.Logger)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .Build();
    }
}
