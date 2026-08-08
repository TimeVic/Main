using Autofac;
using TimeTracker.Business.Logging.Client.GrayLog;

namespace TimeTracker.Business.Di.Autofac.Modules;

public sealed class LoggingModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<GraylogGelfClient>()
            .As<IGraylogGelfClient>()
            .SingleInstance();

        builder.RegisterType<GraylogClient>()
            .As<IGraylogClient>()
            .SingleInstance();
    }
}
