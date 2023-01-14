using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Persistence.Transactions.Behaviors;
using Serilog;
using Serilog.Extensions.Autofac.DependencyInjection;
using TimeTracker.Business;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Notifications.Services;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Redmine;
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
            builder.RegisterType<RedmineClientMock>()
                .As<IRedmineClient>()
                .SingleInstance();
        }

        _serviceProvider = builder.Build();
        Scope = _serviceProvider.BeginLifetimeScope();
        
        DbSessionProvider = Scope.Resolve<IDbSessionProvider>();
        EmailSendingServiceMock = Scope.Resolve<IEmailSendingService>() as EmailSendingServiceMock;
        
        _queueDao = Scope.Resolve<IQueueDao>();
        EmailSendingServiceMock.Reset();
    }

    #region Uploading

    protected IFormFile CreateFormFile(string fileName = "test.pdf")
    {
        var fileExtension = Path.GetExtension(fileName).Replace(".", "");
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
            
        var stubsPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        stubsPath = Path.GetDirectoryName(stubsPath);
        stubsPath = Path.Combine(stubsPath, "Stubs", "Image");
            
        fileExtension = fileExtension.Trim().ToLower();
        if (fileExtension == "jpg" || fileExtension == "jpeg")
        {
            var stubFileBytes = File.ReadAllBytes(
                Path.Combine(stubsPath, "ByExtensions", "image.jpg")
            );
            writer.Write(stubFileBytes);
        }
        else
        {
            var content = "Hello World from a Fake File";
            writer.Write(content);
        }
        writer.Flush();
        stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, "id_from_form", fileName);
    }

    #endregion
    
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
