using Autofac;
using Microsoft.Extensions.Configuration;
using Persistence.Transactions.Behaviors;
using Serilog;
using Serilog.Extensions.Autofac.DependencyInjection;
using TimeTracker.Business;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Notifications.Services;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Testing;

namespace TimeTracker.Tests.Integration.Business.Core;

public abstract class BaseTest: IDisposable
{
    protected readonly IDbSessionProvider DbSessionProvider;
    protected readonly EmailSendingServiceMock EmailSendingServiceMock;
    protected readonly ILifetimeScope Scope;
    
    private readonly IContainer _serviceProvider;
    protected readonly IQueueDao _queueDao;

    protected bool IsFakeIntegrations = true;

    public BaseTest(bool isFakeIntegrations = true)
    {
        IsFakeIntegrations = isFakeIntegrations;
        
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
        builder.RegisterType<EmailSendingServiceMock>()
            .As<IEmailSendingService>()
            .InstancePerLifetimeScope();

        if (IsFakeIntegrations)
        {
            builder.RegisterType<ClickUpClientMock>()
                .As<IClickUpClient>()
                .SingleInstance();
        }

        _serviceProvider = builder.Build();
        Scope = _serviceProvider.BeginLifetimeScope();
        
        DbSessionProvider = Scope.Resolve<IDbSessionProvider>();
        EmailSendingServiceMock = Scope.Resolve<IEmailSendingService>() as EmailSendingServiceMock;
        
        _queueDao = Scope.Resolve<IQueueDao>();
        EmailSendingServiceMock.Reset();
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
