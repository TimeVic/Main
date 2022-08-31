using Autofac;
using Domain.Abstractions;
using TimeTracker.Business.Common;
using TimeTracker.Business.Orm;

namespace TimeTracker.Business.Di.Autofac.Modules
{
    public class DomainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(typeof(BusinessAssemblyMarker).Assembly)
                .AssignableTo<IDomainService>()
                .AsImplementedInterfaces()
                .InstancePerDependency();

            builder
                .RegisterAssemblyTypes(typeof(BusinessAssemblyMarker).Assembly)
                .AssignableTo<IScopedDomainService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder
                .RegisterAssemblyTypes(typeof(BusinessCommonAssemblyMarker).Assembly)
                .AssignableTo<IDomainService>()
                .AsImplementedInterfaces()
                .InstancePerDependency();
            
            builder
                .RegisterAssemblyTypes(typeof(BusinessCommonAssemblyMarker).Assembly)
                .AssignableTo<IScopedDomainService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            
            builder
                .RegisterAssemblyTypes(typeof(BusinessOrmAssemblyMarker).Assembly)
                .AssignableTo<IDomainService>()
                .AsImplementedInterfaces()
                .InstancePerDependency();
            
            builder
                .RegisterAssemblyTypes(typeof(BusinessOrmAssemblyMarker).Assembly)
                .AssignableTo<IScopedDomainService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}
