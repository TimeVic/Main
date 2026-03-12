using System.Text;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Persistence.Transactions.Behaviors;
using Serilog;
using Serilog.Extensions.Autofac.DependencyInjection;
using TimeTracker.Business;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.ExternalClients.ClickUp;
using TimeTracker.Business.Services.ExternalClients.Jira;
using TimeTracker.Business.Services.ExternalClients.Redmine;
using TimeTracker.Business.Testing;
using TimeTracker.Business.Testing.Services;

namespace TimeTracker.Tests.Integration.Business.Core;

public abstract class BaseTest: IDisposable
{
    protected readonly IDbSessionProvider DbSessionProvider;
    protected readonly SmtpClientServiceMock SmtpClientServiceMock;
    protected readonly FirebaseClientServiceMock FirebaseClientService;
    protected readonly ILifetimeScope Scope;
    
    private readonly IContainer _serviceProvider;
    protected readonly IQueueDao _queueDao;

    protected bool IsFakeIntegrations = true;
    private readonly IDbCleanUpService _dbCleanUpService;

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
        builder.RegisterType<SmtpClientServiceMock>()
            .As<ISmtpClientService>()
            .InstancePerLifetimeScope();
        builder.RegisterType<FirebaseClientServiceMock>()
            .As<IFirebaseClientService>()
            .InstancePerLifetimeScope();

        if (IsFakeIntegrations)
        {
            builder.RegisterType<ClickUpClientMock>()
                .As<IClickUpClient>()
                .SingleInstance();
            builder.RegisterType<RedmineClientMock>()
                .As<IRedmineClient>()
                .SingleInstance();
            builder.RegisterType<JiraClientMock>()
                .As<IJiraClient>()
                .SingleInstance();
        }

        _serviceProvider = builder.Build();
        Scope = _serviceProvider.BeginLifetimeScope();
        
        DbSessionProvider = Scope.Resolve<IDbSessionProvider>();
        _dbCleanUpService = Scope.Resolve<IDbCleanUpService>();
        _dbCleanUpService.CleanUp().Wait();
        
        SmtpClientServiceMock = (Scope.Resolve<ISmtpClientService>() as SmtpClientServiceMock)!;
        FirebaseClientService = (Scope.Resolve<IFirebaseClientService>() as FirebaseClientServiceMock)!;
        
        _queueDao = Scope.Resolve<IQueueDao>();
        SmtpClientServiceMock.Reset();
        FirebaseClientService.Reset();
    }

    #region Uploading

    protected IFormFile CreateFormFile(string fileName = "test.pdf", byte[]? fileBytes = null)
    {
        var fileExtension = Path.GetExtension(fileName).Replace(".", "");
        var stream = new MemoryStream();
            
        var stubsPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        stubsPath = Path.GetDirectoryName(stubsPath);
        stubsPath = Path.Combine(stubsPath, "stubs");
            
        if (fileBytes != null)
        {
            stream.Write(fileBytes);
        }
        else
        {
            var file = Path.Combine(stubsPath, fileName);
            if (File.Exists(file))
            {
                var stubFileBytes = File.ReadAllBytes(file);
                stream.Write(stubFileBytes);
            }
            else
            {
                var content = "Hello World from a Fake File";
                stream.Write(Encoding.UTF8.GetBytes(content));
            }
        }
        stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, "id_from_form", fileName);
    }

    #endregion
    
    protected async Task FlushDbChanges(bool isClearSession = false)
    { 
        await DbSessionProvider.CurrentSession.FlushAsync();
        if (isClearSession)
        {
            DbSessionProvider.CurrentSession.Clear();
        }
    }
    
    public void Dispose()
    {
        _serviceProvider.Dispose();
        Scope.Dispose();
    }
}
