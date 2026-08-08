using Microsoft.Extensions.Configuration;
using Serilog;
using TimeTracker.Business.Common.Utils;

namespace TimeTracker.Business.Helpers
{
    public static class ApplicationHelper
    {
        public static string HostingEnvironment
        {
            get
            {
                var value = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                return string.IsNullOrEmpty(value) ? "Development" : value;
            }

        }
        
        public static IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .ConfigureConfigurationProvider()
                .Build();
        }

        public static IConfigurationBuilder ConfigureConfigurationProvider(this IConfigurationBuilder builder)
        {
            Log.Logger.Information($"Initializing configuration with \"{HostingEnvironment}\" environment");
            var basePath = AssemblyUtils.GetAssemblyPath();
            var isTestingEnvironment = HostingEnvironment == "Testing";
            builder.SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: !isTestingEnvironment)
#if DEBUG
                .AddJsonFile("appsettings.Debug.json", true, !isTestingEnvironment)
#endif
                .AddJsonFile($"appsettings.{HostingEnvironment}.json", true, !isTestingEnvironment);

            if (isTestingEnvironment)
            {
                builder.AddJsonFile("appsettings.Testing.json", optional: true, reloadOnChange: false);
            }

            builder
                .AddJsonFile($"appsettings.Local.json", optional: true, reloadOnChange: !isTestingEnvironment)
                .AddEnvironmentVariables();

            return builder;
        }
    }
}
