using Autofac;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Common.Services.Web.ReCaptcha;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Jira;
using TimeTracker.Business.Services.ExternalClients.Redmine;

namespace TimeTracker.Business.Testing.Extensions;

public static class ContainerBuilderExtensions
{
    public static void ConfigureTestingScope(this ContainerBuilder builder)
    {
        builder.RegisterAssemblyModules(
            typeof(BusinessAssemblyMarker).Assembly,
            typeof(BusinessTestingAssemblyMarker).Assembly
        );
        builder.RegisterType<FakeReCaptchaService>().As<IReCaptchaService>().InstancePerDependency();
        builder.RegisterType<SmtpClientServiceMock>().As<ISmtpClientService>().InstancePerLifetimeScope();
        builder.RegisterType<FirebaseClientServiceMock>().As<IFirebaseClientService>().InstancePerLifetimeScope();
        builder.RegisterType<ClickUpClientMock>().As<IClickUpClient>().InstancePerLifetimeScope();
        builder.RegisterType<RedmineClientMock>().As<IRedmineClient>().InstancePerLifetimeScope();
        builder.RegisterType<JiraClientMock>().As<IJiraClient>().InstancePerLifetimeScope();
    }
}
