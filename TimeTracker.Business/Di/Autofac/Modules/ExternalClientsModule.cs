using Autofac;
using Domain.Abstractions;
using TimeTracker.Business.Notifications;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Redmine;
using TimeTracker.Business.Services.ExternalClients.Jira;

namespace TimeTracker.Business.Di.Autofac.Modules
{
    public class ExternalClientsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ClickUpClient>()
                .As<IClickUpClient>()
                .InstancePerDependency();
            builder
                .RegisterType<RedmineClient>()
                .As<IRedmineClient>()
                .InstancePerDependency();
            builder
                .RegisterType<JiraClient>()
                .As<IJiraClient>()
                .InstancePerDependency();
        }
    }
}
