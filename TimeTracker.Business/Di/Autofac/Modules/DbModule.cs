using Autofac;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Connection;

namespace TimeTracker.Business.Di.Autofac.Modules
{
    public class DbModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<DbConnectionFactory>()
                .As<IDbConnectionFactory>()
                .SingleInstance();

            builder
                .RegisterType<DbSessionProvider>()
                .As<IDbSessionProvider>()
                .InstancePerLifetimeScope();
        }
    }
}
