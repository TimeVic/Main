using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Api.FileStorage;
using TimeTracker.Business.Testing.Extensions;

namespace TimeTracker.Tests.Integration.Api.FileStorage;

public class TestStartup: Startup
{
    public TestStartup(IConfiguration configuration) : base(configuration)
    {
    }

    public override void ConfigureContainer(ContainerBuilder builder)
    {
        base.ConfigureContainer(builder);
        builder.ConfigureTestingScope();
    }
}
