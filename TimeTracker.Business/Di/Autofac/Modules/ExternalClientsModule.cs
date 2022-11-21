using Autofac;
using Domain.Abstractions;
using TimeTracker.Business.Notifications;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Redmine;

namespace TimeTracker.Business.Di.Autofac.Modules
{
    public class ExternalClientsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ClickUpClient>()
                .As<IClickUpClient>()
                .SingleInstance();
            builder
                .RegisterType<RedmineClient>()
                .As<IRedmineClient>()
                .SingleInstance();
        }
    }
}
