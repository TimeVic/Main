using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Api;
using TimeTracker.Business;
using TimeTracker.Business.Common.Services.Web.ReCaptcha;
using TimeTracker.Business.Notifications.Services;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Redmine;
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
        builder.RegisterType<EmailSendingServiceMock>().As<IEmailSendingService>().InstancePerLifetimeScope();
        builder.RegisterType<ClickUpClientMock>().As<IClickUpClient>().InstancePerLifetimeScope();
        builder.RegisterType<RedmineClientMock>().As<IRedmineClient>().InstancePerLifetimeScope();
    }
}
