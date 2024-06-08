using Autofac;
using Microsoft.Extensions.Configuration;
using TimeTracker.Api;
using TimeTracker.Business;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Common.Services.Web.ReCaptcha;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Jira;
using TimeTracker.Business.Services.ExternalClients.Redmine;
using TimeTracker.Business.Testing;
using TimeTracker.Business.Testing.Extensions;

namespace TimeTracker.Tests.Integration.Api;

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
