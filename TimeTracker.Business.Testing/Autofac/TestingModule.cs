using Autofac;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;

namespace TimeTracker.Business.Testing.Autofac;

public class TestingModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterAssemblyTypes(typeof(BusinessTestingAssemblyMarker).Assembly)
            .AsClosedTypesOf(typeof(IDataFactory<>))
            .InstancePerDependency();
        
        builder
            .RegisterAssemblyTypes(typeof(BusinessTestingAssemblyMarker).Assembly)
            .As<IUserSeeder>()
            .InstancePerDependency();
    }
}
