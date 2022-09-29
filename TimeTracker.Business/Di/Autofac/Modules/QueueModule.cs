using Autofac;
using Domain.Abstractions;
using TimeTracker.Business.Notifications;

namespace TimeTracker.Business.Di.Autofac.Modules
{
    public class QueueModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(typeof(BusinessNotificationsAssemblyMarker).Assembly)
                .AsClosedTypesOf(typeof(IAsyncQueueHandler<>))
                .InstancePerLifetimeScope();
        }
    }
}
