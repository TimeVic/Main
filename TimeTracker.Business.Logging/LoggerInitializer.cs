using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Graylog;
using TransportType = Serilog.Sinks.Graylog.Core.Transport.TransportType;

namespace TimeTracker.Business.Logging;

public static class LoggerInitializer
{
    public static LoggerConfiguration GetSerilogBuilder(bool isEnableInitLogging = true)
    {
        var environment = GetHostingEnvironment();
        var configuration = BuildConfiguration(environment);
        var logBuilder = new LoggerConfiguration().ReadFrom.Configuration(configuration);

        if (IsRunningFromXUnit())
        {
            return logBuilder;
        }

        if (isEnableInitLogging)
        {
            Log.Information("Init Serilog configuration for {HostingEnvironment} environment", environment);
        }

        logBuilder.Enrich.WithProperty("Environment", environment);
        logBuilder.Enrich.WithProperty("AppName", configuration.GetValue<string>("App:Name"));

        var grayLogHost = configuration.GetValue<string>("App:Logging:GrayLog:Host");
        if (!string.IsNullOrWhiteSpace(grayLogHost))
        {
            logBuilder.WriteTo.Graylog(new GraylogSinkOptions
            {
                HostnameOrAddress = grayLogHost,
                Port = configuration.GetValue<int>("App:Logging:GrayLog:Port", 12201),
                MinimumLogEventLevel = LogEventLevel.Information,
                TransportType = TransportType.Udp
            });
        }

        return logBuilder;
    }

    public static Logger BuildSerilogInstance()
    {
        return GetSerilogBuilder().CreateLogger();
    }

    private static IConfigurationRoot BuildConfiguration(string environment)
    {
        var isTestingEnvironment = environment == "Testing";
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: !isTestingEnvironment)
#if DEBUG
            .AddJsonFile("appsettings.Debug.json", optional: true, reloadOnChange: !isTestingEnvironment)
#endif
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: !isTestingEnvironment);

        if (isTestingEnvironment)
        {
            configurationBuilder.AddJsonFile("appsettings.Testing.json", optional: true, reloadOnChange: false);
        }

        configurationBuilder
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: !isTestingEnvironment)
            .AddEnvironmentVariables();

        return configurationBuilder.Build();
    }

    private static string GetHostingEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return string.IsNullOrWhiteSpace(environment) ? "Development" : environment;
    }

    private static bool IsRunningFromXUnit()
    {
        return AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
            assembly.FullName?.StartsWith("xunit.runner", StringComparison.OrdinalIgnoreCase) == true);
    }
}
