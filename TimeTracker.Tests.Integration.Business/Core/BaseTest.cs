using Autofac;
using Microsoft.Extensions.Configuration;
using Persistence.Transactions.Behaviors;
using Serilog;
using Serilog.Extensions.Autofac.DependencyInjection;
using TimeTracker.Business;
using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Integration.Business.Core;

public class BaseTest: IDisposable
{
    protected readonly IDbSessionProvider DbSessionProvider;
    private readonly IContainer _serviceProvider;
    private readonly ILifetimeScope _scope;

    public BaseTest()
    {
        var configuration = ApplicationHelper.BuildConfiguration();
        var builder = new ContainerBuilder();
        builder.RegisterInstance(configuration)
            .As<IConfiguration>()
            .SingleInstance();
        var serilogConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration);
        builder.RegisterSerilog(serilogConfiguration);
        
        builder.RegisterAssemblyModules(typeof(BusinessAssemblyMarker).Assembly);
        _serviceProvider = builder.Build();
        _scope = _serviceProvider.BeginLifetimeScope();
        
        DbSessionProvider = _scope.Resolve<IDbSessionProvider>();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
    }
}
