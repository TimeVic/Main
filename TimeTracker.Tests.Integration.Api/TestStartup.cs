using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Api;
using TimeTracker.Business;
using TimeTracker.Business.Common.Services.Web.ReCaptcha;
using TimeTracker.Business.Notifications.Services;
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
        builder.RegisterType<FakeReCaptchaService>().As<IReCaptchaService>().InstancePerDependency();
        builder.RegisterType<FakeEmailSendingService>().As<IEmailSendingService>().InstancePerLifetimeScope();
    }
}
