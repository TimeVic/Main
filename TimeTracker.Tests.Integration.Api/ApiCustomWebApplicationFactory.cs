﻿using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Notifications.Services;

namespace TimeTracker.Tests.Integration.Api;

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
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseStartup<TestStartup>()
                    .UseSerilog()
                    .UseContentRoot(AssemblyUtils.GetAssemblyPath())
                    .ConfigureTestServices(services => 
                    {
                        services.AddHttpContextAccessor();
                        services.AddScoped<IEmailSendingService, FakeEmailSendingService>();
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
