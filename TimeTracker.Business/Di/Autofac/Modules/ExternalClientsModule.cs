using Autofac;
using Domain.Abstractions;
using TimeTracker.Business.Notifications;
using TimeTracker.Business.Services.ExternalClients.ClickUp;

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
        }
    }
}
