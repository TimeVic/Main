using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TimeTracker.Api.FileStorage;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Integration.Api.FileStorage;

public class ApiCustomWebApplicationFactory: WebApplicationFactory<TestStartup>
{
    public void ConfigureServices(IServiceCollection services)
    {
        // This method should be here to run the tests
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        var builder = Host.CreateDefaultBuilder()
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .UseSerilog()
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseStartup<TestStartup>()
                    .UseContentRoot(AssemblyUtils.GetAssemblyPath(typeof(ApiFileStorageAssemblyMarker).Assembly))
                    .ConfigureTestServices(services => 
                    {
                        services.AddHttpContextAccessor();
                        // We can further customize our application setup here.
                    })
                    .ConfigureAppConfiguration(builder =>
                    {
                        builder.ConfigureConfigurationProvider();
                    })
                    .UseTestServer();
            });
        return builder;
    }
}
