using Autofac;
using Domain.Abstractions;

namespace TimeTracker.Business.FileStorage.Di
{
    public class DomainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(typeof(BusinessFileStorageAssemblyMarker).Assembly)
                .AssignableTo<IDomainService>()
                .AsImplementedInterfaces()
                .InstancePerDependency();
            
            builder
                .RegisterAssemblyTypes(typeof(BusinessFileStorageAssemblyMarker).Assembly)
                .AssignableTo<IScopedDomainService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}
