using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Api;
using TimeTracker.Business;
using TimeTracker.Business.Testing;

namespace TimeTracker.Tests.Integration.Api;

public class TestStartup: Startup
{
    public TestStartup(IConfiguration configuration) : base(configuration)
    {
    }
    
    public override void ConfigureContainer(ContainerBuilder builder)
    {
        base.ConfigureContainer(builder);
        builder.RegisterAssemblyModules(
            typeof(BusinessAssemblyMarker).Assembly,
            typeof(BusinessTestingAssemblyMarker).Assembly
        );
        
    }
}
