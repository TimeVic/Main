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
            return builder.SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.${HostingEnvironment}.json", true)
                .AddJsonFile($"appsettings.Local.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}
