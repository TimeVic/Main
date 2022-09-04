using Autofac;
using Microsoft.Extensions.Configuration;
using Persistence.Transactions.Behaviors;
using Serilog;
using Serilog.Extensions.Autofac.DependencyInjection;
using TimeTracker.Business;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Notifications.Services;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Testing;

namespace TimeTracker.Tests.Integration.Business.Core;

public class BaseTest: IDisposable
{
    protected readonly IDbSessionProvider DbSessionProvider;
    protected readonly FakeEmailSendingService EmailSendingService;
    protected readonly ILifetimeScope Scope;
    
    private readonly IContainer _serviceProvider;
    private readonly IQueueDao _queueDao;

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
        
        builder.RegisterAssemblyModules(
            typeof(BusinessAssemblyMarker).Assembly,
            typeof(BusinessTestingAssemblyMarker).Assembly
        );
        
        // Register fackers
        builder.RegisterType<FakeEmailSendingService>()
            .As<IEmailSendingService>()
            .InstancePerLifetimeScope();
        
        _serviceProvider = builder.Build();
        Scope = _serviceProvider.BeginLifetimeScope();
        
        DbSessionProvider = Scope.Resolve<IDbSessionProvider>();
        EmailSendingService = Scope.Resolve<IEmailSendingService>() as FakeEmailSendingService;
        
        _queueDao = Scope.Resolve<IQueueDao>();
        // Clear queue
        _queueDao.CompleteAllPending().Wait();
    }

    protected async Task CommitDbChanges()
    { 
        await DbSessionProvider.PerformCommitAsync();
        DbSessionProvider.CurrentSession.Clear();
    }
    
    public void Dispose()
    {
        Scope.Dispose();
        _serviceProvider.Dispose();
    }
}
