using Autofac;
using Domain.Abstractions;
using TimeTracker.Business.Common;

namespace TimeTracker.Business.Di.Autofac.Modules
{
    public class DomainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // builder
            //     .RegisterType<ThrottleRequestsService>()
            //     .As<IThrottleRequestsService>()
            //     .SingleInstance();

            builder
                .RegisterAssemblyTypes(typeof(BusinessAssemblyMarker).Assembly)
                .AssignableTo<IDomainService>()
                .AsImplementedInterfaces()
                .InstancePerDependency();

            // builder
            //     .RegisterAssemblyTypes(typeof(BusinessNotificationsAssemblyMarker).Assembly)
            //     .AssignableTo<IDomainService>()
            //     .AsImplementedInterfaces()
            //     .InstancePerDependency();
            
            builder
                .RegisterAssemblyTypes(typeof(BusinessCommonAssemblyMarker).Assembly)
                .AssignableTo<IDomainService>()
                .AsImplementedInterfaces()
                .InstancePerDependency();
        }
    }
}
