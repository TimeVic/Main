using Autofac;
using Notification.Abstractions;
using TimeTracker.Business.Notifications;

namespace TimeTracker.Business.Di.Autofac.Modules
{
    public class NotificationsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(typeof(BusinessNotificationsAssemblyMarker).Assembly)
                .AsClosedTypesOf(typeof(IAsyncNotification<>))
                .InstancePerLifetimeScope();

            builder
                .RegisterType<ScopedAsyncNotificationFactory>()
                .As<IAsyncNotificationFactory>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<DefaultAsyncNotificationBuilder>()
                .As<IAsyncNotificationBuilder>()
                .InstancePerLifetimeScope();
        }
    }
}
